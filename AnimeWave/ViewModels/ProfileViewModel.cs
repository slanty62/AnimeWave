namespace AnimeWave.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Initial { get; set; }
            = "A";

        public string AvatarStyle { get; set; }
            = "violet";

        public string AvatarSymbol { get; set; }
            = "月";

        public DateTime CreatedAt { get; set; }

        public int FavoritesCount { get; set; }

        public int ViewedCount { get; set; }


        public List<ProfileRecentAnimeViewModel>
            RecentAnime
        { get; set; }
            = new();
    }


    public class ProfileRecentAnimeViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string? PosterUrl { get; set; }

        public decimal Rating { get; set; }

        public int ReleaseYear { get; set; }

        public string? AgeRating { get; set; }

        public DateTime ViewedAt { get; set; }
    }
}