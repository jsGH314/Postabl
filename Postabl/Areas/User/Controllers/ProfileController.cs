using DAL;
using DAL.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace Postabl.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class ProfileController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IUnitOfWork unitOfWork, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _env = env;
        }

        // GET: User/Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_unitOfWork.Profile.Get(p => p.ApplicationUserId == userId));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var blogPostList = _unitOfWork.BlogPost.GetAll(b => b.ApplicationUserId == userId)
                .OrderByDescending(b => b.PublishedDate)
                .ToList();

            var likedByDisplayMap = new Dictionary<int, string>();
            foreach (var post in blogPostList)
            {
                likedByDisplayMap[post.Id] = ResolveLikedByDisplay(post.LikedBy);
            }

            UserProfileVM profileVM = new UserProfileVM()
            {
                User = _unitOfWork.ApplicationUser.Get(u => u.Id == userId),
                Profile = _unitOfWork.Profile.Get(p => p.ApplicationUserId == userId),
                BlogPostList = blogPostList,
                LikedByDisplayMap = likedByDisplayMap
            };

            return View(profileVM);
        }

        // Added this section to allow viewing another user's public profile by user id (public view)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            var profile = _unitOfWork.Profile.Get(p => p.ApplicationUserId == id);
            var posts = _unitOfWork.BlogPost.GetAll(b => b.ApplicationUserId == id && b.IsPublic)
                .OrderByDescending(b => b.PublishedDate)
                .ToList();

            var likedByDisplayMap = new Dictionary<int, string>();
            foreach (var post in posts)
            {
                likedByDisplayMap[post.Id] = ResolveLikedByDisplay(post.LikedBy);
            }

            var profileVM = new UserProfileVM
            {
                User = user ?? new ApplicationUser(),
                Profile = profile ?? new Profile(),
                BlogPostList = posts,
                LikedByDisplayMap = likedByDisplayMap
            };

            // Reuse the existing Profile view
            return View("Profile", profileVM);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ViewPublicProfile(int id)
        {
            var profile = _unitOfWork.Profile.Get(p => p.Id == id);

            if (profile == null) return NotFound();

            var userId = profile.ApplicationUserId;

            var posts = _unitOfWork.BlogPost.GetAll(b => b.ApplicationUserId == userId && b.IsPublic)
                .OrderByDescending(b => b.PublishedDate)
                .ToList();

            var likedByDisplayMap = new Dictionary<int, string>();
            foreach (var post in posts)
            {
                likedByDisplayMap[post.Id] = ResolveLikedByDisplay(post.LikedBy);
            }

            var profileVm = new UserProfileVM
            {
                User = profile.ApplicationUser ?? new ApplicationUser(),
                Profile = profile,
                BlogPostList = posts,
                LikedByDisplayMap = likedByDisplayMap
            };

            return View("ViewPublicProfile", profileVm);
        }

        // GET: User/Profile/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = _unitOfWork.Profile.Get(p => p.Id == id);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        // POST: User/Profile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Bio,DisplayName")] Profile profile)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            var existing = _unitOfWork.Profile.Get(p => p.Id == id);
            if (existing == null) return NotFound();

            // Only update allowed fields from the incoming model
            existing.Bio = profile.Bio;
            existing.DisplayName = profile.DisplayName;

            try
            {
                _unitOfWork.Profile.Update(existing);
                _unitOfWork.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_unitOfWork.Profile.Get(p => p.Id == id) == null) return NotFound();
                throw;
            }

            // Redirect to profile view (or Index) to avoid re-post issues
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(int id, IFormFile file)
        {
            return await UploadImage(id, file, ImageType.Profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCoverImage(int id, IFormFile file)
        {
            return await UploadImage(id, file, ImageType.Cover);
        }

        #region Private Helper Methods

        private enum ImageType
        {
            Profile,
            Cover
        }

        private async Task<IActionResult> UploadImage(int profileId, IFormFile file, ImageType imageType)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, error = "No file uploaded" });
            }

            var profile = _unitOfWork.Profile.Get(p => p.Id == profileId);
            if (profile == null)
            {
                return NotFound(new { success = false, error = "Profile not found" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename with original extension
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Generate URL with cache-busting query parameter
            var imageUrl = $"/uploads/profiles/{fileName}?v={DateTime.UtcNow.Ticks}";

            // Update the appropriate property based on image type
            switch (imageType)
            {
                case ImageType.Profile:
                    profile.ProfileImageUrl = imageUrl;
                    break;
                case ImageType.Cover:
                    profile.CoverImageUrl = imageUrl;
                    break;
            }

            _unitOfWork.Profile.Update(profile);
            _unitOfWork.Save();

            return Json(new { success = true, url = imageUrl });
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

        #endregion
    }
}
