using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Travelogram.Models;
using Travelogram.Data;
using Microsoft.EntityFrameworkCore;
// ... other using statements ...

namespace Travelogram.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var posts = from p in _context.Posts
                    select p;

        // If the search string is null or empty, return an empty list.
        if (String.IsNullOrEmpty(searchString))
        {
            return View(new List<Post>());
        }

        posts = posts.Where(s => s.Heading.Contains(searchString));
        return View(await posts.ToListAsync());
    }



}
