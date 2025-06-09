namespace ParquetReadWeb_API.DTOs.Responses
{
    public class FilterDefaultsResponse
    {
        public List<string> Countries { get; set; } = [];

        public decimal MaxSalary { get; set; }
    }
}
