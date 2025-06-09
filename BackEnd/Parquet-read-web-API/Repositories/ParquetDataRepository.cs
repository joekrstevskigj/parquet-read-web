namespace ParquetReadWeb_API.Repositories
{
    using Microsoft.Extensions.Logging;
    using ParquetReadWeb_API.DTOs.Requests;
    using ParquetReadWeb_API.DTOs.Responses;
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

        public async Task<FilterDefaultsResponse> GetAllCountriesAsync()
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection("DataSource=:memory:");
                await conn.OpenAsync().ConfigureAwait(false);

                var query = $@"
                    SELECT DISTINCT {PersonRecordColumns.Country}
                    FROM read_parquet('{_filePath.Replace("\\", "\\\\")}')
                    WHERE {PersonRecordColumns.Country} IS NOT NULL AND {PersonRecordColumns.Country} <> ''
                    ORDER BY {PersonRecordColumns.Country};
                ";

                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;

                var countries = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var country = reader[0]?.ToString();
                    if (!string.IsNullOrWhiteSpace(country))
                        countries.Add(country);
                }

                return new FilterDefaultsResponse
                {
                    Countries = countries
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while querying parquet data for countries.");
                return new FilterDefaultsResponse();
            }
        }

        public async Task<FilterDefaultsResponse> GetMaxSalaryAsync()
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection("DataSource=:memory:");
                await conn.OpenAsync().ConfigureAwait(false);

                var query = $@"
                    SELECT MAX({PersonRecordColumns.Salary})
                    FROM read_parquet('{_filePath.Replace("\\", "\\\\")}')
                    WHERE {PersonRecordColumns.Salary} IS NOT NULL;
                ";

                using var cmd = conn.CreateCommand();
                cmd.CommandText = query;

                var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                decimal maxSalary = 0;
                if (result != null && result != DBNull.Value)
                {
                    maxSalary = Convert.ToDecimal(result);
                }

                return new FilterDefaultsResponse
                {
                    MaxSalary = maxSalary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while querying parquet data for max salary.");
                return new FilterDefaultsResponse();
            }
        }

        public async Task<EmployeesResult> GetFilteredAsync(EmployeesFilterRequest filter)
        {
            var offset = (filter.Page - 1) * filter.PageSize;

            try
            {
                using var conn = _connectionFactory.CreateConnection("DataSource=:memory:");
                await conn.OpenAsync().ConfigureAwait(false);

                //create temp tables so we optimize the search 
                var initialWhere = BuildFilterClause(filter);
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

                var searchWhere = BuildSearchClause(filter.SearchTerm, filter);

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
                        IpAddress = reader[PersonRecordColumns.IpAddress]?.ToString(),
                        CreditCard = reader[PersonRecordColumns.CreditCard]?.ToString(),
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

        private string BuildFilterClause(EmployeesFilterRequest filter)
        {
            var filters = new List<string>();

            if (filter.RegistrationDateFrom.HasValue || filter.RegistrationDateTo.HasValue)
            {
                var from = filter.RegistrationDateFrom.HasValue
                    ? $"DATE '{filter.RegistrationDateFrom.Value:yyyy-MM-dd}'"
                    : "DATE '1900-01-01'";

                var to = filter.RegistrationDateTo.HasValue
                    ? $"DATE '{filter.RegistrationDateTo.Value:yyyy-MM-dd}'"
                    : "DATE '2100-12-31'";

                filters.Add($@"{PersonRecordColumns.RegistrationDate} IS NOT NULL
                   AND {PersonRecordColumns.RegistrationDate}::DATE BETWEEN {from} AND {to}");
            }

            if (!string.IsNullOrEmpty(filter.Gender))
                filters.Add($"LOWER({PersonRecordColumns.Gender}) = '{filter.Gender.ToLowerInvariant().Replace("'", "''")}'");

            if (!string.IsNullOrEmpty(filter.Country))
                filters.Add($"LOWER({PersonRecordColumns.Country}) = '{filter.Country.ToLowerInvariant().Replace("'", "''")}'");

            if (filter.Salary.HasValue)
                filters.Add($"{PersonRecordColumns.Salary} BETWEEN 0 AND {filter.Salary.Value}");

            if (filter.BirthDateFrom.HasValue || filter.BirthDateTo.HasValue)
            {
                var from = filter.BirthDateFrom.HasValue
                    ? $"DATE '{filter.BirthDateFrom.Value:yyyy-MM-dd}'"
                    : "DATE '1800-01-01'";

                var to = filter.BirthDateTo.HasValue
                    ? $"DATE '{filter.BirthDateTo.Value:yyyy-MM-dd}'"
                    : "DATE '2100-12-31'";

                filters.Add($@"{PersonRecordColumns.BirthDate} IS NOT NULL 
                   AND {PersonRecordColumns.BirthDate} <> '' 
                   AND STRPTIME({PersonRecordColumns.BirthDate}, '%m/%d/%Y') BETWEEN {from} AND {to}");
            }

            return string.Join(" AND ", filters);
        }

        private string BuildSearchClause(string? searchTerm, EmployeesFilterRequest filter)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return string.Empty;

            var term = searchTerm.Replace("'", "''").ToLower();
            var clauses = new List<string>();

            if (filter.searchFirstName)
                clauses.Add($"LOWER({PersonRecordColumns.FirstName}) LIKE '%{term}%'");
            if (filter.searchLastName)
                clauses.Add($"LOWER({PersonRecordColumns.LastName}) LIKE '%{term}%'");
            if (filter.searchEmail)
                clauses.Add($"LOWER({PersonRecordColumns.Email}) LIKE '%{term}%'");
            if (filter.searchComments)
                clauses.Add($"LOWER({PersonRecordColumns.Comments}) LIKE '%{term}%'");
            if (filter.searchTitle)
                clauses.Add($"LOWER({PersonRecordColumns.Title}) LIKE '%{term}%'");

            if (clauses.Count == 0)
                return string.Empty;

            return $"({string.Join(" OR ", clauses)})";
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