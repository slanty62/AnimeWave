namespace AnimeWave.Models
{
    public class ViewingHistory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public int AnimeId { get; set; }

        public Anime Anime { get; set; } = null!;

        public DateTime ViewedAt { get; set; }
            = DateTime.UtcNow;
    }
}