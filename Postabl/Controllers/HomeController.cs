using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Postabl.Models;
using SQLitePCL;
using System.Diagnostics;
using System.Security.Claims;

namespace Postabl.Controllers
{
    //This controller handles the home page (public feed of user posts) and privacy page.
    //This feed will show a list of blog posts from all users that have set their visibility to public.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {

            IEnumerable<BlogPost> blogPosts = new List<BlogPost>
            {
                new BlogPost { Id = 1, Title = "First Post", Content = "This is the public content of the first post.", Author = "Admin", PublishedDate = DateTime.Now, IsPublic = true },
                new BlogPost { Id = 2, Title = "Second Post", Content = "This is the public content of the second post.", Author = "Admin", PublishedDate = DateTime.Now, IsPublic = true }
            };
            return View(blogPosts);
        }

        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_context.Profiles.FirstOrDefault(x => x.ApplicationUserId == userId));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
