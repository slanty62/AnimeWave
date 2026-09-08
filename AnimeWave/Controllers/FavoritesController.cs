using AnimeWave.Data;
using AnimeWave.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnimeWave.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            string userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var favorites =
                await _context.Favorites

                .Where(f =>
                    f.UserId == userId)

                .Include(f => f.Anime)

                    .ThenInclude(a =>
                        a.AnimeGenres)

                    .ThenInclude(ag =>
                        ag.Genre)

                .OrderByDescending(f =>
                    f.CreatedAt)

                .ToListAsync();

            return View(favorites);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(
            int animeId)
        {
            string userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var favorite =
                await _context.Favorites

                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.AnimeId == animeId);


            if (favorite == null)
            {
                _context.Favorites.Add(
                    new Favorite
                    {
                        UserId = userId,
                        AnimeId = animeId
                    });
            }
            else
            {
                _context.Favorites.Remove(
                    favorite);
            }


            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "Anime",
                new
                {
                    id = animeId
                });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int animeId)
        {
            string userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var favorite =
                await _context.Favorites

                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.AnimeId == animeId);

            if (favorite != null)
            {
                _context.Favorites.Remove(
                    favorite);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(
                nameof(Index));
        }
    }
}