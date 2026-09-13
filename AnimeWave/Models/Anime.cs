using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class Anime
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
            = string.Empty;


        [MaxLength(200)]
        public string? OriginalTitle { get; set; }


        [MaxLength(3000)]
        public string? Description { get; set; }


        public int ReleaseYear { get; set; }


        public decimal Rating { get; set; }


        [MaxLength(20)]
        public string? AgeRating { get; set; }


        public int EpisodesCount { get; set; }


        [MaxLength(50)]
        public string? Status { get; set; }


        [MaxLength(500)]
        public string? PosterUrl { get; set; }


        [MaxLength(500)]
        public string? BannerUrl { get; set; }



        // =========================================================
        // POPULARITY
        //
        // Сколько раз пользователи открывали Details этого Anime.
        // Это именно количество открытий, а не уникальных зрителей.
        // =========================================================

        public int OpenCount { get; set; }



        // =========================================================
        // RELATIONSHIPS
        // =========================================================

        public ICollection<AnimeGenre> AnimeGenres { get; set; }
            = new List<AnimeGenre>();


        public ICollection<Episode> Episodes { get; set; }
            = new List<Episode>();
    }
}