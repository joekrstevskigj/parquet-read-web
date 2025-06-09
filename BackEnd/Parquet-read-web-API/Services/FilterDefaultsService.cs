namespace ParquetReadWeb_API.Services
{
    using Microsoft.Extensions.Caching.Memory;
    using ParquetReadWeb_API.DTOs.Responses;
    using ParquetReadWeb_API.Interfaces.Repositories;
    using ParquetReadWeb_API.Interfaces.Services;
    using System.Threading.Tasks;

    public class FilterDefaultsService : IFilterDefaultsService
    {
        private readonly IMemoryCache _cache;
        private readonly IParquetDataRepository _repository;

        public FilterDefaultsService(IMemoryCache cache, IParquetDataRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public async Task<FilterDefaultsResponse> GetAllCountriesAsync()
        {
            var cacheKey = $"countires_for_filter";

            if (_cache.TryGetValue(cacheKey, out FilterDefaultsResponse? cachedResult))
            {
                return cachedResult ?? new FilterDefaultsResponse();
            }

            var result = await _repository.GetAllCountriesAsync().ConfigureAwait(false);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(160));
            return result;
        }

        public async Task<FilterDefaultsResponse> GetMaxSalaryAsync()
        {
            var cacheKey = $"max_salary_filter";

            if (_cache.TryGetValue(cacheKey, out FilterDefaultsResponse? cachedResult))
            {
                return cachedResult ?? new FilterDefaultsResponse();
            }

            var result = await _repository.GetMaxSalaryAsync().ConfigureAwait(false);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(160));
            return result;
        }
    }
}
