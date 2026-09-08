using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser>
            _userManager;


        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;

            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            // =============================================
            // ИМЯ
            // =============================================

            string userName =
                !string.IsNullOrWhiteSpace(user.UserName)
                    ? user.UserName
                    : user.Email ?? "Пользователь";


            string initial =
                !string.IsNullOrWhiteSpace(userName)
                    ? userName.Substring(0, 1).ToUpper()
                    : "A";


            // =============================================
            // ИЗБРАННОЕ
            // =============================================

            int favoritesCount =
                await _context.Favorites
                    .CountAsync(f =>
                        f.UserId == user.Id);


            // =============================================
            // ИСТОРИЯ
            // =============================================

            int viewedCount =
                await _context.ViewingHistories
                    .CountAsync(v =>
                        v.UserId == user.Id);


            var recent =
                await _context.ViewingHistories

                    .Where(v =>
                        v.UserId == user.Id)

                    .Include(v =>
                        v.Anime)

                    .OrderByDescending(v =>
                        v.ViewedAt)

                    .Take(8)

                    .Select(v =>
                        new ProfileRecentAnimeViewModel
                        {
                            Id =
                                v.Anime.Id,

                            Title =
                                v.Anime.Title,

                            PosterUrl =
                                v.Anime.PosterUrl,

                            Rating =
                                v.Anime.Rating,

                            ReleaseYear =
                                v.Anime.ReleaseYear,

                            AgeRating =
                                v.Anime.AgeRating,

                            ViewedAt =
                                v.ViewedAt
                        }
                    )

                    .ToListAsync();


            // =============================================
            // VIEWMODEL
            // =============================================

            var model =
                new ProfileViewModel
                {
                    UserName =
                        userName,

                    Email =
                        user.Email ?? "Email не указан",

                    Initial =
                        initial,

                    CreatedAt =
                        user.CreatedAt,

                    FavoritesCount =
                        favoritesCount,

                    ViewedCount =
                        viewedCount,

                    RecentAnime =
                        recent
                };


            return View(model);
        }
    }
}