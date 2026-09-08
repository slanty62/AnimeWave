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
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? genreId,
            decimal? minRating,
            string sort = "rating")
        {
            var query = _context.Animes

                .Include(a => a.AnimeGenres)

                .ThenInclude(ag => ag.Genre)

                .AsQueryable();



            // =====================================================
            // ПОИСК
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();


                query = query.Where(a =>

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
            // ФИЛЬТР ПО ЖАНРУ
            // =====================================================

            if (genreId.HasValue)
            {
                query = query.Where(a =>

                    a.AnimeGenres.Any(
                        ag =>
                            ag.GenreId
                            ==
                            genreId.Value
                    )
                );
            }



            // =====================================================
            // МИНИМАЛЬНЫЙ РЕЙТИНГ
            // =====================================================

            if (minRating.HasValue)
            {
                query = query.Where(
                    a =>
                        a.Rating
                        >=
                        minRating.Value
                );
            }



            // =====================================================
            // СОРТИРОВКА
            // =====================================================

            query = sort switch
            {
                "new" =>
                    query.OrderByDescending(
                        a => a.ReleaseYear
                    ),

                "old" =>
                    query.OrderBy(
                        a => a.ReleaseYear
                    ),

                "title" =>
                    query.OrderBy(
                        a => a.Title
                    ),

                "rating-low" =>
                    query.OrderBy(
                        a => a.Rating
                    ),

                _ =>
                    query.OrderByDescending(
                        a => a.Rating
                    )
            };



            // =====================================================
            // ДАННЫЕ ДЛЯ ФИЛЬТРОВ
            // =====================================================

            ViewBag.Genres =
                await _context.Genres

                    .OrderBy(g => g.Name)

                    .ToListAsync();


            ViewBag.Search =
                search;


            ViewBag.GenreId =
                genreId;


            ViewBag.MinRating =
                minRating;


            ViewBag.Sort =
                sort;



            var animes =
                await query.ToListAsync();


            return View(animes);
        }



        // =========================================================
        // СТРАНИЦА АНИМЕ
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
            // СОРТИРУЕМ СЕРИИ
            // =====================================================

            if (anime.Episodes != null)
            {
                anime.Episodes =
                    anime.Episodes

                        .OrderBy(e =>
                            e.EpisodeNumber)

                        .ToList();
            }



            // =====================================================
            // ПО УМОЛЧАНИЮ НЕ В ИЗБРАННОМ
            // =====================================================

            ViewBag.IsFavorite = false;



            // =====================================================
            // ЕСЛИ ПОЛЬЗОВАТЕЛЬ АВТОРИЗОВАН
            // =====================================================

            if (
                User.Identity?.IsAuthenticated
                == true
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
                    // =============================================
                    // ПРОВЕРЯЕМ ИЗБРАННОЕ
                    // =============================================

                    ViewBag.IsFavorite =
                        await _context.Favorites

                            .AnyAsync(
                                f =>
                                    f.UserId
                                    ==
                                    userId

                                    &&

                                    f.AnimeId
                                    ==
                                    anime.Id
                            );



                    // =============================================
                    // ИСТОРИЯ ПРОСМОТРОВ
                    // =============================================

                    var history =
                        await _context.ViewingHistories

                            .FirstOrDefaultAsync(
                                v =>
                                    v.UserId
                                    ==
                                    userId

                                    &&

                                    v.AnimeId
                                    ==
                                    anime.Id
                            );



                    // Если пользователь ещё
                    // не открывал это аниме
                    if (history == null)
                    {
                        history =
                            new ViewingHistory
                            {
                                UserId =
                                    userId,

                                AnimeId =
                                    anime.Id,

                                ViewedAt =
                                    DateTime.UtcNow
                            };


                        _context.ViewingHistories.Add(
                            history
                        );
                    }

                    // Если уже открывал —
                    // просто обновляем дату,
                    // чтобы аниме поднялось
                    // наверх профиля.
                    else
                    {
                        history.ViewedAt =
                            DateTime.UtcNow;
                    }



                    await _context
                        .SaveChangesAsync();
                }
            }



            return View(anime);
        }



        // =========================================================
        // ПРОСМОТР СЕРИИ
        // =========================================================
        //
        // Систему добавления серий из админки мы убрали,
        // но этот метод можно оставить.
        //
        // Если в БД уже существуют серии,
        // старые кнопки просмотра продолжат работать.
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
                            e.Id
                            ==
                            episodeId

                            &&

                            e.AnimeId
                            ==
                            animeId
                    );



            if (episode == null)
            {
                return NotFound();
            }



            // =====================================================
            // ОБНОВЛЯЕМ ИСТОРИЮ
            // =====================================================

            if (
                User.Identity?.IsAuthenticated
                == true
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
                                    v.UserId
                                    ==
                                    userId

                                    &&

                                    v.AnimeId
                                    ==
                                    animeId
                            );



                    if (history == null)
                    {
                        history =
                            new ViewingHistory
                            {
                                UserId =
                                    userId,

                                AnimeId =
                                    animeId,

                                ViewedAt =
                                    DateTime.UtcNow
                            };


                        _context.ViewingHistories.Add(
                            history
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



            return View(episode);
        }
    }
}