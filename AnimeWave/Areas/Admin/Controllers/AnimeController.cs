using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnimeController : Controller
    {
        private readonly ApplicationDbContext _context;


        public AnimeController(
            ApplicationDbContext context)
        {
            _context = context;
        }



        // =========================================================
        // СПИСОК АНИМЕ
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var animes =
                await _context.Animes

                    .Include(a =>
                        a.AnimeGenres)

                    .ThenInclude(ag =>
                        ag.Genre)

                    .OrderBy(a =>
                        a.Title)

                    .ToListAsync();


            return View(animes);
        }



        // =========================================================
        // ДОБАВЛЕНИЕ АНИМЕ
        // GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new AnimeFormViewModel
                {
                    ReleaseYear =
                        DateTime.Now.Year,

                    Rating =
                        8.0m,

                    AgeRating =
                        "16+",

                    EpisodesCount =
                        12,

                    Status =
                        "Выходит",

                    Genres =
                        await GetGenresAsync()
                };


            return View(model);
        }



        // =========================================================
        // ДОБАВЛЕНИЕ АНИМЕ
        // POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AnimeFormViewModel model)
        {
            // =====================================================
            // VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                model.Genres =
                    await GetGenresAsync(
                        model.SelectedGenreIds
                    );


                return View(model);
            }



            // =====================================================
            // CREATE ANIME
            // =====================================================

            var anime =
                new Anime
                {
                    Title =
                        model.Title.Trim(),


                    OriginalTitle =
                        string.IsNullOrWhiteSpace(
                            model.OriginalTitle
                        )
                            ? null
                            : model.OriginalTitle.Trim(),


                    Description =
                        string.IsNullOrWhiteSpace(
                            model.Description
                        )
                            ? null
                            : model.Description.Trim(),


                    ReleaseYear =
                        model.ReleaseYear,


                    Rating =
                        model.Rating,


                    AgeRating =
                        string.IsNullOrWhiteSpace(
                            model.AgeRating
                        )
                            ? null
                            : model.AgeRating.Trim(),


                    EpisodesCount =
                        model.EpisodesCount,


                    Status =
                        string.IsNullOrWhiteSpace(
                            model.Status
                        )
                            ? "Выходит"
                            : model.Status.Trim(),


                    PosterUrl =
                        string.IsNullOrWhiteSpace(
                            model.PosterUrl
                        )
                            ? null
                            : model.PosterUrl.Trim(),


                    BannerUrl =
                        string.IsNullOrWhiteSpace(
                            model.BannerUrl
                        )
                            ? null
                            : model.BannerUrl.Trim()
                };


            _context.Animes.Add(anime);


            await _context.SaveChangesAsync();



            // =====================================================
            // GENRES
            // =====================================================

            if (
                model.SelectedGenreIds != null
                &&
                model.SelectedGenreIds.Any()
            )
            {
                var genreIds =
                    model.SelectedGenreIds
                        .Distinct()
                        .ToList();


                foreach (var genreId in genreIds)
                {
                    _context.AnimeGenres.Add(
                        new AnimeGenre
                        {
                            AnimeId =
                                anime.Id,

                            GenreId =
                                genreId
                        }
                    );
                }


                await _context.SaveChangesAsync();
            }



            // =====================================================
            // TOAST
            // =====================================================

            TempData["SuccessMessage"] =
                $"Аниме «{anime.Title}» успешно добавлено ✓";


            return RedirectToAction(
                nameof(Index)
            );
        }



        // =========================================================
        // РЕДАКТИРОВАНИЕ
        // GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var anime =
                await _context.Animes

                    .Include(a =>
                        a.AnimeGenres)

                    .FirstOrDefaultAsync(
                        a =>
                            a.Id == id
                    );


            if (anime == null)
            {
                return NotFound();
            }



            var selectedGenreIds =
                anime.AnimeGenres

                    .Select(ag =>
                        ag.GenreId)

                    .ToList();



            var model =
                new AnimeFormViewModel
                {
                    Id =
                        anime.Id,


                    Title =
                        anime.Title,


                    OriginalTitle =
                        anime.OriginalTitle,


                    Description =
                        anime.Description,


                    ReleaseYear =
                        anime.ReleaseYear,


                    Rating =
                        anime.Rating,


                    AgeRating =
                        anime.AgeRating,


                    EpisodesCount =
                        anime.EpisodesCount,


                    Status =
                        anime.Status,


                    PosterUrl =
                        anime.PosterUrl,


                    BannerUrl =
                        anime.BannerUrl,


                    SelectedGenreIds =
                        selectedGenreIds,


                    Genres =
                        await GetGenresAsync(
                            selectedGenreIds
                        )
                };


            return View(model);
        }



        // =========================================================
        // РЕДАКТИРОВАНИЕ
        // POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AnimeFormViewModel model)
        {
            var anime =
                await _context.Animes

                    .Include(a =>
                        a.AnimeGenres)

                    .FirstOrDefaultAsync(
                        a =>
                            a.Id == model.Id
                    );


            if (anime == null)
            {
                return NotFound();
            }



            // =====================================================
            // VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                model.Genres =
                    await GetGenresAsync(
                        model.SelectedGenreIds
                    );


                return View(model);
            }



            // =====================================================
            // UPDATE BASIC DATA
            // =====================================================

            anime.Title =
                model.Title.Trim();


            anime.OriginalTitle =
                string.IsNullOrWhiteSpace(
                    model.OriginalTitle
                )
                    ? null
                    : model.OriginalTitle.Trim();


            anime.Description =
                string.IsNullOrWhiteSpace(
                    model.Description
                )
                    ? null
                    : model.Description.Trim();


            anime.ReleaseYear =
                model.ReleaseYear;


            anime.Rating =
                model.Rating;


            anime.AgeRating =
                string.IsNullOrWhiteSpace(
                    model.AgeRating
                )
                    ? null
                    : model.AgeRating.Trim();


            anime.EpisodesCount =
                model.EpisodesCount;


            anime.Status =
                string.IsNullOrWhiteSpace(
                    model.Status
                )
                    ? "Выходит"
                    : model.Status.Trim();


            anime.PosterUrl =
                string.IsNullOrWhiteSpace(
                    model.PosterUrl
                )
                    ? null
                    : model.PosterUrl.Trim();


            anime.BannerUrl =
                string.IsNullOrWhiteSpace(
                    model.BannerUrl
                )
                    ? null
                    : model.BannerUrl.Trim();



            // =====================================================
            // UPDATE GENRES
            //
            // Не удаляем все связи вслепую,
            // а сравниваем старые и новые.
            // =====================================================

            var selectedIds =
                (
                    model.SelectedGenreIds
                    ??
                    new List<int>()
                )
                .Distinct()
                .ToHashSet();



            var currentIds =
                anime.AnimeGenres

                    .Select(ag =>
                        ag.GenreId)

                    .ToHashSet();



            // Удаляем жанры,
            // которые были сняты пользователем

            var relationsToRemove =
                anime.AnimeGenres

                    .Where(ag =>
                        !selectedIds.Contains(
                            ag.GenreId
                        )
                    )

                    .ToList();


            if (relationsToRemove.Any())
            {
                _context.AnimeGenres.RemoveRange(
                    relationsToRemove
                );
            }



            // Добавляем новые жанры

            var genresToAdd =
                selectedIds

                    .Where(id =>
                        !currentIds.Contains(id))

                    .ToList();


            foreach (var genreId in genresToAdd)
            {
                _context.AnimeGenres.Add(
                    new AnimeGenre
                    {
                        AnimeId =
                            anime.Id,

                        GenreId =
                            genreId
                    }
                );
            }



            await _context.SaveChangesAsync();



            // =====================================================
            // TOAST
            // =====================================================

            TempData["SuccessMessage"] =
                $"Аниме «{anime.Title}» успешно изменено ✓";


            return RedirectToAction(
                nameof(Index)
            );
        }



        // =========================================================
        // УДАЛЕНИЕ
        // ТОЛЬКО POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            var anime =
                await _context.Animes

                    .FirstOrDefaultAsync(
                        a =>
                            a.Id == id
                    );


            if (anime == null)
            {
                TempData["ErrorMessage"] =
                    "Аниме не найдено.";

                return RedirectToAction(
                    nameof(Index)
                );
            }



            string animeTitle =
                anime.Title;



            // =====================================================
            // ANIME GENRES
            // =====================================================

            var animeGenres =
                await _context.AnimeGenres

                    .Where(ag =>
                        ag.AnimeId == id)

                    .ToListAsync();


            if (animeGenres.Any())
            {
                _context.AnimeGenres.RemoveRange(
                    animeGenres
                );
            }



            // =====================================================
            // FAVORITES
            // =====================================================

            var favorites =
                await _context.Favorites

                    .Where(f =>
                        f.AnimeId == id)

                    .ToListAsync();


            if (favorites.Any())
            {
                _context.Favorites.RemoveRange(
                    favorites
                );
            }



            // =====================================================
            // OLD EPISODES
            //
            // Управление сериями мы убрали,
            // но если старые записи ещё существуют,
            // они будут удалены вместе с аниме.
            // =====================================================

            var episodes =
                await _context.Episodes

                    .Where(e =>
                        e.AnimeId == id)

                    .ToListAsync();


            if (episodes.Any())
            {
                _context.Episodes.RemoveRange(
                    episodes
                );
            }



            // =====================================================
            // ВАЖНО:
            // ViewingHistories вручную здесь не запрашиваем.
            //
            // Когда таблица истории будет создана миграцией,
            // связь Anime -> ViewingHistory настроена на Cascade,
            // поэтому PostgreSQL удалит историю автоматически.
            //
            // Это также позволяет удалению работать,
            // даже если миграция профиля пока ещё не применена.
            // =====================================================



            // =====================================================
            // DELETE ANIME
            // =====================================================

            _context.Animes.Remove(
                anime
            );



            try
            {
                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    $"Аниме «{animeTitle}» успешно удалено ✓";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    $"Не удалось удалить аниме «{animeTitle}».";

                return RedirectToAction(
                    nameof(Index)
                );
            }



            return RedirectToAction(
                nameof(Index)
            );
        }



        // =========================================================
        // СПИСОК ЖАНРОВ
        // =========================================================

        private async Task<List<SelectListItem>>
            GetGenresAsync(
                IEnumerable<int>? selectedIds = null)
        {
            var selected =
                selectedIds?.ToHashSet()
                ??
                new HashSet<int>();



            return await _context.Genres

                .OrderBy(g =>
                    g.Name)

                .Select(g =>
                    new SelectListItem
                    {
                        Value =
                            g.Id.ToString(),

                        Text =
                            g.Name,

                        Selected =
                            selected.Contains(
                                g.Id
                            )
                    }
                )

                .ToListAsync();
        }
    }
}