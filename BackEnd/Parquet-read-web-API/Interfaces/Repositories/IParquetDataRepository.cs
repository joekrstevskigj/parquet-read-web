namespace ParquetReadWeb_API.Interfaces.Repositories
{
    using ParquetReadWeb_API.DTOs.Requests;
    using ParquetReadWeb_API.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IParquetDataRepository
    {
        Task<EmployeesResult> GetFilteredAsync(EmployeesFilterRequest filter);
    }
}