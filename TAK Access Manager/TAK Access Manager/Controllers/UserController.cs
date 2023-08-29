using Microsoft.AspNetCore.Mvc;
using TAK_Access_Manager.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Data;
using AzureStorage;
using Microsoft.AspNetCore.Authorization;
using TAK_Access_Manager.Models.ViewModels;
using System.Text.RegularExpressions;

namespace TAK_Access_Manager.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly TakDbContext _context;
        public IConfiguration _config { get; }
        public ILogger<UserController> _Logger;
        public UserController(TakDbContext context, IConfiguration configuration, ILogger<UserController> logger)
        {
            _context = context;
            _config = configuration;
            _Logger = logger;
        }

        public IActionResult UserView()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                UserViewModel viewModel = GetUserViewModel();
                return View(viewModel);
            }
            else
                return View();
        }

        [Authorize(Roles = "TAKAM-ADMINS-ROLE, TAKAM-USERS-ROLE")]
        public IActionResult AccountSettingsView()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                UserViewModel viewModel = GetUserViewModel();
                return View(viewModel);
            }
            else
                return View();
        }

        [Authorize(Roles = "TAKAM-ADMINS-ROLE, TAKAM-USERS-ROLE")]
        [HttpGet("{userId}")]
        public async Task<List<Configuration>> GetConfigs(Guid userId)
        {
            List<Configuration> AllConfigs = _context.Configurations.FromSqlRaw($"select * from Configurations nolock where UserId = '{userId}'").ToList();
            return AllConfigs;
        }

        [Authorize(Roles = "TAKAM-ADMINS-ROLE, TAKAM-USERS-ROLE")]
        [HttpPost]
        public async Task<IActionResult> PostConfig([FromForm] Configuration config)
        {
            try
            {
                if (config.ConfigType == 2)
                {
                    // get signed in user info
                    Guid userObjectID;
                    var claimsIdentity = User.Identity as ClaimsIdentity;

                    var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                    if (userObjectIDClaim != null)
                        userObjectID = Guid.Parse(userObjectIDClaim.Value);
                    else
                        userObjectID = Guid.NewGuid();

                    var dataPkg = _context.DataPackages.Where(x => x.PackageName == config.Subject).FirstOrDefault();
                    dataPkg.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Requested").Select(q => q.StatusCode).FirstOrDefault();
                    // create json object
                    List<Configuration> _data = new List<Configuration>();
                    var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    var utcNow = DateTime.UtcNow;
                    var pacificNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);

                    _data.Add(new Configuration()
                    {
                        Subject = config.Subject,
                        GroupIds = dataPkg.GroupIds,
                        ConfiguredBy = config.ConfiguredBy,
                        ConfigureDate = pacificNow,
                        Notes = "Revoked on " + pacificNow.ToString(),
                        Server = dataPkg.Server,
                        StatusCid = dataPkg.StatusCid,
                        PackageId = dataPkg.PackageId,
                        UserId = userObjectID,
                        FileName = dataPkg.PackageName,
                        BlobPath = dataPkg.BlobPath,
                        ConfigType = config.ConfigType,
                    });

                    string json = System.Text.Json.JsonSerializer.Serialize(_data);
                    var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    var complete = Path.Combine(systemPath, @"files");

                    if (!Directory.Exists(complete))
                        Directory.CreateDirectory(complete);

                    foreach (var file in _data)
                    {
                        // add to Db
                        var update = _context.DataPackages.Update(dataPkg);
                        var add = _context.Configurations.Add(file);
                        var saveChanges = _context.SaveChanges();

                        // create file
                        await using FileStream createStream = System.IO.File.Create(complete + $"\\{file.Subject}.json");
                        await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
                        createStream.Close();

                        // upload to blob storage
                        using (var stream = System.IO.File.OpenRead(complete + $"\\{file.Subject}.json"))
                        {
                            BlobStorage blobStorage = new BlobStorage(_config);
                            var blockBlob = blobStorage.AuthBlob().GetBlockBlobReference($"NewConfigurations/{file.Subject}.json");
                            await blobStorage.UploadToBlobStorage(blockBlob, stream);
                        }
                    }
                    return Ok();
                }
                else if (config.ConfigType == 5)
                {
                    // get last config
                    string filenameOrig;
                    var filenam = config.FileName;
                    var relConfigs = _context.Configurations.Where(x => x.PackageId == Int32.Parse(config.Subject)).ToList();
                    if (!relConfigs.Any())
                    {
                        foreach (var c in _context.Configurations.ToList())
                        {
                            if (Regex.IsMatch(c.Subject, filenam.Substring(0, 8)))
                                relConfigs.Add(c);
                        }
                    }

                    var cntOfReissues = _context.Configurations.Where(x => x.Subject == config.Subject && x.ConfigType == 5).ToList();

                    if (!cntOfReissues.Any())
                    {
                        foreach (var c in _context.Configurations.ToList())
                        {
                            if (Regex.IsMatch(c.Subject, filenam.Substring(0, 8)) && c.ConfigType == 5)
                                cntOfReissues.Add(c);
                        }
                    }

                    var lastConfig = relConfigs.OrderByDescending(x => x.ConfigureDate).FirstOrDefault();
                    var lastCfgDataPkg = _context.DataPackages.Find(lastConfig.PackageId);
                    if (lastCfgDataPkg != null)
                    {
                        lastCfgDataPkg.Renewed = true;
                        _context.Update(lastCfgDataPkg);
                    }
                    else
                    {
                        foreach (var c in _context.DataPackages)
                        {
                            if (Regex.IsMatch(c.PackageName, filenam.Substring(0, 8)) && c.Renewed == false)
                            {
                                c.Renewed = true;
                                _context.Update(c);
                            }
                        }
                    }

                    var versionNo = "";
                    var oldSubj = "";
                    if (cntOfReissues.Count() == 0 || cntOfReissues == null && lastCfgDataPkg.ParentPkgId == null)
                    {
                        versionNo = "R1";
                        oldSubj = lastConfig.Subject;
                    }
                    else
                    {
                        int lastIndex = lastConfig.Subject.LastIndexOf('-') + 1;
                        versionNo = "R" + (Int32.Parse(lastConfig.Subject.Substring(lastIndex + 1)) + 1).ToString();
                        oldSubj = lastConfig.Subject.Substring(0, (lastIndex - 1));
                    }

                    // get signed in user info
                    Guid userObjectID;
                    var claimsIdentity = User.Identity as ClaimsIdentity;

                    var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                    if (userObjectIDClaim != null)
                    {
                        userObjectID = Guid.Parse(userObjectIDClaim.Value);
                    }
                    else
                        userObjectID = Guid.NewGuid();

                    // create json object
                    List<Configuration> _data = new List<Configuration>();

                    var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    var utcNow = DateTime.UtcNow;
                    var pacificNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);

                    _data.Add(new Configuration()
                    {
                        Subject = oldSubj + "-" + versionNo,
                        GroupIds = lastConfig.GroupIds,
                        ConfiguredBy = lastConfig.ConfiguredBy,
                        ConfigureDate = pacificNow,
                        ExpirationDate = @DateTime.Today.AddDays(Int32.Parse(_config.GetSection("CertConfig")["CertLifetimeDays"])),
                        Notes = "Reissued from file: " + lastConfig.FileName,
                        Server = lastConfig.Server,
                        StatusCid = 1,
                        PackageId = null,
                        UserId = userObjectID,
                        FileName = null,
                        BlobPath = null,
                        ConfigType = 5
                    });

                    DataPackage pkg = new DataPackage()
                    {
                        PackageName = oldSubj + "-" + versionNo,
                        GroupIds = lastConfig.GroupIds,
                        Server = lastConfig.Server,
                        Notes = "Reissued from file: " + lastConfig.FileName,
                        BlobPath = null,
                        StatusCid = _context.PkgStatusCodes.Where(x => x.StatusDescription == "Requested").Select(y => y.StatusCode).FirstOrDefault(),
                        UserId = userObjectID,
                        ExpirationDate = DateTime.Today.AddDays(Int32.Parse(_config.GetSection("CertConfig")["CertLifetimeDays"])),
                        ConfigureDt = pacificNow,
                        ParentPkgId = lastConfig.PackageId,
                        Renewed = false,
                    };

                    string json = System.Text.Json.JsonSerializer.Serialize(_data);
                    var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    var complete = Path.Combine(systemPath, @"files");

                    if (!Directory.Exists(complete))
                        Directory.CreateDirectory(complete);

                    foreach (var file in _data)
                    {
                        // add to Db
                        var addPkg = _context.DataPackages.Add(pkg);
                        var addConfig = _context.Configurations.Add(file);
                        var saveChanges = _context.SaveChanges();

                        // create file
                        await using FileStream createStream = System.IO.File.Create(complete + $"\\{file.Subject}.json");
                        await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
                        createStream.Close();

                        // upload to blob storage
                        using (var stream = System.IO.File.OpenRead(complete + $"\\{file.Subject}.json"))
                        {
                            BlobStorage blobStorage = new BlobStorage(_config);
                            var blockBlob = blobStorage.AuthBlob().GetBlockBlobReference($"NewConfigurations/{file.Subject}.json");
                            await blobStorage.UploadToBlobStorage(blockBlob, stream);
                        }
                    }

                    return Ok();

                }
                else if (config.ConfigType == 1)
                {
                    // get assigned groups
                    var groupIds = _context.TakGroups.ToList();
                    string newGroupIds = "";
                    foreach (var group in groupIds)
                    {
                        if (config.GroupIds == group.GroupName)
                            newGroupIds += group.GroupId;
                    }

                    // get signed in user info
                    Guid userObjectID;
                    var claimsIdentity = User.Identity as ClaimsIdentity;

                    var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                    if (userObjectIDClaim != null)
                        userObjectID = Guid.Parse(userObjectIDClaim.Value);
                    else
                        userObjectID = Guid.NewGuid();

                    // create json object
                    List<Configuration> _data = new List<Configuration>();
                    var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    var utcNow = DateTime.UtcNow;
                    var pacificNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);

                    _data.Add(new Configuration()
                    {
                        Subject = config.Subject,
                        GroupIds = newGroupIds,
                        ConfiguredBy = config.ConfiguredBy,
                        ConfigureDate = pacificNow,
                        ExpirationDate = config.ExpirationDate,
                        Notes = config.Notes,
                        Server = config.Server,
                        StatusCid = 1,
                        PackageId = null,
                        UserId = userObjectID,
                        FileName = null,
                        BlobPath = null,
                        ConfigType = 1
                    });

                    DataPackage pkg = new DataPackage()
                    {
                        PackageName = config.Subject,
                        GroupIds = newGroupIds,
                        Server = config.Server,
                        Notes = config.Notes,
                        BlobPath = config.BlobPath,
                        StatusCid = _context.PkgStatusCodes.Where(x => x.StatusDescription == "Requested").Select(y => y.StatusCode).FirstOrDefault(),
                        UserId = userObjectID,
                        ExpirationDate = config.ExpirationDate,
                        ConfigureDt = pacificNow,
                        Renewed = false,
                    };

                    string json = System.Text.Json.JsonSerializer.Serialize(_data);
                    var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    var complete = Path.Combine(systemPath, @"files");

                    if (!Directory.Exists(complete))
                    {
                        Directory.CreateDirectory(complete);
                    }
                    foreach (var file in _data)
                    {
                        // add to Db
                        var addPkg = _context.DataPackages.Add(pkg);
                        var addConfig = _context.Configurations.Add(file);
                        var saveChanges = _context.SaveChanges();

                        // create file
                        await using FileStream createStream = System.IO.File.Create(complete + $"\\{file.Subject}.json");
                        await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
                        createStream.Close();

                        // upload to blob storage
                        using (var stream = System.IO.File.OpenRead(complete + $"\\{file.Subject}.json"))
                        {
                            BlobStorage blobStorage = new BlobStorage(_config);
                            var blockBlob = blobStorage.AuthBlob().GetBlockBlobReference($"NewConfigurations/{file.Subject}.json");
                            await blobStorage.UploadToBlobStorage(blockBlob, stream);
                        }
                    }
                    return Ok();

                }

            }
            catch (ArgumentException e)
            {
                _Logger.LogError(e.InnerException?.Message);
                _Logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
            return BadRequest();
        }


        [Authorize(Roles = "TAKAM-ADMINS-ROLE, TAKAM-USERS-ROLE")]
        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            DataPackage pkgToDownload = await _context.DataPackages.FirstOrDefaultAsync(m => m.PackageId == id);

            BlobStorage blobStorage = new BlobStorage(_config);

            var blob = blobStorage.AuthBlob().GetBlobReference("ConnectionPackages/" + pkgToDownload.PackageName + "/" + pkgToDownload.PackageName + ".zip");
            var source = blob.OpenRead();

            var trimmedFileName = blob.Name.ToString().Split('/').Last();
            return File(source, "application/zip", trimmedFileName);
        }

        [HttpGet("{uid}")]
        public bool GetProcessedConfigs(Guid? uid)
        {
            List<Configuration> configsToDownload = _context.Configurations.Where(m => m.UserId == uid && m.StatusCid == 1).ToList();
            foreach (var cfg in configsToDownload)
            {
                BlobStorage blobStorage = new BlobStorage(_config);
                var blob = blobStorage.AuthBlob().GetBlobReference("RevokedPackages/RVK-" + cfg.Subject + ".json");
                var blobCreated = blobStorage.AuthBlob().GetBlobReference("ProcessedConfigs/" + cfg.Subject + ".json"); ;
                var existsRevoked = blob.Exists();
                var existsCreated = blobCreated.Exists();
                var existsExpired = cfg.ExpirationDate <= DateTime.UtcNow;

                if (existsRevoked)
                {
                    var source = blob.OpenRead();
                    var trimmedFileName = blob.Name.ToString().Split('/').Last();
                    var parsedFile = System.Text.Json.JsonSerializer.Deserialize<List<Configuration>>(source);
                    foreach (var f in parsedFile)
                    {
                        if (f.ConfigId == cfg.ConfigId && f.BlobPath != null && f.ConfigType == _context.CfgTypeCodes.Where(t => t.TypeDescription == "Revoke").Select(t => t.TypeCode).FirstOrDefault())
                        {
                            cfg.BlobPath = f.BlobPath;
                            cfg.FileName = f.FileName;
                            cfg.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Revoked").Select(q => q.StatusCode).FirstOrDefault();

                            var updatedFile = _context.DataPackages.Where(m => m.UserId == uid && m.PackageId == f.PackageId).FirstOrDefault();
                            updatedFile.BlobPath = null;
                            updatedFile.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Revoked").Select(q => q.StatusCode).FirstOrDefault();
                            updatedFile.PackageName = cfg.FileName;
                            updatedFile.Notes = cfg.Notes;

                            _context.Update(cfg);
                            _context.Update(updatedFile);
                            _context.SaveChanges();
                            return true;
                        }
                    }
                }
                else if (existsCreated)
                {
                    var source = blobCreated.OpenRead();
                    var trimmedFileName = blobCreated.Name.ToString().Split('/').Last();
                    var parsedFile = System.Text.Json.JsonSerializer.Deserialize<List<Configuration>>(source);
                    foreach (var f in parsedFile)
                    {
                        if (f.ConfigId == cfg.ConfigId && f.BlobPath != null && !(_context.Configurations.Any(x => x.ConfigType == 2 && x.Subject == f.Subject)) && f.ConfigType == _context.CfgTypeCodes.Where(t => t.TypeDescription == "UserCreate").Select(t => t.TypeCode).FirstOrDefault())
                        {
                            cfg.BlobPath = f.BlobPath;
                            cfg.FileName = f.FileName;
                            cfg.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Available").Select(q => q.StatusCode).FirstOrDefault();

                            var updatedFile = _context.DataPackages.Where(m => m.UserId == uid && m.BlobPath == null).FirstOrDefault();
                            updatedFile.BlobPath = cfg.BlobPath;
                            updatedFile.PackageName = cfg.FileName;
                            updatedFile.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Available").Select(q => q.StatusCode).FirstOrDefault();

                            _context.Update(cfg);
                            _context.Update(updatedFile);
                            _context.SaveChanges();
                            return true;
                        }
                        else if (f.ConfigId == cfg.ConfigId && f.BlobPath != null && f.ConfigType == _context.CfgTypeCodes.Where(t => t.TypeDescription == "Renew").Select(t => t.TypeCode).FirstOrDefault())
                        {
                            cfg.BlobPath = f.BlobPath;
                            cfg.FileName = f.FileName;
                            cfg.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Available").Select(q => q.StatusCode).FirstOrDefault();

                            var updatedFile = _context.DataPackages.Where(m => m.PackageName == f.FileName && m.BlobPath == null && m.UserId == uid && m.StatusCid == 1).FirstOrDefault();
                            updatedFile.BlobPath = cfg.BlobPath;
                            updatedFile.PackageName = cfg.FileName;
                            updatedFile.StatusCid = _context.PkgStatusCodes.Where(w => w.StatusDescription == "Available").Select(q => q.StatusCode).FirstOrDefault();

                            _context.Update(cfg);
                            _context.Update(updatedFile);
                            _context.SaveChanges();
                            return true;
                        }
                    }
                }
            }

            List<DataPackage> expiredCerts = _context.DataPackages.Where(e => e.UserId == uid && e.StatusCid != 4 && e.ExpirationDate <= DateTime.Now).ToList();
            foreach (var cert in expiredCerts)
            {
                cert.StatusCid = 4;
                _context.Update(cert);
                _context.SaveChanges();
            }
            return false;
        }

        
        public UserViewModel GetUserViewModel()
        {
            var hasRoleAdmin = User.IsInRole("TAKAM-ADMINS-ROLE");
            var hasRoleAccess = User.IsInRole("TAKAM-USERS-ROLE");
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var utcNow = DateTime.UtcNow;
            var pacificNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);

            UserViewModel modelReturn = new UserViewModel();

            Guid userObjectID;
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            var userNameClaim = claimsIdentity.FindFirst("preferred_username").Value;
            if (userObjectIDClaim != null)
            {
                userObjectID = Guid.Parse(userObjectIDClaim.Value);
                var t = GetProcessedConfigs(userObjectID);
            }
            else
                userObjectID = Guid.NewGuid();
            modelReturn.user.UserId = userObjectID;

            TakUser currentUsr = new TakUser();


            var userExists = _context.TakUsers?.Any(x => x.UserId == userObjectID);
            if (userExists == null || !(bool)userExists)
            {
                var defaultGroup = _context.TakGroups.First(x => x.GroupName == "ANON");
                TakUser newUser = new TakUser()
                {
                    UserId = userObjectID,
                    UserName = userNameClaim,
                    LastLogon = pacificNow,
                    PrimaryGroup = defaultGroup.GroupId,
                    CreatedOn = pacificNow,
                    LastModified = pacificNow
                };
                modelReturn.user = newUser;

                _context.TakUsers.Add(newUser);
                _context.SaveChanges();
            }
            else
            {
                var user = _context.TakUsers.Find(userObjectID);
                user.LastLogon = pacificNow;
                modelReturn.user = user;
                _context.SaveChanges();
            }

            modelReturn.fileList = _context.DataPackages.Any() == null ? null : _context.DataPackages.Where(x => x.UserId == userObjectID).OrderByDescending(y => y.ConfigureDt).ToList();
            modelReturn.groupDetails = _context.TakGroups.Find(modelReturn.user.PrimaryGroup);
            modelReturn.groupList = _context.TakGroups.ToList();
            modelReturn.IsAdmin = User.IsInRole("TAKAM-ADMINS-ROLE");
            return modelReturn;
        }
    }
}
