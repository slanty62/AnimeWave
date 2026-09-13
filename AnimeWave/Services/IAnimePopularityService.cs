using AnimeWave.ViewModels;

namespace AnimeWave.Services
{
    public interface IAnimePopularityService
    {
        Task<Dictionary<int, AnimePopularityViewModel>>
            GetAllAsync();
    }
}