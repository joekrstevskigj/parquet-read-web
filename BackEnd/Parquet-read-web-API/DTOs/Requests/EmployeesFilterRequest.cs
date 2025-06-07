namespace ParquetReadWeb_API.DTOs.Requests
{
    public class EmployeesFilterRequest
    {
        public DateTime? RegistrationDate { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}
