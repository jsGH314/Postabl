using DAL;
using DAL.Repository;
using DAL.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Postabl.Areas.User.Controllers
{
    [Area("User")]
    public class BlogPostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public BlogPostController(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        // GET: User/BlogPost
        public async Task<IActionResult> Index()
        {
            return View(_unitOfWork.BlogPost.GetAll());
        }

        public async Task<IActionResult> ViewPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            var profile = _unitOfWork.Profile.Get(p => p.ApplicationUserId == blogPost.ApplicationUserId);

            // Generate avatar URL with initials
            var displayName = profile?.DisplayName ?? profile?.ApplicationUser?.Name ?? blogPost.Author ?? "User";
            var profileImageUrl = string.IsNullOrWhiteSpace(profile?.ProfileImageUrl)
                ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(displayName)}&background=random&color=fff&size=128"
                : profile.ProfileImageUrl;

            var isAuthor = blogPost.ApplicationUserId == User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Map to a view model for reading a post
            var postVM = new PostDetailsVM
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Content = blogPost.Content,
                Author = blogPost.Author,
                PublishedDate = blogPost.PublishedDate,
                Likes = blogPost.Likes,
                IsPublic = blogPost.IsPublic,
                IsAuthor = isAuthor,
                ProfileId = profile?.Id,
                ProfileImageUrl = profileImageUrl,
                LikedByDisplay = ResolveLikedByDisplay(blogPost.LikedBy)
            };

            return View(postVM);
        }

        // GET: User/BlogPost/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
        //    if (blogPost == null)
        //    {
        //        return NotFound();
        //    }

        //    var profile = _unitOfWork.Profile.Get(p => p.ApplicationUserId == blogPost.ApplicationUserId);

        //    // Generate avatar URL with initials
        //    var displayName = profile?.DisplayName ?? profile?.ApplicationUser?.Name ?? blogPost.Author ?? "User";
        //    var profileImageUrl = string.IsNullOrWhiteSpace(profile?.ProfileImageUrl)
        //        ? $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(displayName)}&background=random&color=fff&size=128"
        //        : profile.ProfileImageUrl;

        //    // Map to a view model for reading a post
        //    var postVM = new PostDetailsVM
        //    {
        //        Id = blogPost.Id,
        //        Title = blogPost.Title,
        //        Content = blogPost.Content,
        //        Author = blogPost.Author,
        //        PublishedDate = blogPost.PublishedDate,
        //        Likes = blogPost.Likes,
        //        IsPublic = blogPost.IsPublic,
        //        ProfileId = profile?.Id,
        //        ProfileImageUrl = profileImageUrl,
        //        LikedByDisplay = ResolveLikedByDisplay(blogPost.LikedBy)
        //    };

        //    return View(postVM);
        //}

        // GET: User/BlogPost/Create
        public IActionResult Create()
        {
            var blogPost = new BlogPost
            {
                PublishedDate = DateTime.Now
            };
            return View(blogPost);
        }

        // POST: User/BlogPost/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApplicationUserId,Title,Content,PublishedDate,Author,Likes,IsPublic")] BlogPost blogPost)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            if (ModelState.IsValid)
            {
                blogPost.ApplicationUserId = userId;
                blogPost.PublishedDate = DateTime.Now;
                blogPost.Author = user.Name;
                blogPost.Likes = 0;
                _unitOfWork.BlogPost.Add(blogPost);
                _unitOfWork.Save();
                return RedirectToAction("Profile", "Profile");
            }
            return RedirectToAction("Profile", "Profile");
        }

        // GET: User/BlogPost/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }
            return View(blogPost);
        }

        // POST: User/BlogPost/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,IsPublic")] BlogPost posted, bool isPublic)
        {
            if (id != posted.Id)
            {
                return NotFound();
            }

            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);

            if (ModelState.IsValid)
            {
                blogPost.Title = posted.Title;
                blogPost.Content = posted.Content;
                blogPost.IsPublic = isPublic;
            }

            try
            {
                _unitOfWork.BlogPost.Update(blogPost);
                _unitOfWork.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(blogPost.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(ViewPost), new { id = blogPost.Id });
        }

        // GET: User/BlogPost/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: User/BlogPost/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
            if (blogPost != null)
            {
                _unitOfWork.BlogPost.Remove(blogPost);
            }

            _unitOfWork.Save();
            return RedirectToAction("Profile", "Profile");
        }

        // POST: User/BlogPost/Like
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            // If user is not authenticated, redirect to Identity login with returnUrl back to the referring page
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    // Fallback to the post details page
                    returnUrl = Url.Action(nameof(ViewPost), "BlogPost", new { area = "User", id });
                }

                var loginUrl = $"/Identity/Account/Login?returnUrl={WebUtility.UrlEncode(returnUrl)}";
                return Redirect(loginUrl);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();

            var blogPost = _unitOfWork.BlogPost.Get(b => b.Id == id);
            if (blogPost == null) return NotFound();

            // parse LikedBy JSON into HashSet<string>
            var likedBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(blogPost.LikedBy))
            {
                try
                {
                    likedBy = JsonSerializer.Deserialize<HashSet<string>>(blogPost.LikedBy) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    // If deserialization fails, reset to empty set to avoid blocking users
                    likedBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }

            if (likedBy.Contains(userId))
            {
                // remove like
                likedBy.Remove(userId);
            }
            else
            {
                // add like
                likedBy.Add(userId);
            }

            // keep aggregate Likes in sync with the stored list
            blogPost.Likes = likedBy.Count;
            blogPost.LikedBy = JsonSerializer.Serialize(likedBy);

            _unitOfWork.BlogPost.Update(blogPost);
            _unitOfWork.Save();

            // Return to the referring page when possible (feed or post view)
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(ViewPost), new { id });
        }

        // POST: User/BlogPost/QuickPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // require logged-in user
        public async Task<IActionResult> QuickPost(string content, bool isPublic)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Content cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var blogPost = new BlogPost();

            if(ModelState.IsValid)
            {
                blogPost.ApplicationUserId = userId;
                blogPost.PublishedDate = DateTime.Now;
                blogPost.Author = user.Name;
                blogPost.Content = content;
                blogPost.Title = "Quick Post";
                blogPost.IsPublic = isPublic;
                blogPost.Likes = 0;
                _unitOfWork.BlogPost.Add(blogPost);
                _unitOfWork.Save();
                return RedirectToAction("Profile", "Profile");
            }
            return RedirectToAction("Profile", "Profile");
        }

        private bool BlogPostExists(int id)
        {
            return _unitOfWork.BlogPost.Get(b => b.Id == id) != null;
        }

        private string ResolveLikedByDisplay(string? likedByJson)
        {
            if (string.IsNullOrWhiteSpace(likedByJson))
            {
                return string.Empty;
            }

            List<string> ids;
            try
            {
                ids = JsonSerializer.Deserialize<List<string>>(likedByJson) ?? new List<string>();
            }
            catch
            {
                return string.Empty;
            }

            if (ids.Count == 0) return string.Empty;

            // Batch-fetch profiles and application users to avoid N+1 queries
            var profileList = _unitOfWork.Profile.GetAll().Where(p => p != null && ids.Contains(p.ApplicationUserId)).ToList();
            var profilesByUserId = profileList.ToDictionary(p => p.ApplicationUserId, p => p);

            var appUserList = _unitOfWork.ApplicationUser.GetAll().Where(u => u != null && ids.Contains(u.Id)).ToList();
            var appUsersById = appUserList.ToDictionary(u => u.Id, u => u);

            var names = new List<string>();
            foreach (var uid in ids)
            {
                if (profilesByUserId.TryGetValue(uid, out var prof) && !string.IsNullOrWhiteSpace(prof.DisplayName))
                {
                    names.Add(prof.DisplayName!);
                }
                else if (appUsersById.TryGetValue(uid, out var au) && !string.IsNullOrWhiteSpace(au.Name))
                {
                    names.Add(au.Name);
                }
                else
                {
                    names.Add("Someone");
                }
            }

            // Format: "Alice", "Alice and Bob", "Alice, Bob and 3 others"
            if (names.Count == 0) return string.Empty;
            if (names.Count == 1) return names[0];
            if (names.Count == 2) return $"{names[0]} and {names[1]}";
            if (names.Count <= 3) return string.Join(", ", names.Take(names.Count - 1)) + $" and {names.Last()}";
            return $"{names[0]}, {names[1]} and {names.Count - 2} others";
        }
    }
}
