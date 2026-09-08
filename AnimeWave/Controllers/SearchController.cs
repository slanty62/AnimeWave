using AnimeWave.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;


        public SearchController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // SMART SEARCH
        // GET: /Search/Suggest?q=nar
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Suggest(
            string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(
                    Array.Empty<object>()
                );
            }


            q = q.Trim();


            // Не отправляем запросы на одну букву
            if (q.Length < 2)
            {
                return Json(
                    Array.Empty<object>()
                );
            }


            var animes =
                await _context.Animes

                    .AsNoTracking()

                    .Where(a =>

                        EF.Functions.ILike(
                            a.Title,
                            $"%{q}%"
                        )

                        ||

                        (
                            a.OriginalTitle != null

                            &&

                            EF.Functions.ILike(
                                a.OriginalTitle,
                                $"%{q}%"
                            )
                        )
                    )

                    .OrderByDescending(a =>
                        a.Rating)

                    .ThenBy(a =>
                        a.Title)

                    .Take(8)

                    .Select(a =>
                        new
                        {
                            id =
                                a.Id,

                            title =
                                a.Title,

                            originalTitle =
                                a.OriginalTitle,

                            posterUrl =
                                a.PosterUrl,

                            rating =
                                a.Rating,

                            releaseYear =
                                a.ReleaseYear,

                            ageRating =
                                a.AgeRating
                        }
                    )

                    .ToListAsync();


            return Json(animes);
        }
    }
}