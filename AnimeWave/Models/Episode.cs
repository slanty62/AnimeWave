using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class Episode
    {
        public int Id { get; set; }

        public int AnimeId { get; set; }

        public Anime Anime { get; set; } = null!;

        public int EpisodeNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        public DateTime? ReleaseDate { get; set; }
    }
}