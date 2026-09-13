using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.Services;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace AnimeWave.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IAnimePopularityService
            _popularityService;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public HomeController(
            ApplicationDbContext context,
            IAnimePopularityService popularityService)
        {
            _context =
                context;

            _popularityService =
                popularityService;
        }



        // =========================================================
        // HOME
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // =====================================================
            // LOAD ANIME
            // =====================================================

            List<Anime> animeList =
                await _context.Animes

                    .AsNoTracking()

                    .Include(
                        a => a.AnimeGenres
                    )

                    .ThenInclude(
                        ag => ag.Genre
                    )

                    .OrderByDescending(
                        a => a.Rating
                    )

                    .ThenByDescending(
                        a => a.ReleaseYear
                    )

                    .ToListAsync();



            // =====================================================
            // POPULARITY
            // =====================================================

            Dictionary<
                int,
                AnimePopularityViewModel
            > popularity =
                await _popularityService
                    .GetAllAsync();



            ViewBag.Popularity =
                popularity;



            // =====================================================
            // HERO
            //
            // Берём лучшие тайтлы с banner.
            // =====================================================

            List<Anime> heroAnime =
                animeList

                    .Where(
                        a =>
                            !string.IsNullOrWhiteSpace(
                                a.BannerUrl
                            )
                    )

                    .OrderByDescending(
                        a =>
                            popularity.TryGetValue(
                                a.Id,
                                out var stat
                            )
                                ? stat.PopularityScore
                                : 0
                    )

                    .ThenByDescending(
                        a => a.Rating
                    )

                    .Take(6)

                    .ToList();



            // Если баннеров мало —
            // дополняем лучшими тайтлами.

            if (heroAnime.Count < 6)
            {
                var existingIds =
                    heroAnime
                        .Select(
                            a => a.Id
                        )
                        .ToHashSet();


                var additional =
                    animeList

                        .Where(
                            a =>
                                !existingIds.Contains(
                                    a.Id
                                )
                        )

                        .Take(
                            6 - heroAnime.Count
                        );


                heroAnime.AddRange(
                    additional
                );
            }


            ViewBag.HeroAnime =
                heroAnime;



            // =====================================================
            // POPULAR / TRENDING
            // =====================================================

            List<Anime> popularAnime =
                animeList

                    .OrderByDescending(
                        a =>
                            popularity.TryGetValue(
                                a.Id,
                                out var stat
                            )
                                ? stat.PopularityScore
                                : 0
                    )

                    .ThenByDescending(
                        a => a.Rating
                    )

                    .ThenByDescending(
                        a => a.ReleaseYear
                    )

                    .Take(12)

                    .ToList();



            // На абсолютно новой БД все показатели могут быть 0.
            // В таком случае рейтинг каталога всё равно даст
            // нормальный порядок.

            return View(
                popularAnime
            );
        }



        // =========================================================
        // ERROR
        // =========================================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ??
                        HttpContext.TraceIdentifier
                }
            );
        }
    }
}