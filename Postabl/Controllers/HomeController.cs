using DAL;
using DAL.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Postabl.Models;
using SQLitePCL;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Postabl.Controllers
{
    //This controller handles the home page (public feed of user posts) and privacy page.
    //This feed will show a list of blog posts from all users that have set their visibility to public.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
            _unitOfWork = new DAL.Repository.UnitOfWork(context);
        }

        // If the visitor is authenticated show the private feed (replaces Index for logged-in users).
        public IActionResult Index(int page = 1, int pageSize = 12)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                // call the private feed logic so logged-in users see all posts
                return PrivateFeed(page, pageSize);
            }

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
                //LikedByDetails = ResolveLikedByDisplay(p.LikedBy),
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

        // New: Private feed that shows every post (public and private). Requires authentication.
        [Authorize]
        public IActionResult PrivateFeed(int page = 1, int pageSize = 12)
        {
            // no IsPublic filter — show all posts
            var query = _context.BlogPosts
                        .OrderByDescending(b => b.PublishedDate);

            var posts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

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

            var vm = new PublicFeedVM
            {
                Posts = postCards,
                TotalPosts = query.Count(),
                Page = page,
                PageSize = pageSize,
                IsAuthenticated = true
            };

            // Render a dedicated view so you can tweak the header/labels separately from the public feed
            return View("PrivateFeed", vm);
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

        private (List<string> Names, string Summary) ResolveLikedByDisplay(string? likedByJson)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(likedByJson))
            {
                return (names, string.Empty);
            }

            List<string> ids;
            try
            {
                ids = JsonSerializer.Deserialize<List<string>>(likedByJson) ?? new List<string>();
            }
            catch
            {
                // malformed JSON: return empty results rather than throwing
                return (names, string.Empty);
            }

            if (ids.Count == 0) return (names, string.Empty);

            // Batch-fetch profiles and application users to avoid N+1 queries
            // IRepository.GetAll() used synchronously in this codebase
            var profileList = _unitOfWork.Profile.GetAll().Where(p => p != null && ids.Contains(p.ApplicationUserId)).ToList();
            var profilesByUserId = profileList.ToDictionary(p => p.ApplicationUserId, p => p);

            var appUserList = _unitOfWork.ApplicationUser.GetAll().Where(u => u != null && ids.Contains(u.Id)).ToList();
            var appUsersById = appUserList.ToDictionary(u => u.Id, u => u);

            foreach (var uid in ids)
            {
                if (profilesByUserId.TryGetValue(uid, out var prof) && !string.IsNullOrWhiteSpace(prof.DisplayName))
                {
                    names.Add(prof.DisplayName!);
                    continue;
                }

                if (appUsersById.TryGetValue(uid, out var au) && !string.IsNullOrWhiteSpace(au.Name))
                {
                    names.Add(au.Name);
                    continue;
                }

                names.Add("Unknown");
            }

            string summary;
            if (names.Count == 0) summary = string.Empty;
            else if (names.Count <= 3) summary = string.Join(", ", names);
            else summary = $"{names[0]}, {names[1]} and {names.Count - 2} others";

            return (names, summary);
        }
    }
}
