using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TAK_Access_Manager.Models;
using TAK_Access_Manager.Models.ViewModels;

namespace TAK_Access_Manager.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly TakDbContext _context;

        public HomeController(TakDbContext context, ILogger<HomeController> logger, HttpClient httpClient, IConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClient;
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                IndexViewModel viewModel = GetIndexViewModel();
                var isAgAdmin = User.IsInRole("TAKAM-ADMINS-ROLE");
                if (!(isAgAdmin) || !(viewModel.user.Active))
                    return RedirectToAction("UserView", "User");
                else
                    return View(viewModel);
            }
            else
                return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public IndexViewModel GetIndexViewModel()
        {
            IndexViewModel modelReturn = new IndexViewModel();
            modelReturn.userList = _context.TakUsers.ToList();
            modelReturn.groupList = _context.TakGroups.ToList();

            Guid userObjectID;
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userObjectIDClaim = claimsIdentity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            var userNameClaim = claimsIdentity.FindFirst("preferred_username").Value;
            var indexOfAt = userNameClaim.LastIndexOf("@");
            var domain = userNameClaim.Substring(indexOfAt+1, (userNameClaim.Length-indexOfAt-1));

            var agency = _context.TakAgencies.Where(x => x.Domain == domain).First();

            if (userObjectIDClaim != null)
            {
                userObjectID = Guid.Parse(userObjectIDClaim.Value);
            }
            else
                userObjectID = Guid.NewGuid();
            modelReturn.user.UserId = userObjectID;

            TakUser currentUsr = new TakUser();


            var userExists = _context.TakUsers?.Any(x => x.UserId == userObjectID);
            if (userExists == null || !(bool)userExists)
            {
                var defaultGroup = _context.TakGroups.First(x => x.GroupId == agency.DefaultGroupId);
                TakUser newUser = new TakUser()
                {
                    UserId = userObjectID,
                    UserName = userNameClaim,
                    LastLogon = DateTime.Now,
                    PrimaryGroup = defaultGroup.GroupId,
                    CreatedOn = DateTime.Now,
                    LastModified = DateTime.Now,
                    Active = true
                };
                modelReturn.user = newUser;
                _context.TakUsers.Add(newUser);
                _context.SaveChanges();
            }
            else
            {
                var user = _context.TakUsers.Find(userObjectID);
                user.LastLogon = DateTime.Now;
                modelReturn.user = user;
                _context.SaveChanges();
            }

            return modelReturn;
        }
    }
}