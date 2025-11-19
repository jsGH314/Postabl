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
using System.Threading.Tasks;
using Utility;

namespace Postabl.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class ProfileController : Controller
    {
        //private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IUnitOfWork unitOfWork, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _env = env;
        }
        //public ProfileController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
        //This will be the main dashboard for users to see their profile, blog posts, and other relevant information.
        [Authorize]
        //Views all of the current logged in users posts
        public  IActionResult Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole(SD.Role_User))
            {
                IEnumerable<BlogPost> blogPostList = _unitOfWork.BlogPost.GetAll(u => u.ApplicationUserId == userId);
                
                //UserProfileVM userVM = new()
                //{
                //    BlogPostList = blogPostList,
                //};
                return View(blogPostList);
            }
            return View();
        }

        // GET: User/Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_context.Profiles.FirstOrDefault(x => x.ApplicationUserId == userId));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            UserProfileVM profileVM = new UserProfileVM()
            {
                User = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId),
                Profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ApplicationUserId == userId),
                //BlogPostList = await _context.BlogPosts.Where(b => b.ApplicationUserId == userId).ToListAsync()
                BlogPostList = await _context.BlogPosts
                    .Where(b => b.ApplicationUserId == userId)
                    .OrderByDescending(b => b.PublishedDate)
                    .ToListAsync()
            };

            return View(profileVM);
        }


        // Added this section to allow viewing another user's public profile by user id (public view)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ApplicationUserId == id);
            var posts = await _context.BlogPosts
                .Where(b => b.ApplicationUserId == id && b.IsPublic)
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();

            var profileVM = new UserProfileVM
            {
                User = user ?? new ApplicationUser(),
                Profile = profile ?? new Profile(),
                BlogPostList = posts
            };

            // Reuse the existing Profile view
            return View("Profile", profileVM);
        }

        [AllowAnonymous]
        [HttpGet] // friendly absolute route, e.g. /u/123
        public async Task<IActionResult> ViewPublicProfile(int id)
        {
            // load profile including the related ApplicationUser (if present)
            var profile = await _context.Profiles
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null) return NotFound();

            var userId = profile.ApplicationUserId;
            var posts = await _context.BlogPosts
                .Where(b => b.ApplicationUserId == userId && b.IsPublic)
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();

            var profileVm = new UserProfileVM
            {
                User = profile.ApplicationUser ?? new ApplicationUser(),
                Profile = profile,
                BlogPostList = posts
            };

            return View("ViewPublicProfile", profileVm);
        }



        // GET: User/Profile/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var profile = await _context.Profiles
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (profile == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(profile);
        //}

        // GET: User/Profile/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: User/Profile/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Bio,DisplayName,UserId")] Profile profile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(profile);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(profile);
        //}

        // GET: User/Profile/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
               return NotFound();
            }

            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        // POST: User/Profile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Bio,DisplayName")] Profile profile /*IFormFile ProfileImage, IFormFile CoverImage*/)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            // Load existing entity to preserve FK and any other fields not in the bind list
            var existing = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();

            // Only update allowed fields from the incoming model
            existing.Bio = profile.Bio;
            existing.DisplayName = profile.DisplayName;

            try
            {
                _context.Profiles.Update(existing);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Profiles.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            // Redirect to profile view (or Index) to avoid re-post issues
            return RedirectToAction(nameof(Profile));
        }

        public string ImageUpload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                // Handle file upload logic here
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "profiles", image.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyToAsync(stream);
                }
                //returns the relative path to be saved in the database
                return $"/uploads/profiles/{image.FileName}";
            }
            return null;
        }

        // GET: User/Profile/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var profile = await _context.Profiles
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (profile == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(profile);
        //}

        // POST: User/Profile/Delete/5
        //    [HttpPost, ActionName("Delete")]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> DeleteConfirmed(int id)
        //    {
        //        var profile = await _context.Profiles.FindAsync(id);
        //        if (profile != null)
        //        {
        //            _context.Profiles.Remove(profile);
        //        }

        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    private bool ProfileExists(int id)
        //    {
        //        return _context.Profiles.Any(e => e.Id == id);
        //    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, error = "No file uploaded" });

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return NotFound(new { success = false, error = "Profile not found" });

            var uploads = System.IO.Path.Combine(_env.WebRootPath, "uploads", "profiles");
            System.IO.Directory.CreateDirectory(uploads);

            var ext = System.IO.Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploads, fileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // update DB with cache-busting query so browser fetches new image
            profile.ProfileImageUrl = $"/uploads/profiles/{fileName}?v={DateTime.UtcNow.Ticks}";

            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();

            return Json(new { success = true, url = profile.ProfileImageUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCoverImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, error = "No file uploaded" });

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return NotFound(new { success = false, error = "Profile not found" });

            var uploads = System.IO.Path.Combine(_env.WebRootPath, "uploads", "profiles");
            System.IO.Directory.CreateDirectory(uploads);

            var ext = System.IO.Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploads, fileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            profile.CoverImageUrl = $"/uploads/profiles/{fileName}?v={DateTime.UtcNow.Ticks}";

            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();

            return Json(new { success = true, url = profile.CoverImageUrl });
        }
    }
}
