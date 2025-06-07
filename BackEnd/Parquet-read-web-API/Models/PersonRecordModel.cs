namespace ParquetReadWeb_API.Models
{
    using System.Net;

    public class PersonRecordModel
    {
        public DateTime? RegistrationDate { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public decimal Salary { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Comments { get; set; }
        public string? Title { get; set; }
        public string? IpAddress { get; set; }
    }
}
