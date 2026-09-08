using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class AnimeFormViewModel
    {
        public int Id { get; set; }


        [Required(
            ErrorMessage = "Введите название аниме."
        )]
        [MaxLength(200)]
        public string Title { get; set; }
            = string.Empty;


        [MaxLength(200)]
        public string? OriginalTitle { get; set; }


        [MaxLength(3000)]
        public string? Description { get; set; }


        [Range(
            1900,
            2100,
            ErrorMessage = "Укажите корректный год."
        )]
        public int ReleaseYear { get; set; }


        [Range(
            0,
            10,
            ErrorMessage = "Рейтинг должен быть от 0 до 10."
        )]
        public decimal Rating { get; set; }


        [MaxLength(20)]
        public string? AgeRating { get; set; }


        [Range(
            0,
            10000,
            ErrorMessage = "Некорректное количество серий."
        )]
        public int EpisodesCount { get; set; }


        [MaxLength(50)]
        public string? Status { get; set; }


        [MaxLength(500)]
        public string? PosterUrl { get; set; }


        [MaxLength(500)]
        public string? BannerUrl { get; set; }


        public List<int> SelectedGenreIds { get; set; }
            = new();


        public IEnumerable<SelectListItem> Genres { get; set; }
            = new List<SelectListItem>();
    }
}