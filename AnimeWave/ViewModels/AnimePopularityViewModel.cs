namespace AnimeWave.ViewModels
{
    public class AnimePopularityViewModel
    {
        public int AnimeId { get; set; }


        // =========================================================
        // RAW STATISTICS
        // =========================================================

        public int OpenCount { get; set; }

        public int FavoritesCount { get; set; }

        public int UniqueViewersCount { get; set; }



        // =========================================================
        // RECENT ACTIVITY
        // =========================================================

        public int RecentFavoritesCount { get; set; }

        public int RecentViewersCount { get; set; }



        // =========================================================
        // SCORES
        // =========================================================

        public int PopularityScore { get; set; }

        public int RecentActivityScore { get; set; }



        // =========================================================
        // RANKING
        // =========================================================

        public int Rank { get; set; }

        public int? RecentRank { get; set; }



        // =========================================================
        // BADGES
        // =========================================================

        public bool IsHot { get; set; }

        public bool IsTrending { get; set; }

        public bool IsTop10 { get; set; }
    }
}