namespace ParquetReadWeb_API.Data
{
    using DuckDB.NET.Data;
    using ParquetReadWeb_API.Interfaces.Data;

    public class DuckDbConnectionFactory : IDuckDbConnectionFactory
    {
        public DuckDBConnection CreateConnection(string connectionString)
        {
            return new DuckDBConnection(connectionString);
        }
    }
}
