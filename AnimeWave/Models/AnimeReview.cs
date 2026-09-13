using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class AnimeReview
    {
        public int Id { get; set; }


        // =========================================================
        // USER
        // =========================================================

        [Required]
        public string UserId { get; set; }
            = string.Empty;

        public ApplicationUser User { get; set; }
            = null!;


        // =========================================================
        // ANIME
        // =========================================================

        public int AnimeId { get; set; }

        public Anime Anime { get; set; }
            = null!;


        // =========================================================
        // REVIEW
        // =========================================================

        [Range(
            1,
            10,
            ErrorMessage = "Оценка должна быть от 1 до 10."
        )]
        public int Rating { get; set; }


        [Required(ErrorMessage = "Напишите текст отзыва.")]
        [MaxLength(
            2000,
            ErrorMessage = "Отзыв не должен превышать 2000 символов."
        )]
        public string Text { get; set; }
            = string.Empty;


        // =========================================================
        // DATES
        // =========================================================

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        public DateTime? UpdatedAt { get; set; }
    }
}