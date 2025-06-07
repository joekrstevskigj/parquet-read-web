namespace ParquetReadWeb_API.DTOs
{
    public class PersonalRecordDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Comments { get; set; }
        public string? Title { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public decimal Salary { get; set; }
        public string? RegistrationDate { get; set; }
        public string? BirthDate { get; set; }
    }
}
