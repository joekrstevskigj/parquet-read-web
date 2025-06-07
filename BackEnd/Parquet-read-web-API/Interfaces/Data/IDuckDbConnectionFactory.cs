namespace ParquetReadWeb_API.Interfaces.Data
{
    using DuckDB.NET.Data;

    public interface IDuckDbConnectionFactory
    {
        DuckDBConnection CreateConnection(string connectionString);
    }
}
