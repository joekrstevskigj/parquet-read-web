using Microsoft.Extensions.Caching.Memory;
using ParquetReadWeb_API.DTOs;
using ParquetReadWeb_API.DTOs.Requests;
using ParquetReadWeb_API.DTOs.Responses;
using ParquetReadWeb_API.Interfaces.Repositories;
using ParquetReadWeb_API.Interfaces.Services;

namespace ParquetReadWeb_API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IMemoryCache _cache;
        private readonly IParquetDataRepository _repository;
        private readonly string _filePath;

        public EmployeeService(IMemoryCache cache, IParquetDataRepository repository, IConfiguration configuration)
        {
            _cache = cache;
            _repository = repository;
            _filePath = configuration["Parquet:FilePath"] ?? throw new ArgumentNullException("Parquet:FilePath");
        }

        public async Task<EmployeesResultResponse> GetEmployeesAsync(EmployeesFilterRequest filter)
        {
            bool isInitialLoad =
                filter.RegistrationDateFrom == null &&
                filter.RegistrationDateTo == null &&
                string.IsNullOrEmpty(filter.Gender) &&
                string.IsNullOrEmpty(filter.Country) &&
                filter.Salary == null &&
                filter.BirthDateFrom == null &&
                filter.BirthDateTo == null &&
                string.IsNullOrEmpty(filter.SearchTerm) &&
                (filter.Page == 1 || filter.Page == 0) &&
                (filter.PageSize == 100 || filter.PageSize == 0);

            if (isInitialLoad)
            {
                var fileLastWrite = File.GetLastWriteTimeUtc(_filePath);
                var cacheKey = $"employees_initial_load_{fileLastWrite:yyyyMMddHHmmss}";

                if (_cache.TryGetValue(cacheKey, out EmployeesResultResponse? cachedResult))
                {
                    return cachedResult ?? new EmployeesResultResponse();//to suppres warning
                }

                var result = await GetEmployeesResultResponseAsync(filter).ConfigureAwait(false);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                return result;
            }
            else
            {
                return await GetEmployeesResultResponseAsync(filter).ConfigureAwait(false);
            }
        }

        private async Task<EmployeesResultResponse> GetEmployeesResultResponseAsync(EmployeesFilterRequest filter)
        {
            var result = await _repository.GetFilteredAsync(filter).ConfigureAwait(false);
            return new EmployeesResultResponse
            {
                Employees = result.Employees.Select(x => new PersonalRecordDTO
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    Comments = x.Comments,
                    Title = x.Title,
                    Gender = x.Gender,
                    Country = x.Country,
                    Salary = x.Salary,
                    RegistrationDate = x.RegistrationDate?.ToString("M/d/yyyy"),
                    BirthDate = x.BirthDate?.ToString("M/d/yyyy"),
                    CreditCard = x.CreditCard,
                    IpAddress = x.IpAddress,
                }).ToList(),
                TotalRecordCount = result.TotalRecordCount,
            };
        }
    }
}
