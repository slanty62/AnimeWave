using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class EpisodeFormViewModel
    {
        public int Id { get; set; }

        public int AnimeId { get; set; }


        [Required(ErrorMessage = "Укажите номер серии.")]
        [Range(1, 10000, ErrorMessage = "Номер серии должен быть больше 0.")]
        public int EpisodeNumber { get; set; } = 1;


        [Required(ErrorMessage = "Введите название серии.")]
        [MaxLength(200, ErrorMessage = "Название слишком длинное.")]
        public string Title { get; set; } = string.Empty;


        [MaxLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов.")]
        public string? Description { get; set; }


        [Range(1, 1000, ErrorMessage = "Продолжительность должна быть больше 0.")]
        public int DurationMinutes { get; set; } = 24;


        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }


        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }
    }
}