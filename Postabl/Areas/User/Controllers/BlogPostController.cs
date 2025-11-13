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

            var blogPost = await _context.BlogPosts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
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
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,PublishedDate,Author,Likes,ProfileId")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
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
            return RedirectToAction(nameof(Index));
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

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            // Simple increment; you can add further checks (one-like-per-user) if desired
            blogPost.Likes += 1;
            _context.Update(blogPost);
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
        public async Task<IActionResult> QuickPost(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Content cannot be empty.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            var blogPost = new BlogPost
            {
                ApplicationUserId = userId,
                Author = user.Name,
                Content = content,
                Title = "Quick Post",
                PublishedDate = DateTime.Now,
                Likes = 0,
                IsPublic = true
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
