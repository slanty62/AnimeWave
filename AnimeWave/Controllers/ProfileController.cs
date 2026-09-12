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

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;



        // =========================================================
        // AVATAR CATALOG
        // =========================================================

        private static readonly Dictionary<
            string,
            (string Name, string Symbol)
        > AvatarCatalog =
            new()
            {
                {
                    "violet",
                    (
                        "Лунный",
                        "月"
                    )
                },

                {
                    "sakura",
                    (
                        "Сакура",
                        "桜"
                    )
                },

                {
                    "kitsune",
                    (
                        "Кицунэ",
                        "狐"
                    )
                },

                {
                    "blade",
                    (
                        "Клинок",
                        "刀"
                    )
                },

                {
                    "neko",
                    (
                        "Нэко",
                        "猫"
                    )
                },

                {
                    "star",
                    (
                        "Звезда",
                        "星"
                    )
                },

                {
                    "cyber",
                    (
                        "Cyber",
                        "夢"
                    )
                },

                {
                    "wave",
                    (
                        "Волна",
                        "波"
                    )
                }
            };



        // =========================================================
        // THEME CATALOG
        // =========================================================

        private static readonly Dictionary<
            string,
            (string Name, string Description)
        > ThemeCatalog =
            new()
            {
                {
                    "violet",
                    (
                        "Violet",
                        "Фирменный фиолетовый AnimeWave"
                    )
                },

                {
                    "sakura",
                    (
                        "Sakura",
                        "Розовые и малиновые акценты"
                    )
                },

                {
                    "cyber",
                    (
                        "Cyber",
                        "Неоновый cyan и футуристический glow"
                    )
                },

                {
                    "ocean",
                    (
                        "Ocean",
                        "Холодные синие и голубые оттенки"
                    )
                },

                {
                    "crimson",
                    (
                        "Crimson",
                        "Насыщенные красные акценты"
                    )
                }
            };



        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context =
                context;

            _userManager =
                userManager;

            _signInManager =
                signInManager;
        }



        // =========================================================
        // PROFILE
        //
        // GET: /Profile
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



            // =====================================================
            // DISPLAY NAME
            // =====================================================

            string displayName =
                !string.IsNullOrWhiteSpace(
                    user.DisplayName
                )
                    ? user.DisplayName
                    : (
                        user.UserName
                        ??
                        user.Email
                        ??
                        "AnimeWave User"
                    );



            string initial =
                !string.IsNullOrWhiteSpace(
                    displayName
                )
                    ? displayName
                        .Trim()[0]
                        .ToString()
                        .ToUpperInvariant()
                    : "A";



            // =====================================================
            // STATISTICS
            // =====================================================

            int favoritesCount =
                await _context.Favorites

                    .AsNoTracking()

                    .CountAsync(
                        f =>
                            f.UserId ==
                            user.Id
                    );



            int viewedCount =
                await _context.ViewingHistories

                    .AsNoTracking()

                    .CountAsync(
                        v =>
                            v.UserId ==
                            user.Id
                    );



            // =====================================================
            // RECENT VIEWING HISTORY
            // =====================================================

            var recentAnime =
                await _context.ViewingHistories

                    .AsNoTracking()

                    .Where(
                        v =>
                            v.UserId ==
                            user.Id
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



            // =====================================================
            // VIEW MODEL
            // =====================================================

            var model =
                new ProfileViewModel
                {
                    UserName =
                        displayName,

                    Email =
                        user.Email
                        ?? string.Empty,

                    Initial =
                        initial,

                    AvatarStyle =
                        NormalizeAvatar(
                            user.AvatarStyle
                        ),

                    AvatarSymbol =
                        GetAvatarSymbol(
                            user.AvatarStyle
                        ),

                    CreatedAt =
                        user.CreatedAt,

                    FavoritesCount =
                        favoritesCount,

                    ViewedCount =
                        viewedCount,

                    RecentAnime =
                        recentAnime
                };



            return View(
                model
            );
        }



        // =========================================================
        // EDIT PROFILE
        //
        // GET: /Profile/Edit
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
                        ?? string.Empty,

                    Email =
                        user.Email
                        ?? string.Empty,

                    AvatarStyle =
                        NormalizeAvatar(
                            user.AvatarStyle
                        ),

                    AvatarOptions =
                        GetAvatarOptions(),

                    ThemeStyle =
                        NormalizeTheme(
                            user.ThemeStyle
                        ),

                    ThemeOptions =
                        GetThemeOptions()
                };



            return View(
                model
            );
        }



        // =========================================================
        // EDIT PROFILE
        //
        // POST: /Profile/Edit
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
            // ALWAYS RESTORE OPTIONS
            //
            // Эти списки нужны, если мы вернём View
            // после ошибки Validation.
            // =====================================================

            model.AvatarOptions =
                GetAvatarOptions();


            model.ThemeOptions =
                GetThemeOptions();



            // =====================================================
            // NORMALIZE BASIC INPUT
            // =====================================================

            model.DisplayName =
                model.DisplayName?
                    .Trim()
                ?? string.Empty;


            model.Email =
                model.Email?
                    .Trim()
                ?? string.Empty;



            // =====================================================
            // AVATAR VALIDATION
            // =====================================================

            if (
                string.IsNullOrWhiteSpace(
                    model.AvatarStyle
                )

                ||

                !AvatarCatalog.ContainsKey(
                    model.AvatarStyle
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.AvatarStyle),
                    "Выберите корректный аватар."
                );


                model.AvatarStyle =
                    NormalizeAvatar(
                        user.AvatarStyle
                    );
            }



            // =====================================================
            // THEME VALIDATION
            // =====================================================

            if (
                string.IsNullOrWhiteSpace(
                    model.ThemeStyle
                )

                ||

                !ThemeCatalog.ContainsKey(
                    model.ThemeStyle
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.ThemeStyle),
                    "Выберите корректную тему."
                );


                model.ThemeStyle =
                    NormalizeTheme(
                        user.ThemeStyle
                    );
            }



            // =====================================================
            // CHECK WHAT USER WANTS TO CHANGE
            // =====================================================

            string currentEmail =
                user.Email
                ?? string.Empty;


            bool emailChanged =
                !string.Equals(
                    currentEmail,
                    model.Email,
                    StringComparison.OrdinalIgnoreCase
                );


            bool passwordChanged =
                !string.IsNullOrWhiteSpace(
                    model.NewPassword
                );


            bool sensitiveChange =
                emailChanged
                ||
                passwordChanged;



            // =====================================================
            // CURRENT PASSWORD IS REQUIRED
            //
            // Только если пользователь меняет:
            // - Email
            // - Password
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
                    "Введите текущий пароль для изменения email или пароля."
                );
            }



            // =====================================================
            // EMAIL UNIQUENESS
            // =====================================================

            if (
                emailChanged
                &&
                !string.IsNullOrWhiteSpace(
                    model.Email
                )
            )
            {
                var userWithSameEmail =
                    await _userManager
                        .FindByEmailAsync(
                            model.Email
                        );


                if (
                    userWithSameEmail != null
                    &&
                    userWithSameEmail.Id != user.Id
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Этот email уже используется другим аккаунтом."
                    );
                }
            }



            // =====================================================
            // CURRENT PASSWORD CHECK
            // =====================================================

            if (
                sensitiveChange
                &&
                !string.IsNullOrWhiteSpace(
                    model.CurrentPassword
                )
            )
            {
                bool passwordCorrect =
                    await _userManager
                        .CheckPasswordAsync(
                            user,
                            model.CurrentPassword
                        );


                if (!passwordCorrect)
                {
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "Текущий пароль указан неверно."
                    );
                }
            }



            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                return View(
                    model
                );
            }



            // =====================================================
            // DATABASE TRANSACTION
            // =====================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();



            try
            {
                // =================================================
                // CHANGE PASSWORD
                // =================================================

                if (passwordChanged)
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
                        AddIdentityErrors(
                            passwordResult
                        );


                        return View(
                            model
                        );
                    }
                }



                // =================================================
                // CHANGE EMAIL
                // =================================================

                if (emailChanged)
                {
                    var emailResult =
                        await _userManager
                            .SetEmailAsync(
                                user,
                                model.Email
                            );


                    if (!emailResult.Succeeded)
                    {
                        AddIdentityErrors(
                            emailResult
                        );


                        return View(
                            model
                        );
                    }



                    // В AnimeWave UserName = Email.
                    //
                    // Поэтому при смене Email
                    // обновляем и UserName.

                    var usernameResult =
                        await _userManager
                            .SetUserNameAsync(
                                user,
                                model.Email
                            );


                    if (!usernameResult.Succeeded)
                    {
                        AddIdentityErrors(
                            usernameResult
                        );


                        return View(
                            model
                        );
                    }
                }



                // =================================================
                // PROFILE DATA
                // =================================================

                user.DisplayName =
                    model.DisplayName;


                user.AvatarStyle =
                    NormalizeAvatar(
                        model.AvatarStyle
                    );


                user.ThemeStyle =
                    NormalizeTheme(
                        model.ThemeStyle
                    );



                // =================================================
                // UPDATE USER
                // =================================================

                var updateResult =
                    await _userManager
                        .UpdateAsync(
                            user
                        );


                if (!updateResult.Succeeded)
                {
                    AddIdentityErrors(
                        updateResult
                    );


                    return View(
                        model
                    );
                }



                // =================================================
                // COMMIT
                // =================================================

                await transaction
                    .CommitAsync();



                // =================================================
                // REFRESH AUTH COOKIE
                // =================================================

                await _signInManager
                    .RefreshSignInAsync(
                        user
                    );



                // =================================================
                // SUCCESS TOAST
                // =================================================

                TempData["SuccessMessage"] =
                    "Профиль успешно обновлён ✓";



                return RedirectToAction(
                    nameof(Index)
                );
            }
            catch (Exception)
            {
                await transaction
                    .RollbackAsync();


                TempData["ErrorMessage"] =
                    "Не удалось обновить профиль. Попробуйте ещё раз.";


                model.AvatarOptions =
                    GetAvatarOptions();


                model.ThemeOptions =
                    GetThemeOptions();


                return View(
                    model
                );
            }
        }



        // =========================================================
        // ADD IDENTITY ERRORS TO MODEL STATE
        // =========================================================

        private void AddIdentityErrors(
            IdentityResult result)
        {
            foreach (
                var error
                in result.Errors
            )
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }
        }



        // =========================================================
        // GET AVATAR OPTIONS
        // =========================================================

        private static List<ProfileAvatarOptionViewModel>
            GetAvatarOptions()
        {
            return AvatarCatalog

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
        // GET AVATAR SYMBOL
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



        // =========================================================
        // GET THEME OPTIONS
        // =========================================================

        private static List<ProfileThemeOptionViewModel>
            GetThemeOptions()
        {
            return ThemeCatalog

                .Select(
                    theme =>
                        new ProfileThemeOptionViewModel
                        {
                            Id =
                                theme.Key,

                            Name =
                                theme.Value.Name,

                            Description =
                                theme.Value.Description
                        }
                )

                .ToList();
        }



        // =========================================================
        // NORMALIZE THEME
        // =========================================================

        private static string NormalizeTheme(
            string? themeStyle)
        {
            return themeStyle switch
            {
                "sakura" =>
                    "sakura",

                "cyber" =>
                    "cyber",

                "ocean" =>
                    "ocean",

                "crimson" =>
                    "crimson",

                _ =>
                    "violet"
            };
        }
    }
}