using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        public ICollection<AnimeGenre> AnimeGenres { get; set; }
            = new List<AnimeGenre>();
    }
}