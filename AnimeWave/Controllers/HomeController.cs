using AnimeWave.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var animes = await _context.Animes
                .Include(a => a.AnimeGenres)
                .ThenInclude(ag => ag.Genre)
                .OrderByDescending(a => a.Rating)
                .ToListAsync();

            return View(animes);
        }
    }
}