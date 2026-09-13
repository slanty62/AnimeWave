using AnimeWave.Data;
using AnimeWave.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace AnimeWave.Services
{
    public class AnimePopularityService
        : IAnimePopularityService
    {
        private readonly ApplicationDbContext _context;


        public AnimePopularityService(
            ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<
            Dictionary<int, AnimePopularityViewModel>
        > GetAllAsync()
        {
            // =====================================================
            // TRENDING WINDOW
            // =====================================================

            DateTime recentSince =
                DateTime.UtcNow.AddDays(-7);



            // =====================================================
            // ANIME + OPEN COUNT
            // =====================================================

            var animeData =
                await _context.Animes

                    .AsNoTracking()

                    .Select(
                        a =>
                            new
                            {
                                a.Id,
                                a.OpenCount
                            }
                    )

                    .ToListAsync();



            // =====================================================
            // FAVORITES
            // =====================================================

            var favoriteData =
                await _context.Favorites

                    .AsNoTracking()

                    .GroupBy(
                        f => f.AnimeId
                    )

                    .Select(
                        group =>
                            new
                            {
                                AnimeId =
                                    group.Key,

                                Total =
                                    group.Count(),

                                Recent =
                                    group
                                        .Where(
                                            f =>
                                                f.CreatedAt >=
                                                recentSince
                                        )
                                        .Count()
                            }
                    )

                    .ToListAsync();



            var favorites =
                favoriteData.ToDictionary(
                    x => x.AnimeId
                );



            // =====================================================
            // UNIQUE VIEWERS
            //
            // ViewingHistory уже имеет UNIQUE(UserId, AnimeId),
            // поэтому Count() фактически равен количеству
            // уникальных зрителей Anime.
            // =====================================================

            var viewerData =
                await _context.ViewingHistories

                    .AsNoTracking()

                    .GroupBy(
                        v => v.AnimeId
                    )

                    .Select(
                        group =>
                            new
                            {
                                AnimeId =
                                    group.Key,

                                Total =
                                    group.Count(),

                                Recent =
                                    group
                                        .Where(
                                            v =>
                                                v.ViewedAt >=
                                                recentSince
                                        )
                                        .Count()
                            }
                    )

                    .ToListAsync();



            var viewers =
                viewerData.ToDictionary(
                    x => x.AnimeId
                );



            // =====================================================
            // BUILD STATISTICS
            // =====================================================

            var statistics =
                animeData

                    .Select(
                        anime =>
                        {
                            int favoritesCount =
                                favorites.TryGetValue(
                                    anime.Id,
                                    out var favorite
                                )
                                    ? favorite.Total
                                    : 0;


                            int recentFavorites =
                                favorite != null
                                    ? favorite.Recent
                                    : 0;


                            int viewersCount =
                                viewers.TryGetValue(
                                    anime.Id,
                                    out var viewer
                                )
                                    ? viewer.Total
                                    : 0;


                            int recentViewers =
                                viewer != null
                                    ? viewer.Recent
                                    : 0;



                            // =====================================
                            // GENERAL POPULARITY
                            // =====================================
                            //
                            // Open Details = 1 point
                            // Favorite     = 5 points
                            // Viewer       = 3 points
                            //
                            // =====================================

                            int popularityScore =
                                anime.OpenCount
                                +
                                (
                                    favoritesCount
                                    *
                                    5
                                )
                                +
                                (
                                    viewersCount
                                    *
                                    3
                                );



                            // =====================================
                            // TRENDING SCORE
                            //
                            // Берём активность только
                            // за последние 7 дней.
                            // =====================================

                            int recentActivityScore =
                                (
                                    recentFavorites
                                    *
                                    5
                                )
                                +
                                (
                                    recentViewers
                                    *
                                    3
                                );



                            return new AnimePopularityViewModel
                            {
                                AnimeId =
                                    anime.Id,

                                OpenCount =
                                    anime.OpenCount,

                                FavoritesCount =
                                    favoritesCount,

                                UniqueViewersCount =
                                    viewersCount,

                                RecentFavoritesCount =
                                    recentFavorites,

                                RecentViewersCount =
                                    recentViewers,

                                PopularityScore =
                                    popularityScore,

                                RecentActivityScore =
                                    recentActivityScore
                            };
                        }
                    )

                    .ToList();



            // =====================================================
            // ALL-TIME RANK
            // =====================================================

            var ranked =
                statistics

                    .OrderByDescending(
                        x => x.PopularityScore
                    )

                    .ThenByDescending(
                        x => x.OpenCount
                    )

                    .ThenBy(
                        x => x.AnimeId
                    )

                    .ToList();



            for (
                int index = 0;
                index < ranked.Count;
                index++
            )
            {
                var item =
                    ranked[index];


                item.Rank =
                    index + 1;


                item.IsTop10 =
                    item.Rank <= 10
                    &&
                    item.PopularityScore > 0;


                item.IsHot =
                    item.Rank <= 3
                    &&
                    item.PopularityScore > 0;
            }



            // =====================================================
            // RECENT TRENDING RANK
            // =====================================================

            var recentRanked =
                statistics

                    .Where(
                        x =>
                            x.RecentActivityScore > 0
                    )

                    .OrderByDescending(
                        x =>
                            x.RecentActivityScore
                    )

                    .ThenByDescending(
                        x =>
                            x.PopularityScore
                    )

                    .ToList();



            for (
                int index = 0;
                index < recentRanked.Count;
                index++
            )
            {
                var item =
                    recentRanked[index];


                item.RecentRank =
                    index + 1;


                item.IsTrending =
                    item.RecentRank <= 5;
            }



            return statistics.ToDictionary(
                x => x.AnimeId
            );
        }
    }
}