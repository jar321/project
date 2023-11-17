using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Travelogram.Data;
using Travelogram.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace Travelogram.Controllers
{
    

    
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private readonly IWebHostEnvironment _hostingEnvironment;
        //private IWebHostEnvironment? hostingEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;  // Create a private field for the IWebHostEnvironment
        private readonly UserManager<IdentityUser> _userManager;
        private UserManager<IdentityUser>? userManager;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            // _hostingEnvironment = hostingEnvironment; // This line is crucial
            _webHostEnvironment = webHostEnvironment;  // Assign the injected instance to the private field
            _userManager = userManager;
        }

        [AllowAnonymous]
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            return _context.Posts != null ?
                        View( _context.Posts.ToList()) :
                        Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
        }


        [AllowAnonymous]
        // GET: Posts/Details/5
        public  IActionResult Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            // Ensure comments are loaded with the post
            var post =  _context.Posts
                        .Include(p => p.Comments)
                            .ThenInclude(c => c.User)  // Load the associated user of each comment
                        .FirstOrDefault(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        //Attribute based access control

        [Authorize]
        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }
        //Attribute based access control
        [Authorize]
        // POST: Posts/Create
       
        [HttpPost]
        //Protects against CSRF token using AntiForgery tokens
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Heading,Content,Fname,Likes")] Post post, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null && image.Length > 0)
                    {
                        if (image.Length > 25 * 1024 * 1024) // 25MB limit
                        {
                            ModelState.AddModelError("image", "The file is too large.");
                            return View(post);
                        }

                        var allowedMimeTypes = new List<string> { "image/jpeg", "image/png", "image/gif" };
                        if (!allowedMimeTypes.Contains(image.ContentType))
                        {
                            ModelState.AddModelError("image", "Invalid file type. Only JPEG, PNG, and GIF are allowed.");
                            return View(post);
                        }

                        // Create unique filename
                        var sanitizedFileName = Path.GetFileName(image.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;

                        // Specify directory to save the file
                        string folder = "uploads";
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath,folder ); 
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        post.FileName = uniqueFileName;  // Store only the filename or relative path in the database
                         if (_userManager != null && User != null)
                {
                    post.UserId = _userManager.GetUserId(User);
                }
                else
                {
                    // Handle the case where _userManager or User is null
                    // You can log the error or redirect to an error page
                    return RedirectToAction("Error");
                }

                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception here
                }
                _context.Add(post);
                await _context.SaveChangesAsync();
                TempData["message"] = "Post has been created.";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [AllowAnonymous]
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Heading,Content,FileName,Likes")] Post post)
        {
            // Find the post by its ID
            var existingPost = await _context.Posts.FindAsync(id);

            // Check if the post exists and if the current user is the author of the post
            if (existingPost == null || existingPost.UserId != _userManager.GetUserId(User))
            {
                // If the post does not exist or the current user is not the author, return a different view or message
                return Unauthorized();  // You can customize this behavior, e.g., return a view with an error message
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            return View(post);
        }

        //Attribute based access control

        [Authorize]
        //GET: MyPosts ...
        public async Task<IActionResult> MyPosts()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userPosts = await _context.Posts.Where(p => p.UserId == currentUserId).ToListAsync();

            return View(userPosts);
        }

        [AllowAnonymous]
        // GET: Posts/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post =  _context.Posts
                .FirstOrDefault(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [AllowAnonymous]
        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Find the post by its ID
            var post =  _context.Posts.Find(id);

            if (post == null)
            {
                return NotFound();
            }

            // Check if the current user is the author of the post
            if (post.UserId != _userManager.GetUserId(User))
            {
                // If the current user is not the author, do not allow deletion and return a different view or message
                return Unauthorized();  // You can customize this behavior, e.g., return a view with an error message
            }

            _context.Posts.Remove(post);
             _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        // ...

        // This section is inside your PostsController class
        [AllowAnonymous]
        // POST: Add a new comment to a post
        [HttpPost]
        public async Task<IActionResult> AddComment(string content, int postId)
        {
            // Create a new comment object
            var comment = new Comment
            {
                Content = content,           // Set the content of the comment
                PostId = postId,             // Associate the comment with the post
                UserId = _userManager.GetUserId(User)  // Set the user who made the comment
            };

            // Add the comment to the database
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Redirect to the post's details page
            return RedirectToAction("Details", new { id = postId });
        }
        [AllowAnonymous]
        // GET: Edit an existing comment
        public async Task<IActionResult> EditComment(int id)
        {
            // Find the comment by its ID
            var comment = await _context.Comments.FindAsync(id);

            // Check if the current user is the author of the comment
            if (comment.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();  // If not, return an Unauthorized response
            }

            // If the user is the author, show the edit view
            return View(comment);
        }
        [AllowAnonymous]
        // POST: Save changes to an edited comment
        [HttpPost]
        public async Task<IActionResult> EditComment(Comment comment)
        {
            // Check if the current user is the author of the comment
            if (comment.UserId != _userManager.GetUserId(User))
            {
                return Unauthorized();  // If not, return an Unauthorized response
            }

            // Update the comment in the database
            _context.Update(comment);
            await _context.SaveChangesAsync();

            // Redirect to the post's details page
            return RedirectToAction("Details", new { id = comment.PostId });
        }
        [AllowAnonymous]
        // DELETE: Remove a comment
        // DELETE: Remove a comment
        public async Task<IActionResult> DeleteComment(int id)
        {
            // Find the comment by its ID
            var comment = await _context.Comments.FindAsync(id);

            // Check if the current user is the author of the comment
            if (comment.UserId != _userManager.GetUserId(User))
            {
                // If the current user is not the author, do not allow deletion and return a different view or message
                return Unauthorized();  // You can customize this behavior, e.g., return a view with an error message
            }

            // If the current user is the author, remove the comment from the database
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            // Redirect to the post's details page
            return RedirectToAction("Details", new { id = comment.PostId });
        }


        // ... (rest of your PostsController)

        // POST: like
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Likes += 1;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }
        // POST: Dislike
        [HttpPost]
        public async Task<IActionResult> Dislike(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Dislikes += 1;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }


        private bool PostExists(int id)
        {
            return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
