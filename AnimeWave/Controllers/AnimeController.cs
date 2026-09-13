using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.Services;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AnimeWave.Controllers
{
    public class AnimeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAnimePopularityService _popularityService;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public AnimeController(
            ApplicationDbContext context,
            IAnimePopularityService popularityService)
        {
            _context = context;
            _popularityService = popularityService;
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
            // =====================================================
            // PAGE SIZE
            // =====================================================

            pageSize =
                pageSize == 18
                    ? 18
                    : 12;


            if (page < 1)
            {
                page = 1;
            }



            // =====================================================
            // BASE QUERY
            // =====================================================

            IQueryable<Anime> query =
                _context.Animes
                    .AsNoTracking()
                    .Include(a => a.AnimeGenres)
                    .ThenInclude(ag => ag.Genre);



            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();


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
            // MIN RATING
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
            // TOTAL
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
                page = totalPages;
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
            // PAGE DATA
            // =====================================================

            List<Anime> animes =
                await query
                    .Skip(
                        (page - 1) *
                        pageSize
                    )
                    .Take(pageSize)
                    .ToListAsync();



            // =====================================================
            // GENRES
            // =====================================================

            List<Genre> genres =
                await _context.Genres
                    .AsNoTracking()
                    .OrderBy(
                        g => g.Name
                    )
                    .ToListAsync();



            // =====================================================
            // VIEW DATA
            // =====================================================

            ViewBag.Genres =
                genres;

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



            // =====================================================
            // POPULARITY
            // =====================================================

            ViewBag.Popularity =
                await _popularityService
                    .GetAllAsync();



            return View(animes);
        }



        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            // =====================================================
            // ANIME
            // =====================================================

            Anime? anime =
                await _context.Animes
                    .Include(
                        a => a.AnimeGenres
                    )
                    .ThenInclude(
                        ag => ag.Genre
                    )
                    .Include(
                        a => a.Episodes
                    )
                    .FirstOrDefaultAsync(
                        a => a.Id == id
                    );


            if (anime == null)
            {
                return NotFound();
            }



            // =====================================================
            // OPEN COUNTER
            //
            // +1 при каждом открытии Details.
            // =====================================================

            await _context.Animes
                .Where(
                    a => a.Id == anime.Id
                )
                .ExecuteUpdateAsync(
                    update =>
                        update.SetProperty(
                            a => a.OpenCount,
                            a => a.OpenCount + 1
                        )
                );



            // =====================================================
            // EPISODES
            // =====================================================

            anime.Episodes =
                anime.Episodes
                    .OrderBy(
                        e => e.EpisodeNumber
                    )
                    .ToList();



            // =====================================================
            // CURRENT USER
            // =====================================================

            string? userId =
                User.Identity?.IsAuthenticated == true
                    ? User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    )
                    : null;



            // =====================================================
            // FAVORITE
            // =====================================================

            ViewBag.IsFavorite =
                false;


            if (!string.IsNullOrWhiteSpace(userId))
            {
                ViewBag.IsFavorite =
                    await _context.Favorites
                        .AsNoTracking()
                        .AnyAsync(
                            f =>
                                f.UserId == userId
                                &&
                                f.AnimeId == anime.Id
                        );



                // =================================================
                // VIEWING HISTORY
                // =================================================

                ViewingHistory? history =
                    await _context.ViewingHistories
                        .FirstOrDefaultAsync(
                            v =>
                                v.UserId == userId
                                &&
                                v.AnimeId == anime.Id
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


                await _context.SaveChangesAsync();
            }



            // =====================================================
            // SIMILAR ANIME
            // =====================================================

            List<int> currentGenreIds =
                anime.AnimeGenres
                    .Select(
                        ag => ag.GenreId
                    )
                    .Distinct()
                    .ToList();



            List<Anime> similarAnime;



            if (currentGenreIds.Count > 0)
            {
                List<Anime> candidates =
                    await _context.Animes
                        .AsNoTracking()
                        .Include(
                            a => a.AnimeGenres
                        )
                        .ThenInclude(
                            ag => ag.Genre
                        )
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
                            a => a.Rating
                        )
                        .ThenByDescending(
                            a => a.ReleaseYear
                        )
                        .Take(6)
                        .ToList();
            }
            else
            {
                similarAnime =
                    await _context.Animes
                        .AsNoTracking()
                        .Include(
                            a => a.AnimeGenres
                        )
                        .ThenInclude(
                            ag => ag.Genre
                        )
                        .Where(
                            a => a.Id != anime.Id
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



            // =====================================================
            // REVIEWS
            // =====================================================

            List<AnimeReview> reviewEntities =
                await _context.AnimeReviews
                    .AsNoTracking()
                    .Include(
                        r => r.User
                    )
                    .Where(
                        r => r.AnimeId == anime.Id
                    )
                    .OrderByDescending(
                        r => r.CreatedAt
                    )
                    .ToListAsync();



            // =====================================================
            // CURRENT USER REVIEW
            // =====================================================

            AnimeReview? currentReview =
                null;


            if (!string.IsNullOrWhiteSpace(userId))
            {
                currentReview =
                    reviewEntities
                        .FirstOrDefault(
                            r => r.UserId == userId
                        );
            }



            // =====================================================
            // AVERAGE RATING
            // =====================================================

            double averageRating =
                reviewEntities.Count > 0
                    ? reviewEntities.Average(
                        r => r.Rating
                    )
                    : 0;



            // =====================================================
            // REVIEW VIEW MODEL
            // =====================================================

            AnimeReviewsSectionViewModel reviewsSection =
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



            // =====================================================
            // POPULARITY
            // =====================================================

            Dictionary<int, AnimePopularityViewModel> popularity =
                await _popularityService
                    .GetAllAsync();


            ViewBag.Popularity =
                popularity;


            if (
                popularity.TryGetValue(
                    anime.Id,
                    out AnimePopularityViewModel? currentPopularity
                )
            )
            {
                ViewBag.CurrentPopularity =
                    currentPopularity;
            }
            else
            {
                ViewBag.CurrentPopularity =
                    null;
            }



            return View(anime);
        }



        // =========================================================
        // SAVE / UPDATE REVIEW
        // =========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReview(
            AnimeReviewFormViewModel model)
        {
            // =====================================================
            // USER
            // =====================================================

            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }



            // =====================================================
            // ANIME EXISTS
            // =====================================================

            bool animeExists =
                await _context.Animes
                    .AsNoTracking()
                    .AnyAsync(
                        a => a.Id == model.AnimeId
                    );


            if (!animeExists)
            {
                return NotFound();
            }



            // =====================================================
            // RATING
            // =====================================================

            if (
                model.Rating < 1
                ||
                model.Rating > 10
            )
            {
                TempData["ErrorMessage"] =
                    "Оценка должна быть от 1 до 10.";


                return RedirectToReviews(
                    model.AnimeId
                );
            }



            // =====================================================
            // REVIEW TEXT
            // =====================================================

            string reviewText =
                model.Text?.Trim()
                ?? string.Empty;


            if (string.IsNullOrWhiteSpace(reviewText))
            {
                TempData["ErrorMessage"] =
                    "Напишите текст отзыва.";


                return RedirectToReviews(
                    model.AnimeId
                );
            }


            if (reviewText.Length > 2000)
            {
                TempData["ErrorMessage"] =
                    "Отзыв не должен превышать 2000 символов.";


                return RedirectToReviews(
                    model.AnimeId
                );
            }



            // =====================================================
            // EXISTING REVIEW
            // =====================================================

            AnimeReview? review =
                await _context.AnimeReviews
                    .FirstOrDefaultAsync(
                        r =>
                            r.UserId == userId
                            &&
                            r.AnimeId == model.AnimeId
                    );



            // =====================================================
            // CREATE
            // =====================================================

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

            // =====================================================
            // UPDATE
            // =====================================================

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



            await _context.SaveChangesAsync();


            return RedirectToReviews(
                model.AnimeId
            );
        }



        // =========================================================
        // DELETE REVIEW
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



            AnimeReview? review =
                await _context.AnimeReviews
                    .FirstOrDefaultAsync(
                        r =>
                            r.Id == reviewId
                            &&
                            r.UserId == userId
                            &&
                            r.AnimeId == animeId
                    );


            if (review == null)
            {
                return NotFound();
            }



            _context.AnimeReviews.Remove(
                review
            );


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Отзыв удалён";


            return RedirectToReviews(
                animeId
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
            Episode? episode =
                await _context.Episodes
                    .Include(
                        e => e.Anime
                    )
                    .FirstOrDefaultAsync(
                        e =>
                            e.Id == episodeId
                            &&
                            e.AnimeId == animeId
                    );


            if (episode == null)
            {
                return NotFound();
            }



            // =====================================================
            // UPDATE HISTORY
            // =====================================================

            if (User.Identity?.IsAuthenticated == true)
            {
                string? userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );


                if (!string.IsNullOrWhiteSpace(userId))
                {
                    ViewingHistory? history =
                        await _context.ViewingHistories
                            .FirstOrDefaultAsync(
                                v =>
                                    v.UserId == userId
                                    &&
                                    v.AnimeId == animeId
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


                    await _context.SaveChangesAsync();
                }
            }



            return View(episode);
        }



        // =========================================================
        // REDIRECT TO REVIEWS
        //
        // Специально НЕ используем проблемную перегрузку
        // RedirectToAction(..., routeValues, fragment).
        // =========================================================

        private IActionResult RedirectToReviews(
            int animeId)
        {
            string? detailsUrl =
                Url.Action(
                    action: nameof(Details),
                    controller: "Anime",
                    values: new
                    {
                        id = animeId
                    }
                );


            if (string.IsNullOrWhiteSpace(detailsUrl))
            {
                detailsUrl =
                    $"/Anime/Details/{animeId}";
            }


            return Redirect(
                $"{detailsUrl}#reviews"
            );
        }



        // =========================================================
        // NORMALIZE AVATAR
        // =========================================================

        private static string NormalizeAvatar(
            string? avatarStyle)
        {
            return avatarStyle switch
            {
                "sakura" =>
                    "sakura",

                "kitsune" =>
                    "kitsune",

                "blade" =>
                    "blade",

                "neko" =>
                    "neko",

                "star" =>
                    "star",

                "cyber" =>
                    "cyber",

                "wave" =>
                    "wave",

                _ =>
                    "violet"
            };
        }



        // =========================================================
        // AVATAR SYMBOL
        // =========================================================

        private static string GetAvatarSymbol(
            string? avatarStyle)
        {
            return NormalizeAvatar(
                avatarStyle
            ) switch
            {
                "sakura" =>
                    "桜",

                "kitsune" =>
                    "狐",

                "blade" =>
                    "刀",

                "neko" =>
                    "猫",

                "star" =>
                    "星",

                "cyber" =>
                    "夢",

                "wave" =>
                    "波",

                _ =>
                    "月"
            };
        }
    }
}