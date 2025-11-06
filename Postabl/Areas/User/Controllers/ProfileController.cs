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

            /////////////////////View test posts
            //IEnumerable<BlogPost> blogPosts = new List<BlogPost>
            //{
            //    new BlogPost { Id = 1, Title = "First Post", Content = "This is the content of the first post.", Author = "Admin", PublishedDate = DateTime.Now, IsPublic = true },
            //    new BlogPost { Id = 2, Title = "Second Post", Content = "This is the content of the second post.", Author = "Admin", PublishedDate = DateTime.Now, IsPublic = true }
            //};
            ////var blogPosts = await _context.BlogPosts.ToListAsync();
            //return View(blogPosts);

            //View All posts made by logged in user
            //Get Current logged in userId
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if(User.IsInRole(SD.Role_User))
            //{
            //    IEnumerable<BlogPost> BlogPostList = _context.BlogPosts.Where(b => b.ApplicationUserId == userId).ToList();
            //    return View(BlogPostList);
            //}
            //return View();
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
                BlogPostList = await _context.BlogPosts.Where(b => b.ApplicationUserId == userId).ToListAsync()
            };

            return View(profileVM);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Bio,DisplayName")] Profile profile, IFormFile ProfileImage, IFormFile CoverImage)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            // get existing to preserve URLs if no new files are uploaded
            var existing = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();

            // ensure the model has existing image urls as a baseline
            profile.ProfileImageUrl = existing.ProfileImageUrl;
            profile.CoverImageUrl = existing.CoverImageUrl;

            // preserve existing FK
            profile.ApplicationUserId = existing.ApplicationUserId;

            // handle profile image upload
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploads);
                var ext = Path.GetExtension(ProfileImage.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
                profile.ProfileImageUrl = $"/uploads/profiles/{fileName}";
            }

            // handle cover image upload
            if(CoverImage != null && CoverImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploads);
                var ext = Path.GetExtension(CoverImage.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImage.CopyToAsync(stream);
                }
                profile.CoverImageUrl = $"/uploads/profiles/{fileName}";
            }

            if(ModelState.IsValid)
            {
                try
                {
                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Profiles.Any(e => e.Id == profile.Id)) return NotFound();
                    throw;
                }
            }
            return View(profile);
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
    }
}
