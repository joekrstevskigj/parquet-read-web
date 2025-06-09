namespace ParquetReadWeb_API.DTOs.Requests
{
    public class EmployeesFilterRequest
    {
        public DateTime? RegistrationDateFrom { get; set; }
        public DateTime? RegistrationDateTo { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public decimal? Salary { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool searchFirstName { get; set; } = true;
        public bool searchLastName { get; set; } = true;
        public bool searchEmail { get; set; } = true;
        public bool searchComments { get; set; } = true;
        public bool searchTitle { get; set; } = true;
    }
}
