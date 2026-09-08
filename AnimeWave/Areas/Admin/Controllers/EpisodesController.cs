using AnimeWave.Data;
using AnimeWave.Models;
using AnimeWave.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EpisodesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EpisodesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // СПИСОК СЕРИЙ
        //
        // /Admin/Episodes?animeId=1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(int animeId)
        {
            if (animeId <= 0)
            {
                return RedirectToAction(
                    "Index",
                    "Anime",
                    new
                    {
                        area = "Admin"
                    }
                );
            }


            var anime = await _context.Animes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == animeId);


            if (anime == null)
            {
                return RedirectToAction(
                    "Index",
                    "Anime",
                    new
                    {
                        area = "Admin"
                    }
                );
            }


            var episodes = await _context.Episodes
                .AsNoTracking()
                .Where(e => e.AnimeId == animeId)
                .OrderBy(e => e.EpisodeNumber)
                .ToListAsync();


            ViewBag.Anime = anime;


            return View(episodes);
        }


        // =========================================================
        // ДОБАВЛЕНИЕ СЕРИИ
        // GET
        //
        // /Admin/Episodes/Create?animeId=1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(int animeId)
        {
            if (animeId <= 0)
            {
                return RedirectToAction(
                    "Index",
                    "Anime",
                    new
                    {
                        area = "Admin"
                    }
                );
            }


            var anime = await _context.Animes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == animeId);


            if (anime == null)
            {
                return RedirectToAction(
                    "Index",
                    "Anime",
                    new
                    {
                        area = "Admin"
                    }
                );
            }


            // Следующий номер серии
            int nextEpisodeNumber = 1;


            var lastEpisode = await _context.Episodes
                .AsNoTracking()
                .Where(e => e.AnimeId == animeId)
                .OrderByDescending(e => e.EpisodeNumber)
                .FirstOrDefaultAsync();


            if (lastEpisode != null)
            {
                nextEpisodeNumber =
                    lastEpisode.EpisodeNumber + 1;
            }


            var model = new EpisodeFormViewModel
            {
                AnimeId = anime.Id,
                EpisodeNumber = nextEpisodeNumber,
                DurationMinutes = 24
            };


            ViewBag.Anime = anime;


            return View(model);
        }


        // =========================================================
        // ДОБАВЛЕНИЕ СЕРИИ
        // POST
        //
        // Данные принимаем напрямую из формы.
        // Так мы исключаем проблему с потерей AnimeId.
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromQuery] int animeId,

            [FromForm(Name = "AnimeId")]
            int formAnimeId,

            [FromForm(Name = "EpisodeNumber")]
            int episodeNumber,

            [FromForm(Name = "Title")]
            string? title,

            [FromForm(Name = "Description")]
            string? description,

            [FromForm(Name = "DurationMinutes")]
            int durationMinutes,

            [FromForm(Name = "ThumbnailUrl")]
            string? thumbnailUrl,

            [FromForm(Name = "ReleaseDate")]
            DateTime? releaseDate)
        {
            // =====================================================
            // ОПРЕДЕЛЯЕМ ANIME ID
            //
            // Сначала берём hidden AnimeId.
            // Если его нет — берём animeId из URL.
            // =====================================================

            int finalAnimeId =
                formAnimeId > 0
                    ? formAnimeId
                    : animeId;


            // =====================================================
            // СОБИРАЕМ VIEWMODEL
            //
            // Если будет ошибка, введённые данные
            // останутся в форме.
            // =====================================================

            var model = new EpisodeFormViewModel
            {
                AnimeId = finalAnimeId,

                EpisodeNumber = episodeNumber,

                Title = title ?? string.Empty,

                Description = description,

                DurationMinutes = durationMinutes,

                ThumbnailUrl = thumbnailUrl,

                ReleaseDate = releaseDate
            };


            // =====================================================
            // ПРОВЕРКА ANIME ID
            // =====================================================

            if (finalAnimeId <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Не удалось определить аниме. AnimeId не был передан."
                );

                return View(model);
            }


            // =====================================================
            // ИЩЕМ ANIME
            // =====================================================

            var anime = await _context.Animes
                .FirstOrDefaultAsync(a =>
                    a.Id == finalAnimeId);


            if (anime == null)
            {
                ModelState.AddModelError(
                    "",
                    $"Аниме с ID {finalAnimeId} не найдено."
                );

                return View(model);
            }


            ViewBag.Anime = anime;


            // =====================================================
            // ПРОВЕРКА НОМЕРА СЕРИИ
            // =====================================================

            if (episodeNumber <= 0)
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.EpisodeNumber),
                    "Номер серии должен быть больше нуля."
                );
            }


            // =====================================================
            // ПРОВЕРКА НАЗВАНИЯ
            // =====================================================

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.Title),
                    "Введите название серии."
                );
            }
            else if (title.Length > 200)
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.Title),
                    "Название не должно превышать 200 символов."
                );
            }


            // =====================================================
            // ПРОВЕРКА ОПИСАНИЯ
            // =====================================================

            if (!string.IsNullOrWhiteSpace(description)
                && description.Length > 1000)
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.Description),
                    "Описание не должно превышать 1000 символов."
                );
            }


            // =====================================================
            // ПРОВЕРКА ДЛИТЕЛЬНОСТИ
            // =====================================================

            if (durationMinutes <= 0)
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.DurationMinutes),
                    "Продолжительность должна быть больше нуля."
                );
            }


            // =====================================================
            // ПРОВЕРКА ПУТИ К ИЗОБРАЖЕНИЮ
            // =====================================================

            if (!string.IsNullOrWhiteSpace(thumbnailUrl)
                && thumbnailUrl.Length > 500)
            {
                ModelState.AddModelError(
                    nameof(EpisodeFormViewModel.ThumbnailUrl),
                    "Путь к изображению слишком длинный."
                );
            }


            // =====================================================
            // ПРОВЕРЯЕМ ДУБЛИКАТ НОМЕРА СЕРИИ
            // =====================================================

            if (episodeNumber > 0)
            {
                bool duplicateEpisode =
                    await _context.Episodes.AnyAsync(e =>
                        e.AnimeId == finalAnimeId
                        &&
                        e.EpisodeNumber == episodeNumber);


                if (duplicateEpisode)
                {
                    ModelState.AddModelError(
                        nameof(EpisodeFormViewModel.EpisodeNumber),
                        $"Серия №{episodeNumber} уже существует."
                    );
                }
            }


            // =====================================================
            // ЕСЛИ ЕСТЬ ОШИБКИ
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Anime = anime;

                return View(model);
            }


            // =====================================================
            // СОЗДАЁМ СЕРИЮ
            // =====================================================

            var episode = new Episode
            {
                AnimeId = finalAnimeId,

                EpisodeNumber = episodeNumber,

                Title = title!.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(description)
                        ? null
                        : description.Trim(),

                DurationMinutes = durationMinutes,

                ThumbnailUrl =
                    string.IsNullOrWhiteSpace(thumbnailUrl)
                        ? null
                        : thumbnailUrl.Trim(),

                ReleaseDate = releaseDate
            };


            // =====================================================
            // СОХРАНЯЕМ В POSTGRESQL
            // =====================================================

            try
            {
                _context.Episodes.Add(episode);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    "",
                    "Ошибка базы данных. Серия не была сохранена."
                );


                // Только для режима разработки можно посмотреть
                // ошибку в Output Visual Studio.
                Console.WriteLine(
                    "Ошибка сохранения Episode:"
                );

                Console.WriteLine(
                    ex.ToString()
                );


                ViewBag.Anime = anime;

                return View(model);
            }


            // =====================================================
            // УСПЕШНО
            //
            // EpisodesCount у Anime НЕ меняем.
            // Это общее количество серий аниме,
            // а здесь у нас демонстрационные серии.
            // =====================================================

            TempData["SuccessMessage"] =
                $"Серия №{episode.EpisodeNumber} успешно добавлена.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    animeId = finalAnimeId
                }
            );
        }


        // =========================================================
        // РЕДАКТИРОВАНИЕ СЕРИИ
        // GET
        //
        // /Admin/Episodes/Edit/1
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }


            var episode = await _context.Episodes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);


            if (episode == null)
            {
                return NotFound();
            }


            var anime = await _context.Animes
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id == episode.AnimeId);


            if (anime == null)
            {
                return NotFound();
            }


            var model = new EpisodeFormViewModel
            {
                Id = episode.Id,

                AnimeId = episode.AnimeId,

                EpisodeNumber = episode.EpisodeNumber,

                Title = episode.Title,

                Description = episode.Description,

                DurationMinutes = episode.DurationMinutes,

                ThumbnailUrl = episode.ThumbnailUrl,

                ReleaseDate = episode.ReleaseDate
            };


            ViewBag.Anime = anime;


            return View(model);
        }


        // =========================================================
        // РЕДАКТИРОВАНИЕ СЕРИИ
        // POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EpisodeFormViewModel model)
        {
            // =====================================================
            // ИЩЕМ СЕРИЮ
            // =====================================================

            var episode = await _context.Episodes
                .FirstOrDefaultAsync(e =>
                    e.Id == model.Id);


            if (episode == null)
            {
                return NotFound();
            }


            // =====================================================
            // ИЩЕМ ANIME
            // =====================================================

            var anime = await _context.Animes
                .FirstOrDefaultAsync(a =>
                    a.Id == episode.AnimeId);


            if (anime == null)
            {
                return NotFound();
            }


            ViewBag.Anime = anime;


            // AnimeId менять через форму нельзя
            model.AnimeId =
                episode.AnimeId;


            // =====================================================
            // ПРОВЕРКИ
            // =====================================================

            if (model.EpisodeNumber <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.EpisodeNumber),
                    "Номер серии должен быть больше нуля."
                );
            }


            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "Введите название серии."
                );
            }


            if (model.DurationMinutes <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.DurationMinutes),
                    "Продолжительность должна быть больше нуля."
                );
            }


            // =====================================================
            // ПРОВЕРКА ДУБЛИКАТА
            // =====================================================

            bool duplicateEpisode =
                await _context.Episodes.AnyAsync(e =>
                    e.AnimeId == episode.AnimeId
                    &&
                    e.EpisodeNumber == model.EpisodeNumber
                    &&
                    e.Id != episode.Id);


            if (duplicateEpisode)
            {
                ModelState.AddModelError(
                    nameof(model.EpisodeNumber),
                    $"Серия №{model.EpisodeNumber} уже существует."
                );
            }


            if (!ModelState.IsValid)
            {
                ViewBag.Anime = anime;

                return View(model);
            }


            // =====================================================
            // ОБНОВЛЯЕМ
            // =====================================================

            episode.EpisodeNumber =
                model.EpisodeNumber;


            episode.Title =
                model.Title.Trim();


            episode.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();


            episode.DurationMinutes =
                model.DurationMinutes;


            episode.ThumbnailUrl =
                string.IsNullOrWhiteSpace(model.ThumbnailUrl)
                    ? null
                    : model.ThumbnailUrl.Trim();


            episode.ReleaseDate =
                model.ReleaseDate;


            // =====================================================
            // СОХРАНЯЕМ
            // =====================================================

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(
                    "Ошибка редактирования Episode:"
                );

                Console.WriteLine(
                    ex.ToString()
                );


                ModelState.AddModelError(
                    "",
                    "Не удалось сохранить изменения."
                );


                ViewBag.Anime = anime;

                return View(model);
            }


            TempData["SuccessMessage"] =
                $"Серия №{episode.EpisodeNumber} успешно изменена.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    animeId = episode.AnimeId
                }
            );
        }


        // =========================================================
        // УДАЛЕНИЕ СЕРИИ
        // POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }


            var episode = await _context.Episodes
                .FirstOrDefaultAsync(e =>
                    e.Id == id);


            if (episode == null)
            {
                return NotFound();
            }


            int animeId =
                episode.AnimeId;


            int episodeNumber =
                episode.EpisodeNumber;


            // =====================================================
            // УДАЛЯЕМ
            // =====================================================

            try
            {
                _context.Episodes.Remove(episode);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(
                    "Ошибка удаления Episode:"
                );

                Console.WriteLine(
                    ex.ToString()
                );


                TempData["ErrorMessage"] =
                    "Не удалось удалить серию.";


                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        animeId = animeId
                    }
                );
            }


            TempData["SuccessMessage"] =
                $"Серия №{episodeNumber} успешно удалена.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    animeId = animeId
                }
            );
        }
    }
}