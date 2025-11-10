using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
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

        public IActionResult Index(int page = 1, int pageSize = 12)
        {
            var query = _context.BlogPosts
                        .Where(b => b.IsPublic)
                        .OrderByDescending(b => b.PublishedDate);

            var posts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // use ToListAsync() if you add async EF Core call

            var userIds = posts.Select(p => p.ApplicationUserId).Where(id => id != null).Distinct().ToList();
            var profiles = _context.Profiles
                .Where(p => userIds.Contains(p.ApplicationUserId))
                .ToDictionary(p => p.ApplicationUserId, p => p);


            var postCards = posts.Select(p => new PostCardVM
            {
                Post = p,
                ProfileImageUrl = profiles.TryGetValue(p.ApplicationUserId, out var prof) && !string.IsNullOrWhiteSpace(prof.ProfileImageUrl)
                    ? prof.ProfileImageUrl
                    : "/images/placeholder-profile.png",
                Profile = profiles.TryGetValue(p.ApplicationUserId, out var profile) ? profile : new Profile()
            }).ToList();

            var PublicFeedVM = new PublicFeedVM
            {
                Posts = postCards,
                TotalPosts = query.Count(),
                Page = page,
                PageSize = pageSize,
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false
            };

            return View(PublicFeedVM);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            UserProfileVM profileVM = new UserProfileVM()
            {
                User = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId),
                Profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ApplicationUserId == userId),
                BlogPostList = await _context.BlogPosts.Where(b => b.ApplicationUserId == userId).ToListAsync()
            };

            return View(profileVM);
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
