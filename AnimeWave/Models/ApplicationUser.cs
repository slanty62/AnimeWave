using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AnimeWave.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public ICollection<ViewingHistory>
            ViewingHistories
        { get; set; }
            = new List<ViewingHistory>();
    }
}