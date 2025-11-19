using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Postabl.Areas.User.Controllers
{
    [Area("User")]
    public class BlogPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User/BlogPost
        public async Task<IActionResult> Index()
        {
            return View(await _context.BlogPosts.ToListAsync());
        }

        public async Task<IActionResult> ViewPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Include ApplicationUser -> Profile so we can map profile id and image safely
            var blogPost = await _context.BlogPosts
                .Include(b => b.ApplicationUser)
                    .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

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
                ProfileId = blogPost.ApplicationUser?.Profile?.Id,
                ProfileImageUrl = blogPost.ApplicationUser?.Profile?.ProfileImageUrl
            };

            return View(postVM);
        }

        // GET: User/BlogPost/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Include ApplicationUser -> Profile so we can map profile id and image safely
            var blogPost = await _context.BlogPosts
                .Include(b => b.ApplicationUser)
                    .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

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
                ProfileId = blogPost.ApplicationUser?.Profile?.Id,
                ProfileImageUrl = blogPost.ApplicationUser?.Profile?.ProfileImageUrl
            };

            return View(postVM);
        }

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
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            //blogPost.Author = user!.Name;
            //blogPost.ApplicationUserId = user.Id;
            //blogPost.ApplicationUser = user;

            if (ModelState.IsValid)
            {
                //blogPost.IsPublic = true;
                //blogPost.ApplicationUser = user!;
                blogPost.ApplicationUserId = userId;
                blogPost.PublishedDate = DateTime.Now;
                blogPost.Author = user.Name;
                blogPost.Likes = 0;
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Profile", "Profile");
            }
            return RedirectToAction("Profile", "Profile");
            //return View(blogPost);
        }

        // GET: User/BlogPost/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,IsPublic")] BlogPost posted)
        {
            if (id != posted.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // load full entity to re-render the view (preserve fields not posted)
                var reload = await _context.BlogPosts
                    .Include(b => b.ApplicationUser)
                        .ThenInclude(u => u.Profile)
                    .FirstOrDefaultAsync(b => b.Id == id);
                return View(reload ?? posted);
            }

            // Load existing entity from DB so we only update allowed properties and preserve the FK
            var existing = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return NotFound();

            // Copy only editable fields
            existing.Title = posted.Title;
            existing.Content = posted.Content;
            existing.IsPublic = posted.IsPublic;

            try
            {
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(existing.Id)) return NotFound();
                throw;
            }

            // Redirect to the post details (or Index) to avoid repost
            return RedirectToAction(nameof(Details), new { id = existing.Id });
        }

        // GET: User/BlogPost/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
            }

            await _context.SaveChangesAsync();
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

            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
            if (blogPost == null) return NotFound();

            // parse LikedBy JSON into HashSet<string>
            var likedBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(blogPost.LikedBy))
            {
                try
                {
                    likedBy = System.Text.Json.JsonSerializer.Deserialize<HashSet<string>>(blogPost.LikedBy) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
            blogPost.LikedBy = System.Text.Json.JsonSerializer.Serialize(likedBy);

            _context.BlogPosts.Update(blogPost);
            await _context.SaveChangesAsync();

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
            //if (string.IsNullOrEmpty(userId))
            //{
            //    // redirect to login (preserves return URL)
            //    var returnUrl = Url.Action("Profile", "Profile", new { area = "User" });
            //    return Redirect($"/Identity/Account/Login?returnUrl={UrlEncoder.Default.Encode(returnUrl)}");
            //}

            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var blogPost = new BlogPost
            {
                ApplicationUserId = userId,
                Author = user.Name,
                Content = content,
                Title = "Quick Post",
                PublishedDate = DateTime.Now,
                Likes = 0,
                IsPublic = isPublic
            };
            _context.Add(blogPost);
            await _context.SaveChangesAsync();
            return RedirectToAction("Profile", "Profile");
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }
}
