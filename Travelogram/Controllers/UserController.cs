using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Travelogram.Models;
using System.Linq;
using System.Threading.Tasks;
using Travelogram.Data;
using Travelogram.ViewModels;

namespace Travelogram.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context; // Replace with your DbContext name if different

        public UserController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userProfileVM = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                UserPosts = _context.Posts.Where(p => p.UserId == user.Id).ToList()
            };

            return View(userProfileVM);
        }
    }

}

