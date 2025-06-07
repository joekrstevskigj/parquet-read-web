namespace ParquetReadWeb_API.DTOs.Responses
{
    public class EmployeesResultResponse
    {
        public List<PersonalRecordDTO> Employees { get; set; } = [];
        public int TotalRecordCount { get; set; } = 0;
        public string? Message { get; set; } = "";
    }
}