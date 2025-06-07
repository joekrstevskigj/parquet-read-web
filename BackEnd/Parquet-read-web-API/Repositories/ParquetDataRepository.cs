namespace ParquetReadWeb_API.Repositories
{
    using DuckDB.NET.Data;
    using Microsoft.Extensions.Logging;
    using ParquetReadWeb_API.DTOs.Requests;
    using ParquetReadWeb_API.Helpers;
    using ParquetReadWeb_API.Interfaces.Data;
    using ParquetReadWeb_API.Interfaces.Repositories;
    using ParquetReadWeb_API.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.Threading.Tasks;

    public class ParquetDataRepository : IParquetDataRepository
    {
        private readonly string _filePath;
        private readonly ILogger<ParquetDataRepository> _logger;
        private readonly IDuckDbConnectionFactory _connectionFactory;

        public ParquetDataRepository(
            IConfiguration configuration,
            ILogger<ParquetDataRepository> logger,
            IDuckDbConnectionFactory connectionFactory)
        {
            _logger = logger; 
            _connectionFactory = connectionFactory;

            var filePath = configuration["Parquet:FilePath"];
            if (string.IsNullOrEmpty(filePath))
            {
                var ex = new ArgumentNullException("Parquet:FilePath");
                _logger.LogError(ex, "Parquet:FilePath configuration is missing.");
                throw ex;
            }
            _filePath = filePath;
        }

        public async Task<EmployeesResult> GetFilteredAsync(EmployeesFilterRequest filter)
        {
            var offset = (filter.Page - 1) * filter.PageSize;

            try
            {
                using var conn = _connectionFactory.CreateConnection("DataSource=:memory:");
                await conn.OpenAsync().ConfigureAwait(false);

                //create temp tables so we optimize the search 
                var initialWhere = BuildWhereClause(filter, excludeSearch: true);
                var createTempTable = $@"
                    CREATE TEMP TABLE filtered AS
                    SELECT *
                    FROM read_parquet('{_filePath.Replace("\\", "\\\\")}')
                    {(!string.IsNullOrEmpty(initialWhere) ? "WHERE " + initialWhere : "")};
                ";

                using (var createCmd = conn.CreateCommand())
                {
                    createCmd.CommandText = createTempTable;
                    await createCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                var searchWhere = BuildSearchClause(filter.SearchTerm);

                //count total results, because we are limiting the end result with pageSize
                var countQuery = $@"
                    SELECT COUNT(*) FROM filtered
                    {(!string.IsNullOrEmpty(searchWhere) ? "WHERE " + searchWhere : "")};
                ";

                int totalCount = 0;
                using (var countCmd = conn.CreateCommand())
                {
                    countCmd.CommandText = countQuery;
                    var result = await countCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    totalCount = Convert.ToInt32(result);
                }

                var finalQuery = $@"
                    SELECT *
                    FROM filtered
                    {(!string.IsNullOrEmpty(searchWhere) ? "WHERE " + searchWhere : "")}
                    LIMIT {filter.PageSize} OFFSET {offset};
                ";

                using var finalCmd = conn.CreateCommand();
                finalCmd.CommandText = finalQuery;

                var records = new List<PersonRecordModel>();
                //using var reader = await finalCmd.ExecuteReaderAsync().ConfigureAwait(false);
                using var reader = await ExecuteReaderAsync(finalCmd, CancellationToken.None);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var record = new PersonRecordModel
                    {
                        FirstName = reader[PersonRecordColumns.FirstName]?.ToString(),
                        LastName = reader[PersonRecordColumns.LastName]?.ToString(),
                        Email = reader[PersonRecordColumns.Email]?.ToString(),
                        Comments = reader[PersonRecordColumns.Comments]?.ToString(),
                        Title = reader[PersonRecordColumns.Title]?.ToString(),
                        Gender = reader[PersonRecordColumns.Gender]?.ToString(),
                        Country = reader[PersonRecordColumns.Country]?.ToString(),
                        Salary = reader[PersonRecordColumns.Salary] != DBNull.Value ? Convert.ToDecimal(reader[PersonRecordColumns.Salary]) : 0,
                        RegistrationDate = ParseDate(reader[PersonRecordColumns.RegistrationDate]),
                        BirthDate = ParseDate(reader[PersonRecordColumns.BirthDate]),
                    };

                    records.Add(record);
                }

                return new EmployeesResult
                {
                    Employees = records,
                    TotalRecordCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while querying parquet data.");
                return new EmployeesResult();
            }
        }

        private string BuildWhereClause(EmployeesFilterRequest filter, bool excludeSearch = false)
        {
            var filters = new List<string>();

            if (filter.RegistrationDate.HasValue)
                filters.Add($"{PersonRecordColumns.RegistrationDate} = DATE '{filter.RegistrationDate:yyyy-MM-dd}'");

            if (!string.IsNullOrEmpty(filter.Gender))
                filters.Add($"LOWER({PersonRecordColumns.Gender}) = '{filter.Gender.ToLowerInvariant().Replace("'", "''")}'");

            if (!string.IsNullOrEmpty(filter.Country))
                filters.Add($"LOWER({PersonRecordColumns.Country}) = '{filter.Country.ToLowerInvariant().Replace("'", "''")}'");

            if (filter.Salary.HasValue)
                filters.Add($"{PersonRecordColumns.Salary} = {filter.Salary.Value}");

            if (filter.BirthDate.HasValue)
                filters.Add($"{PersonRecordColumns.BirthDate} = DATE '{filter.BirthDate:yyyy-MM-dd}'");

            if (!excludeSearch && !string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Replace("'", "''").ToLower();
                filters.Add($@"(
                    LOWER({PersonRecordColumns.FirstName}) LIKE '%{term}%' OR
                    LOWER({PersonRecordColumns.LastName}) LIKE '%{term}%' OR
                    LOWER({PersonRecordColumns.Email}) LIKE '%{term}%' OR
                    LOWER({PersonRecordColumns.Comments}) LIKE '%{term}%' OR
                    LOWER({PersonRecordColumns.Title}) LIKE '%{term}%'
                )");
            }

            return string.Join(" AND ", filters);
        }

        private string BuildSearchClause(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return string.Empty;

            var term = searchTerm.Replace("'", "''").ToLower();
            return $@"(
                LOWER({PersonRecordColumns.FirstName}) LIKE '%{term}%' OR
                LOWER({PersonRecordColumns.LastName}) LIKE '%{term}%' OR
                LOWER({PersonRecordColumns.Email}) LIKE '%{term}%' OR
                LOWER({PersonRecordColumns.Comments}) LIKE '%{term}%' OR
                LOWER({PersonRecordColumns.Title}) LIKE '%{term}%'
            )";
        }

        private static DateTime? ParseDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var str = value.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return null;

            if (DateTime.TryParseExact(str, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            if (DateTime.TryParse(str, out dt))
                return dt;

            return null;
        }

        // For testing and mocking
        protected virtual async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken)
        {
            return await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}