using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
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
        // CATALOG
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



            // SEARCH

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



            // GENRE

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



            // RATING

            if (minRating.HasValue)
            {
                query =
                    query.Where(
                        a =>
                            a.Rating >=
                            minRating.Value
                    );
            }



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



            // SORT

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
                        query.OrderBy(
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



            anime.Episodes =
                anime.Episodes

                    .OrderBy(
                        e =>
                            e.EpisodeNumber
                    )

                    .ToList();



            string? userId =
                User.Identity?.IsAuthenticated == true

                    ? User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    )

                    : null;



            // =====================================================
            // FAVORITE + HISTORY
            // =====================================================

            ViewBag.IsFavorite =
                false;


            if (!string.IsNullOrWhiteSpace(userId))
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



            // =====================================================
            // SIMILAR ANIME
            // =====================================================

            var currentGenreIds =
                anime.AnimeGenres

                    .Select(
                        ag =>
                            ag.GenreId
                    )

                    .Distinct()

                    .ToList();



            List<Anime> similarAnime;



            if (currentGenreIds.Any())
            {
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

                        .OrderByDescending(
                            a => a.Rating
                        )

                        .ThenByDescending(
                            a => a.ReleaseYear
                        )

                        .Take(40)

                        .ToListAsync();



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
                            a =>
                                a.Rating
                        )

                        .ThenByDescending(
                            a =>
                                a.ReleaseYear
                        )

                        .Take(6)

                        .ToList();
            }
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
                            a =>
                                a.Rating
                        )

                        .Take(6)

                        .ToListAsync();
            }



            ViewBag.SimilarAnime =
                similarAnime;



            // =====================================================
            // REVIEWS
            // =====================================================

            var reviewEntities =
                await _context.AnimeReviews

                    .AsNoTracking()

                    .Include(
                        r => r.User
                    )

                    .Where(
                        r =>
                            r.AnimeId ==
                            anime.Id
                    )

                    .OrderByDescending(
                        r =>
                            r.CreatedAt
                    )

                    .ToListAsync();



            var currentReview =
                !string.IsNullOrWhiteSpace(userId)

                    ? reviewEntities
                        .FirstOrDefault(
                            r =>
                                r.UserId ==
                                userId
                        )

                    : null;



            double averageRating =
                reviewEntities.Any()

                    ? reviewEntities
                        .Average(
                            r =>
                                r.Rating
                        )

                    : 0;



            var reviewsSection =
                new AnimeReviewsSectionViewModel
                {
                    AnimeId =
                        anime.Id,

                    IsAuthenticated =
                        !string.IsNullOrWhiteSpace(
                            userId
                        ),

                    AverageRating =
                        averageRating,

                    ReviewsCount =
                        reviewEntities.Count,

                    Form =
                        new AnimeReviewFormViewModel
                        {
                            AnimeId =
                                anime.Id,

                            Rating =
                                currentReview?.Rating
                                ?? 10,

                            Text =
                                currentReview?.Text
                                ?? string.Empty
                        },

                    Reviews =
                        reviewEntities

                            .Select(
                                review =>
                                    new AnimeReviewItemViewModel
                                    {
                                        Id =
                                            review.Id,

                                        AuthorName =
                                            !string.IsNullOrWhiteSpace(
                                                review.User.DisplayName
                                            )
                                                ? review.User.DisplayName!
                                                : review.User.UserName
                                                    ?? "AnimeWave User",

                                        AvatarStyle =
                                            NormalizeAvatar(
                                                review.User.AvatarStyle
                                            ),

                                        AvatarSymbol =
                                            GetAvatarSymbol(
                                                review.User.AvatarStyle
                                            ),

                                        Rating =
                                            review.Rating,

                                        Text =
                                            review.Text,

                                        CreatedAt =
                                            review.CreatedAt,

                                        UpdatedAt =
                                            review.UpdatedAt,

                                        IsMine =
                                            review.UserId ==
                                            userId
                                    }
                            )

                            .ToList()
                };



            ViewBag.ReviewsSection =
                reviewsSection;



            return View(
                anime
            );
        }



        // =========================================================
        // SAVE REVIEW
        //
        // Создаёт новый отзыв или обновляет существующий.
        // =========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReview(
            AnimeReviewFormViewModel model)
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }



            bool animeExists =
                await _context.Animes

                    .AsNoTracking()

                    .AnyAsync(
                        a =>
                            a.Id ==
                            model.AnimeId
                    );


            if (!animeExists)
            {
                return NotFound();
            }



            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Проверь оценку и текст отзыва.";

                return RedirectToAction(
                    nameof(Details),
                    "Anime",
                    new
                    {
                        id = model.AnimeId
                    },
                    "reviews"
                );
            }



            string reviewText =
                model.Text.Trim();


            if (string.IsNullOrWhiteSpace(reviewText))
            {
                TempData["ErrorMessage"] =
                    "Напишите текст отзыва.";

                return RedirectToAction(
                    nameof(Details),
                    "Anime",
                    new
                    {
                        id = model.AnimeId
                    },
                    "reviews"
                );
            }



            var review =
                await _context.AnimeReviews

                    .FirstOrDefaultAsync(
                        r =>
                            r.UserId ==
                            userId

                            &&

                            r.AnimeId ==
                            model.AnimeId
                    );



            if (review == null)
            {
                review =
                    new AnimeReview
                    {
                        UserId =
                            userId,

                        AnimeId =
                            model.AnimeId,

                        Rating =
                            model.Rating,

                        Text =
                            reviewText,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _context.AnimeReviews.Add(
                    review
                );


                TempData["SuccessMessage"] =
                    "Отзыв опубликован ✓";
            }
            else
            {
                review.Rating =
                    model.Rating;


                review.Text =
                    reviewText;


                review.UpdatedAt =
                    DateTime.UtcNow;


                TempData["SuccessMessage"] =
                    "Отзыв обновлён ✓";
            }



            await _context
                .SaveChangesAsync();



            return RedirectToAction(
                nameof(Details),
                "Anime",
                new
                {
                    id = model.AnimeId
                },
                "reviews"
            );
        }



        // =========================================================
        // DELETE OWN REVIEW
        // =========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(
            int reviewId,
            int animeId)
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }



            var review =
                await _context.AnimeReviews

                    .FirstOrDefaultAsync(
                        r =>
                            r.Id ==
                            reviewId

                            &&

                            r.UserId ==
                            userId

                            &&

                            r.AnimeId ==
                            animeId
                    );


            if (review == null)
            {
                return NotFound();
            }



            _context.AnimeReviews.Remove(
                review
            );


            await _context
                .SaveChangesAsync();



            TempData["SuccessMessage"] =
                "Отзыв удалён";


            return RedirectToAction(
                nameof(Details),
                "Anime",
                new
                {
                    id = animeId
                },
                "reviews"
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

                    .Include(
                        e =>
                            e.Anime
                    )

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


                if (!string.IsNullOrWhiteSpace(userId))
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



        // =========================================================
        // AVATAR HELPERS
        // =========================================================

        private static string NormalizeAvatar(
            string? avatarStyle)
        {
            return avatarStyle switch
            {
                "sakura" => "sakura",
                "kitsune" => "kitsune",
                "blade" => "blade",
                "neko" => "neko",
                "star" => "star",
                "cyber" => "cyber",
                "wave" => "wave",

                _ => "violet"
            };
        }



        private static string GetAvatarSymbol(
            string? avatarStyle)
        {
            return NormalizeAvatar(
                avatarStyle
            ) switch
            {
                "sakura" => "桜",
                "kitsune" => "狐",
                "blade" => "刀",
                "neko" => "猫",
                "star" => "星",
                "cyber" => "夢",
                "wave" => "波",

                _ => "月"
            };
        }
    }
}