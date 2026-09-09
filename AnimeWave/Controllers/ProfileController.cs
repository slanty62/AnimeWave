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

        private readonly SignInManager<ApplicationUser>
            _signInManager;


        // =========================================================
        // AVATARS
        // =========================================================

        private static readonly
            Dictionary<string, (string Name, string Symbol)>
            AvatarCatalog =
                new()
                {
                    {
                        "violet",
                        ("Лунный", "月")
                    },

                    {
                        "sakura",
                        ("Сакура", "桜")
                    },

                    {
                        "kitsune",
                        ("Кицунэ", "狐")
                    },

                    {
                        "blade",
                        ("Клинок", "刀")
                    },

                    {
                        "neko",
                        ("Нэко", "猫")
                    },

                    {
                        "star",
                        ("Звезда", "星")
                    },

                    {
                        "cyber",
                        ("Cyber", "夢")
                    },

                    {
                        "wave",
                        ("Волна", "波")
                    }
                };


        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;

            _userManager = userManager;

            _signInManager = signInManager;
        }


        // =========================================================
        // PROFILE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager
                    .GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            string displayName =
                !string.IsNullOrWhiteSpace(
                    user.DisplayName
                )
                    ? user.DisplayName
                    : user.UserName
                        ?? user.Email
                        ?? "Пользователь";


            string initial =
                !string.IsNullOrWhiteSpace(displayName)
                    ? displayName[..1].ToUpper()
                    : "A";


            var avatar =
                GetAvatar(
                    user.AvatarStyle
                );


            int favoritesCount =
                await _context.Favorites
                    .CountAsync(
                        f =>
                            f.UserId == user.Id
                    );


            int viewedCount =
                await _context.ViewingHistories
                    .CountAsync(
                        v =>
                            v.UserId == user.Id
                    );


            var recent =
                await _context.ViewingHistories

                    .Where(
                        v =>
                            v.UserId == user.Id
                    )

                    .Include(
                        v =>
                            v.Anime
                    )

                    .OrderByDescending(
                        v =>
                            v.ViewedAt
                    )

                    .Take(8)

                    .Select(
                        v =>
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


            var model =
                new ProfileViewModel
                {
                    UserName =
                        displayName,

                    Email =
                        user.Email
                        ?? "Email не указан",

                    Initial =
                        initial,

                    AvatarStyle =
                        NormalizeAvatar(
                            user.AvatarStyle
                        ),

                    AvatarSymbol =
                        avatar.Symbol,

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



        // =========================================================
        // EDIT
        // GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user =
                await _userManager
                    .GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            var model =
                new ProfileEditViewModel
                {
                    DisplayName =
                        user.DisplayName
                        ??
                        user.UserName
                        ??
                        string.Empty,

                    Email =
                        user.Email
                        ??
                        string.Empty,

                    AvatarStyle =
                        NormalizeAvatar(
                            user.AvatarStyle
                        )
                };


            FillAvatarOptions(
                model
            );


            return View(model);
        }



        // =========================================================
        // EDIT
        // POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProfileEditViewModel model)
        {
            var user =
                await _userManager
                    .GetUserAsync(User);


            if (user == null)
            {
                return Challenge();
            }


            // =====================================================
            // AVATAR VALIDATION
            // =====================================================

            if (
                !AvatarCatalog.ContainsKey(
                    model.AvatarStyle
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.AvatarStyle),
                    "Выбран неизвестный аватар."
                );
            }



            string newEmail =
                model.Email.Trim();


            string displayName =
                model.DisplayName.Trim();



            bool emailChanged =
                !string.Equals(
                    user.Email,
                    newEmail,
                    StringComparison.OrdinalIgnoreCase
                );


            bool passwordChangeRequested =
                !string.IsNullOrWhiteSpace(
                    model.NewPassword
                );


            bool sensitiveChange =
                emailChanged
                ||
                passwordChangeRequested;



            // =====================================================
            // CURRENT PASSWORD REQUIRED
            // =====================================================

            if (
                sensitiveChange
                &&
                string.IsNullOrWhiteSpace(
                    model.CurrentPassword
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.CurrentPassword),
                    "Для изменения email или пароля введите текущий пароль."
                );
            }



            // =====================================================
            // EMAIL UNIQUE
            // =====================================================

            if (emailChanged)
            {
                var existingUser =
                    await _userManager
                        .FindByEmailAsync(
                            newEmail
                        );


                if (
                    existingUser != null
                    &&
                    existingUser.Id != user.Id
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Пользователь с таким email уже существует."
                    );
                }
            }



            if (!ModelState.IsValid)
            {
                FillAvatarOptions(
                    model
                );


                return View(model);
            }



            // =====================================================
            // CHECK PASSWORD
            // =====================================================

            if (sensitiveChange)
            {
                bool passwordCorrect =
                    await _userManager
                        .CheckPasswordAsync(
                            user,
                            model.CurrentPassword!
                        );


                if (!passwordCorrect)
                {
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "Текущий пароль указан неверно."
                    );


                    FillAvatarOptions(
                        model
                    );


                    return View(model);
                }
            }



            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =================================================
                // CHANGE PASSWORD
                // =================================================

                if (passwordChangeRequested)
                {
                    var passwordResult =
                        await _userManager
                            .ChangePasswordAsync(
                                user,
                                model.CurrentPassword!,
                                model.NewPassword!
                            );


                    if (!passwordResult.Succeeded)
                    {
                        await transaction
                            .RollbackAsync();


                        foreach (
                            var error
                            in passwordResult.Errors
                        )
                        {
                            ModelState.AddModelError(
                                nameof(model.NewPassword),
                                error.Description
                            );
                        }


                        FillAvatarOptions(
                            model
                        );


                        return View(model);
                    }
                }



                // =================================================
                // EMAIL
                // =================================================

                if (emailChanged)
                {
                    var emailResult =
                        await _userManager
                            .SetEmailAsync(
                                user,
                                newEmail
                            );


                    if (!emailResult.Succeeded)
                    {
                        await transaction
                            .RollbackAsync();


                        foreach (
                            var error
                            in emailResult.Errors
                        )
                        {
                            ModelState.AddModelError(
                                nameof(model.Email),
                                error.Description
                            );
                        }


                        FillAvatarOptions(
                            model
                        );


                        return View(model);
                    }



                    // У тебя UserName используется как email,
                    // поэтому обновляем его одновременно.

                    var userNameResult =
                        await _userManager
                            .SetUserNameAsync(
                                user,
                                newEmail
                            );


                    if (!userNameResult.Succeeded)
                    {
                        await transaction
                            .RollbackAsync();


                        foreach (
                            var error
                            in userNameResult.Errors
                        )
                        {
                            ModelState.AddModelError(
                                nameof(model.Email),
                                error.Description
                            );
                        }


                        FillAvatarOptions(
                            model
                        );


                        return View(model);
                    }
                }



                // =================================================
                // DISPLAY NAME + AVATAR
                // =================================================

                user.DisplayName =
                    displayName;


                user.AvatarStyle =
                    NormalizeAvatar(
                        model.AvatarStyle
                    );


                var updateResult =
                    await _userManager
                        .UpdateAsync(
                            user
                        );


                if (!updateResult.Succeeded)
                {
                    await transaction
                        .RollbackAsync();


                    foreach (
                        var error
                        in updateResult.Errors
                    )
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description
                        );
                    }


                    FillAvatarOptions(
                        model
                    );


                    return View(model);
                }



                await transaction
                    .CommitAsync();



                // Обновляем cookie пользователя,
                // чтобы изменения сразу применились.

                await _signInManager
                    .RefreshSignInAsync(
                        user
                    );



                TempData["SuccessMessage"] =
                    "Профиль успешно обновлён ✓";


                return RedirectToAction(
                    nameof(Index)
                );
            }
            catch
            {
                await transaction
                    .RollbackAsync();


                ModelState.AddModelError(
                    string.Empty,
                    "Не удалось сохранить изменения. Попробуйте ещё раз."
                );


                FillAvatarOptions(
                    model
                );


                return View(model);
            }
        }



        // =========================================================
        // AVATAR HELPERS
        // =========================================================

        private static string NormalizeAvatar(
            string? avatarStyle)
        {
            if (
                !string.IsNullOrWhiteSpace(
                    avatarStyle
                )
                &&
                AvatarCatalog.ContainsKey(
                    avatarStyle
                )
            )
            {
                return avatarStyle;
            }


            return "violet";
        }


        private static (
            string Name,
            string Symbol
        ) GetAvatar(
            string? avatarStyle)
        {
            string normalized =
                NormalizeAvatar(
                    avatarStyle
                );


            return AvatarCatalog[
                normalized
            ];
        }


        private static void FillAvatarOptions(
            ProfileEditViewModel model)
        {
            model.AvatarOptions =
                AvatarCatalog

                    .Select(
                        avatar =>
                            new ProfileAvatarOptionViewModel
                            {
                                Id =
                                    avatar.Key,

                                Name =
                                    avatar.Value.Name,

                                Symbol =
                                    avatar.Value.Symbol
                            }
                    )

                    .ToList();
        }
    }
}