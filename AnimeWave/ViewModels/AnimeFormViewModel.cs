using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class AnimeFormViewModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Введите название аниме.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;


        [MaxLength(200)]
        public string? OriginalTitle { get; set; }


        [MaxLength(3000)]
        public string? Description { get; set; }


        [Range(1900, 2100)]
        public int ReleaseYear { get; set; } = DateTime.Now.Year;


        [Range(0, 10)]
        public decimal Rating { get; set; } = 8.0m;


        [MaxLength(20)]
        public string? AgeRating { get; set; } = "16+";


        [Range(0, 10000)]
        public int EpisodesCount { get; set; }


        [MaxLength(50)]
        public string? Status { get; set; } = "Выходит";


        [MaxLength(500)]
        public string? PosterUrl { get; set; }


        [MaxLength(500)]
        public string? BannerUrl { get; set; }



        // Жанры
        public List<int> SelectedGenreIds { get; set; }
            = new();


        public IEnumerable<SelectListItem> Genres { get; set; }
            = new List<SelectListItem>();


        // Дополнительные данные для формы

        public List<SelectListItem> Years { get; set; }
            = new();


        public List<string> AgeRatings { get; set; }
            = new()
            {
                "0+",
                "12+",
                "16+",
                "18+"
            };


        public List<string> Statuses { get; set; }
            = new()
            {
                "Анонс",
                "Выходит",
                "Завершён"
            };
    }
}