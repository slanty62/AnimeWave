using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class Anime
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? OriginalTitle { get; set; }

        [MaxLength(2000)]
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Episode> Episodes { get; set; }
            = new List<Episode>();

        public ICollection<AnimeGenre> AnimeGenres { get; set; }
            = new List<AnimeGenre>();
    }
}