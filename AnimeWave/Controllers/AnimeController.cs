using AnimeWave.Data;
using AnimeWave.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace AnimeWave.Controllers
{
    public class AnimeController : Controller
    {
        private readonly ApplicationDbContext _context;


        public AnimeController(
            ApplicationDbContext context)
        {
            _context = context;
        }



        // =========================================================
        // КАТАЛОГ
        // SEARCH + FILTERS + SORT + PAGINATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? genreId,
            decimal? minRating,
            string sort = "rating",
            int page = 1,
            int pageSize = 12)
        {
            pageSize =
                pageSize == 18
                    ? 18
                    : 12;


            if (page < 1)
            {
                page = 1;
            }


            var query =
                _context.Animes

                    .Include(a =>
                        a.AnimeGenres)

                    .ThenInclude(ag =>
                        ag.Genre)

                    .AsNoTracking()

                    .AsQueryable();



            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search =
                    search.Trim();


                query =
                    query.Where(
                        a =>

                            EF.Functions.ILike(
                                a.Title,
                                $"%{search}%"
                            )

                            ||

                            (
                                a.OriginalTitle != null

                                &&

                                EF.Functions.ILike(
                                    a.OriginalTitle,
                                    $"%{search}%"
                                )
                            )
                    );
            }



            // =====================================================
            // GENRE
            // =====================================================

            if (genreId.HasValue)
            {
                query =
                    query.Where(
                        a =>
                            a.AnimeGenres.Any(
                                ag =>
                                    ag.GenreId ==
                                    genreId.Value
                            )
                    );
            }



            // =====================================================
            // RATING
            // =====================================================

            if (minRating.HasValue)
            {
                query =
                    query.Where(
                        a =>
                            a.Rating >=
                            minRating.Value
                    );
            }



            // =====================================================
            // TOTAL ITEMS
            // =====================================================

            int totalItems =
                await query.CountAsync();


            int totalPages =
                totalItems == 0
                    ? 1
                    : (int)Math.Ceiling(
                        totalItems /
                        (double)pageSize
                    );


            if (page > totalPages)
            {
                page =
                    totalPages;
            }



            // =====================================================
            // SORT
            // =====================================================

            query =
                sort switch
                {
                    "new" =>
                        query
                            .OrderByDescending(
                                a => a.ReleaseYear
                            )
                            .ThenByDescending(
                                a => a.Rating
                            ),

                    "old" =>
                        query
                            .OrderBy(
                                a => a.ReleaseYear
                            )
                            .ThenBy(
                                a => a.Title
                            ),

                    "title" =>
                        query
                            .OrderBy(
                                a => a.Title
                            ),

                    "rating-low" =>
                        query
                            .OrderBy(
                                a => a.Rating
                            )
                            .ThenBy(
                                a => a.Title
                            ),

                    _ =>
                        query
                            .OrderByDescending(
                                a => a.Rating
                            )
                            .ThenBy(
                                a => a.Title
                            )
                };



            // =====================================================
            // PAGINATION
            // =====================================================

            var animes =
                await query

                    .Skip(
                        (page - 1) *
                        pageSize
                    )

                    .Take(
                        pageSize
                    )

                    .ToListAsync();



            // =====================================================
            // FILTER DATA
            // =====================================================

            ViewBag.Genres =
                await _context.Genres

                    .AsNoTracking()

                    .OrderBy(
                        g => g.Name
                    )

                    .ToListAsync();


            ViewBag.Search =
                search;


            ViewBag.GenreId =
                genreId;


            ViewBag.MinRating =
                minRating;


            ViewBag.Sort =
                sort;


            ViewBag.CurrentPage =
                page;


            ViewBag.TotalPages =
                totalPages;


            ViewBag.PageSize =
                pageSize;


            ViewBag.TotalItems =
                totalItems;



            return View(
                animes
            );
        }



        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var anime =
                await _context.Animes

                    .Include(a =>
                        a.AnimeGenres)

                    .ThenInclude(ag =>
                        ag.Genre)

                    .Include(a =>
                        a.Episodes)

                    .FirstOrDefaultAsync(
                        a =>
                            a.Id == id
                    );


            if (anime == null)
            {
                return NotFound();
            }



            // =====================================================
            // EPISODES
            // =====================================================

            anime.Episodes =
                anime.Episodes

                    .OrderBy(
                        e =>
                            e.EpisodeNumber
                    )

                    .ToList();



            // =====================================================
            // FAVORITE
            // =====================================================

            ViewBag.IsFavorite =
                false;



            // =====================================================
            // USER HISTORY
            // =====================================================

            if (
                User.Identity?.IsAuthenticated
                ==
                true
            )
            {
                string? userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );


                if (
                    !string.IsNullOrWhiteSpace(
                        userId
                    )
                )
                {
                    ViewBag.IsFavorite =
                        await _context.Favorites

                            .AnyAsync(
                                f =>
                                    f.UserId ==
                                    userId

                                    &&

                                    f.AnimeId ==
                                    anime.Id
                            );



                    var history =
                        await _context.ViewingHistories

                            .FirstOrDefaultAsync(
                                v =>
                                    v.UserId ==
                                    userId

                                    &&

                                    v.AnimeId ==
                                    anime.Id
                            );


                    if (history == null)
                    {
                        _context.ViewingHistories.Add(
                            new ViewingHistory
                            {
                                UserId =
                                    userId,

                                AnimeId =
                                    anime.Id,

                                ViewedAt =
                                    DateTime.UtcNow
                            }
                        );
                    }
                    else
                    {
                        history.ViewedAt =
                            DateTime.UtcNow;
                    }


                    await _context
                        .SaveChangesAsync();
                }
            }



            // =====================================================
            // SIMILAR ANIME
            // =====================================================

            var currentGenreIds =
                anime.AnimeGenres

                    .Select(
                        ag => ag.GenreId
                    )

                    .Distinct()

                    .ToList();



            List<Anime> similarAnime;



            // =====================================================
            // ЕСЛИ ЕСТЬ ЖАНРЫ
            // =====================================================

            if (currentGenreIds.Any())
            {
                // Сначала получаем подходящих кандидатов.
                // Текущее аниме исключаем.

                var candidates =
                    await _context.Animes

                        .AsNoTracking()

                        .Include(a =>
                            a.AnimeGenres)

                        .ThenInclude(ag =>
                            ag.Genre)

                        .Where(
                            a =>
                                a.Id != anime.Id

                                &&

                                a.AnimeGenres.Any(
                                    ag =>
                                        currentGenreIds.Contains(
                                            ag.GenreId
                                        )
                                )
                        )

                        .Take(40)

                        .ToListAsync();



                // Сортируем уже в памяти:
                //
                // 1. Количество совпадающих жанров
                // 2. Рейтинг
                // 3. Год выхода

                similarAnime =
                    candidates

                        .OrderByDescending(
                            a =>
                                a.AnimeGenres.Count(
                                    ag =>
                                        currentGenreIds.Contains(
                                            ag.GenreId
                                        )
                                )
                        )

                        .ThenByDescending(
                            a => a.Rating
                        )

                        .ThenByDescending(
                            a => a.ReleaseYear
                        )

                        .Take(6)

                        .ToList();
            }

            // =====================================================
            // FALLBACK
            //
            // Если у тайтла вообще нет жанров —
            // просто рекомендуем лучшие другие аниме.
            // =====================================================

            else
            {
                similarAnime =
                    await _context.Animes

                        .AsNoTracking()

                        .Include(a =>
                            a.AnimeGenres)

                        .ThenInclude(ag =>
                            ag.Genre)

                        .Where(
                            a =>
                                a.Id != anime.Id
                        )

                        .OrderByDescending(
                            a => a.Rating
                        )

                        .ThenByDescending(
                            a => a.ReleaseYear
                        )

                        .Take(6)

                        .ToListAsync();
            }



            ViewBag.SimilarAnime =
                similarAnime;



            return View(
                anime
            );
        }



        // =========================================================
        // WATCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Watch(
            int animeId,
            int episodeId)
        {
            var episode =
                await _context.Episodes

                    .Include(e =>
                        e.Anime)

                    .FirstOrDefaultAsync(
                        e =>
                            e.Id ==
                            episodeId

                            &&

                            e.AnimeId ==
                            animeId
                    );


            if (episode == null)
            {
                return NotFound();
            }



            // =====================================================
            // HISTORY
            // =====================================================

            if (
                User.Identity?.IsAuthenticated
                ==
                true
            )
            {
                string? userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );


                if (
                    !string.IsNullOrWhiteSpace(
                        userId
                    )
                )
                {
                    var history =
                        await _context.ViewingHistories

                            .FirstOrDefaultAsync(
                                v =>
                                    v.UserId ==
                                    userId

                                    &&

                                    v.AnimeId ==
                                    animeId
                            );


                    if (history == null)
                    {
                        _context.ViewingHistories.Add(
                            new ViewingHistory
                            {
                                UserId =
                                    userId,

                                AnimeId =
                                    animeId,

                                ViewedAt =
                                    DateTime.UtcNow
                            }
                        );
                    }
                    else
                    {
                        history.ViewedAt =
                            DateTime.UtcNow;
                    }


                    await _context
                        .SaveChangesAsync();
                }
            }



            return View(
                episode
            );
        }
    }
}