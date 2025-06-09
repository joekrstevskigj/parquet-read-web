namespace ParquetReadWeb_API.Interfaces.Services
{
    using ParquetReadWeb_API.DTOs.Responses;

    public interface IFilterDefaultsService
    {
        Task<FilterDefaultsResponse> GetAllCountriesAsync();
        Task<FilterDefaultsResponse> GetMaxSalaryAsync();
    }
}
