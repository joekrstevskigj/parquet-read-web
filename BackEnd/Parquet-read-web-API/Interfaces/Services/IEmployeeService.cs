namespace ParquetReadWeb_API.Interfaces.Services
{
    using ParquetReadWeb_API.DTOs.Requests;
    using ParquetReadWeb_API.DTOs.Responses;
    using ParquetReadWeb_API.Models;

    public interface IEmployeeService
    {
        Task<EmployeesResultResponse> GetEmployeesAsync(EmployeesFilterRequest filter);
    }
}