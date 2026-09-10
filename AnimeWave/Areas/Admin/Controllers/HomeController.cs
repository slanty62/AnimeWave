using AnimeWave.Data;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;


        public HomeController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // ADMIN DASHBOARD
        // GET: /Admin
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // =====================================================
            // MAIN COUNTERS
            // =====================================================

            int animeCount =
                await _context.Animes
                    .CountAsync();


            int usersCount =
                await _context.Users
                    .CountAsync();


            int favoritesCount =
                await _context.Favorites
                    .CountAsync();


            // Сейчас ViewingHistories хранит уникальные
            // просмотренные тайтлы пользователей.
            int viewsCount =
                await _context.ViewingHistories
                    .CountAsync();


            // Систему отзывов пока ещё не создавали.
            int reviewsCount = 0;



            // =====================================================
            // ANIME STATISTICS
            // =====================================================

            int completedAnimeCount =
                await _context.Animes
                    .CountAsync(
                        a =>
                            a.Status == "Завершён"
                    );


            int ongoingAnimeCount =
                await _context.Animes
                    .CountAsync(
                        a =>
                            a.Status == "Выходит"
                    );


            decimal averageRating = 0;


            if (animeCount > 0)
            {
                averageRating =
                    await _context.Animes
                        .AverageAsync(
                            a => a.Rating
                        );
            }



            // =====================================================
            // GENRE STATISTICS
            // =====================================================

            var genreData =
                await _context.Genres

                    .AsNoTracking()

                    .Select(
                        g =>
                            new
                            {
                                g.Name,

                                Count =
                                    g.AnimeGenres.Count()
                            }
                    )

                    .Where(
                        g =>
                            g.Count > 0
                    )

                    .OrderByDescending(
                        g =>
                            g.Count
                    )

                    .ThenBy(
                        g =>
                            g.Name
                    )

                    .Take(8)

                    .ToListAsync();



            int maxGenreCount =
                genreData.Any()
                    ? genreData.Max(
                        g => g.Count
                    )
                    : 0;


            var genreStats =
                genreData

                    .Select(
                        g =>
                            new AdminGenreStatViewModel
                            {
                                Name =
                                    g.Name,

                                Count =
                                    g.Count,

                                Percentage =
                                    maxGenreCount > 0
                                        ? (
                                            (double)g.Count
                                            /
                                            maxGenreCount
                                            *
                                            100
                                        )
                                        : 0
                            }
                    )

                    .ToList();



            // =====================================================
            // RECENT USERS
            // =====================================================

            var users =
                await _context.Users

                    .AsNoTracking()

                    .OrderByDescending(
                        u =>
                            u.CreatedAt
                    )

                    .Take(6)

                    .ToListAsync();



            var recentUsers =
                users

                    .Select(
                        u =>
                            new AdminRecentUserViewModel
                            {
                                Id =
                                    u.Id,

                                DisplayName =
                                    !string.IsNullOrWhiteSpace(
                                        u.DisplayName
                                    )
                                        ? u.DisplayName
                                        : u.UserName
                                            ?? "Пользователь",

                                Email =
                                    u.Email
                                    ?? "Email не указан",

                                AvatarStyle =
                                    NormalizeAvatar(
                                        u.AvatarStyle
                                    ),

                                AvatarSymbol =
                                    GetAvatarSymbol(
                                        u.AvatarStyle
                                    ),

                                CreatedAt =
                                    u.CreatedAt
                            }
                    )

                    .ToList();



            // =====================================================
            // RECENT ANIME
            //
            // Anime пока не имеет CreatedAt,
            // поэтому последние добавленные определяем по ID.
            // =====================================================

            var recentAnime =
                await _context.Animes

                    .AsNoTracking()

                    .OrderByDescending(
                        a =>
                            a.Id
                    )

                    .Take(6)

                    .Select(
                        a =>
                            new AdminRecentAnimeViewModel
                            {
                                Id =
                                    a.Id,

                                Title =
                                    a.Title,

                                OriginalTitle =
                                    a.OriginalTitle,

                                PosterUrl =
                                    a.PosterUrl,

                                Rating =
                                    a.Rating,

                                ReleaseYear =
                                    a.ReleaseYear,

                                Status =
                                    a.Status
                            }
                    )

                    .ToListAsync();



            // =====================================================
            // VIEW MODEL
            // =====================================================

            var model =
                new AdminDashboardViewModel
                {
                    AnimeCount =
                        animeCount,

                    UsersCount =
                        usersCount,

                    FavoritesCount =
                        favoritesCount,

                    ViewsCount =
                        viewsCount,

                    ReviewsCount =
                        reviewsCount,

                    CompletedAnimeCount =
                        completedAnimeCount,

                    OngoingAnimeCount =
                        ongoingAnimeCount,

                    AverageRating =
                        averageRating,

                    GenreStats =
                        genreStats,

                    RecentUsers =
                        recentUsers,

                    RecentAnime =
                        recentAnime
                };


            return View(model);
        }



        // =========================================================
        // AVATAR
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
            return avatarStyle switch
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