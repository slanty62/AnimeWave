namespace AnimeWave.ViewModels
{
    public class AdminDashboardViewModel
    {
        // =========================================================
        // MAIN STATISTICS
        // =========================================================

        public int AnimeCount { get; set; }

        public int UsersCount { get; set; }

        public int FavoritesCount { get; set; }

        public int ViewsCount { get; set; }

        public int ReviewsCount { get; set; }


        // =========================================================
        // ADDITIONAL STATISTICS
        // =========================================================

        public int CompletedAnimeCount { get; set; }

        public int OngoingAnimeCount { get; set; }

        public decimal AverageRating { get; set; }


        // =========================================================
        // GENRES CHART
        // =========================================================

        public List<AdminGenreStatViewModel> GenreStats { get; set; }
            = new();


        // =========================================================
        // RECENT USERS
        // =========================================================

        public List<AdminRecentUserViewModel> RecentUsers { get; set; }
            = new();


        // =========================================================
        // RECENT ANIME
        // =========================================================

        public List<AdminRecentAnimeViewModel> RecentAnime { get; set; }
            = new();
    }


    public class AdminGenreStatViewModel
    {
        public string Name { get; set; }
            = string.Empty;

        public int Count { get; set; }

        public double Percentage { get; set; }
    }


    public class AdminRecentUserViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string DisplayName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string AvatarStyle { get; set; }
            = "violet";

        public string AvatarSymbol { get; set; }
            = "月";

        public DateTime CreatedAt { get; set; }
    }


    public class AdminRecentAnimeViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string? OriginalTitle { get; set; }

        public string? PosterUrl { get; set; }

        public decimal Rating { get; set; }

        public int ReleaseYear { get; set; }

        public string? Status { get; set; }
    }
}