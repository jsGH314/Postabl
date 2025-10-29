using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Postabl.Models;

namespace Postabl.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            IEnumerable<BlogPost> blogPosts = new List<BlogPost>
            {
                new BlogPost { Id = 1, Title = "First Post", Content = "This is the content of the first post.", Author = "Admin", PublishedDate = DateTime.Now },
                new BlogPost { Id = 2, Title = "Second Post", Content = "This is the content of the second post.", Author = "Admin", PublishedDate = DateTime.Now }
            };
            return View(blogPosts);
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
