namespace AnimeWave.Models
{
    public class AnimeGenre
    {
        public int AnimeId { get; set; }

        public Anime Anime { get; set; } = null!;

        public int GenreId { get; set; }

        public Genre Genre { get; set; } = null!;
    }
}