using Microsoft.AspNetCore.Mvc;
using TAK_Access_Manager.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Data;
using AzureStorage;
using Microsoft.AspNetCore.Authorization;
using TAK_Access_Manager.Models.ViewModels;

namespace TAK_Access_Manager.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "TAKAM-ADMINS-ROLE")]
    public class GroupController : Controller
    {
        private readonly TakDbContext _context;
        public IConfiguration _config { get; }
        public IAuthorizationService _authorizationService { get; set; }
        public GroupController(TakDbContext context, IConfiguration configuration, IAuthorizationService authService)
        {
            _context = context;
            _config = configuration;
            _authorizationService = authService;
        }
        [HttpGet("{groupId}")]
        public IActionResult GroupView(int groupId)
        {           
            if (User.Identity?.IsAuthenticated == true)
            {
                GroupViewModel viewModel = GetGroupViewModel(groupId);

                return View(viewModel);

            }
            else
                return View();

        }

        [HttpGet("{userId}")]
        public async Task<List<Configuration>> GetConfigs(Guid userId)
        {
            await GetProcessedConfigs(userId);

            List<Configuration> AllConfigs = _context.Configurations.Where(x => x.UserId == userId).ToList();

            return AllConfigs;
        }

        [HttpPost]
        public async Task<IActionResult> PostConfig([FromForm] Configuration config)
        {

            Guid userObjectID;
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

            if (userObjectIDClaim != null)
                userObjectID = Guid.Parse(userObjectIDClaim.Value);
            else
                userObjectID = Guid.NewGuid();

            List<Configuration> _data = new List<Configuration>();
            _data.Add(new Configuration()
            {
                Subject = config.Subject,
                GroupIds = config.GroupIds,
                ConfiguredBy = config.ConfiguredBy,
                ConfigureDate = DateTime.Now,
                ExpirationDate = null,
                Notes = config.Notes,
                Server = config.Server,
                StatusCid = null,
                PackageId = null,
                UserId = userObjectID,
                FileName = null,
                BlobPath = null,
                ConfigType = 1
            });

            string json = System.Text.Json.JsonSerializer.Serialize(_data);
            var systemPath = System.Environment.
                             GetFolderPath(
                                 Environment.SpecialFolder.CommonApplicationData
                             );
            var complete = Path.Combine(systemPath, @"files");

            if (!Directory.Exists(complete))
                Directory.CreateDirectory(complete);

            foreach (var file in _data)
            {
                var add = _context.Configurations.Add(file);
                var saveChanges = _context.SaveChanges();
                await using FileStream createStream = System.IO.File.Create(complete + $"\\{file.Subject}.json");
                await System.Text.Json.JsonSerializer.SerializeAsync(createStream, _data);
                createStream.Close();

                using (var stream = System.IO.File.OpenRead(complete + $"\\{file.Subject}.json"))
                {
                    BlobStorage blobStorage = new BlobStorage(_config);
                    var blockBlob = blobStorage.AuthBlob().GetBlockBlobReference($"NewConfigurations/{file.Subject}.json");
                    await blobStorage.UploadToBlobStorage(blockBlob, stream);
                }


            }
            return Ok();
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<bool>> GetProcessedConfigs(Guid? uid)
        {

            List<Configuration> configsToDownload = await _context.Configurations.Where(m => m.UserId == uid && m.BlobPath == null).ToListAsync();
            foreach (var cfg in configsToDownload)
            {
                BlobStorage blobStorage = new BlobStorage(_config);

                var blob = blobStorage.AuthBlob().GetBlobReference("ProcessedConfigs/" + cfg.Subject + ".json");
                var exists = blob.Exists();
                if (exists)
                {
                    var source = blob.OpenRead();
                    var trimmedFileName = blob.Name.ToString().Split('/').Last();
                    var parsedFile = System.Text.Json.JsonSerializer.Deserialize<List<Configuration>>(source);
                    foreach (var f in parsedFile)
                    {
                        if (f.BlobPath != null)
                        {
                            cfg.BlobPath = f.BlobPath;
                            cfg.FileName = f.FileName;
                            _context.Update(cfg);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        else                   
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public GroupViewModel GetGroupViewModel(int gid)
        {
            GroupViewModel modelReturn = new GroupViewModel();

            Guid userObjectID;
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            var userNameClaim = claimsIdentity.FindFirst("preferred_username").Value;
            if (userObjectIDClaim != null)
                userObjectID = Guid.Parse(userObjectIDClaim.Value);
            else
                userObjectID = Guid.NewGuid();
            modelReturn.user.UserId = userObjectID;

            TakUser currentUsr = new TakUser();


            var userExists = _context.TakUsers?.Any(x => x.UserId == userObjectID);
            if (userExists == null || !(bool)userExists)
            {
                return null;
            }
            else
            {
                var user = _context.TakUsers.Find(userObjectID);
                modelReturn.user = _context.TakUsers.Find(userObjectID);
            }

            modelReturn.fileList = _context.DataPackages.Where(x => x.GroupIds == gid.ToString()).ToList();
            modelReturn.groupDetails = _context.TakGroups.Find(gid);
            modelReturn.groupList = _context.TakGroups.ToList();
            return modelReturn;
        }

    }
}
