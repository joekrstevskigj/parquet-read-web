namespace ParquetReadWeb_API.Models
{
    using ParquetReadWeb_API.DTOs;

    public class EmployeesResult
    {
        public List<PersonRecordModel> Employees { get; set; } = [];
        public int TotalRecordCount { get; set; }
    }
}
