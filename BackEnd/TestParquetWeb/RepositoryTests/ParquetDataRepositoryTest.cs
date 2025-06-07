using DuckDB.NET.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ParquetReadWeb_API.DTOs.Requests;
using ParquetReadWeb_API.Interfaces.Data;
using ParquetReadWeb_API.Models;
using ParquetReadWeb_API.Repositories;
using System.Data;
using System.Data.Common;

namespace TestParquetWeb.RepositoryTests
{
    public class ParquetDataRepositoryTest
    {
        [Fact]
        public void Constructor_Throws_WhenFilePathMissing()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns((string?)null); 
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var duckFactoryMock = new Mock<IDuckDbConnectionFactory>();

            Assert.Throws<ArgumentNullException>(() =>
                new ParquetDataRepository(configMock.Object, loggerMock.Object, duckFactoryMock.Object));
        }

        [Fact]
        public void Constructor_LogsError_WhenFilePathMissing()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns((string?)null);
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var duckFactoryMock = new Mock<IDuckDbConnectionFactory>();

            try
            {
                new ParquetDataRepository(configMock.Object, loggerMock.Object, duckFactoryMock.Object);
            }
            catch (ArgumentNullException)
            {
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<ArgumentNullException>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        [Fact]
        public async Task GetFilteredAsync_ReturnsEmptyResult_OnException()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns("invalid_path");
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var duckFactoryMock = new Mock<IDuckDbConnectionFactory>();
            var repo = new ParquetDataRepository(configMock.Object, loggerMock.Object, duckFactoryMock.Object);

            var filter = new EmployeesFilterRequest { Page = 1, PageSize = 10 };

            var result = await repo.GetFilteredAsync(filter);

            Assert.NotNull(result);
            Assert.Empty(result.Employees ?? []);
            Assert.Equal(0, result.TotalRecordCount);
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetFilteredAsync_ReturnsPagedResults_WhenDataExists()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns("test.parquet");
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var connectionFactoryMock = new Mock<IDuckDbConnectionFactory>();
            var connectionMock = new Mock<DuckDBConnection>();
            var commandMock = new Mock<DuckDBCommand>();
            var readerMock = new Mock<System.Data.Common.DbDataReader>();

            var repoMock = new Mock<ParquetDataRepository>(configMock.Object, loggerMock.Object, connectionFactoryMock.Object) { CallBase = true };
            
            repoMock.Protected()
                .Setup<Task<DbDataReader>>("ExecuteReaderAsync", ItExpr.IsAny<DbCommand>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(readerMock.Object);

            connectionFactoryMock.Setup(f => f.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);
            connectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);

            commandMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            commandMock.SetupSequence(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(3); // total count

            var callCount = 0;
            readerMock.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => callCount++ < 2); // 2 records

            readerMock.Setup(r => r[It.IsAny<string>()]).Returns((string key) =>
            {
                return key switch
                {
                    "first_name" => "John",
                    "last_name" => "Doe",
                    "email" => "john.doe@example.com",
                    "comments" => "Test",
                    "title" => "Engineer",
                    "gender" => "M",
                    "country" => "US",
                    "salary" => 1000m,
                    "registration_dttm" => "1/1/2020",
                    "birthdate" => "1/1/1990",
                    _ => String.Empty
                };
            });

            var filter = new EmployeesFilterRequest { Page = 1, PageSize = 2 };

            var result = await repoMock.Object.GetFilteredAsync(filter);

            Assert.NotNull(result);
            Assert.Equal(2, result.Employees.Count);
            Assert.Equal(3, result.TotalRecordCount);
        }

        [Fact]
        public async Task GetFilteredAsync_AppliesFiltersCorrectly()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns("test.parquet");
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var connectionFactoryMock = new Mock<IDuckDbConnectionFactory>();
            var connectionMock = new Mock<DuckDBConnection>();
            var commandMock = new Mock<DuckDBCommand>();
            var readerMock = new Mock<System.Data.Common.DbDataReader>();

            var repoMock = new Mock<ParquetDataRepository>(configMock.Object, loggerMock.Object, connectionFactoryMock.Object) { CallBase = true };
            repoMock.Protected()
                .Setup<Task<DbDataReader>>("ExecuteReaderAsync", ItExpr.IsAny<DbCommand>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(readerMock.Object);

            connectionFactoryMock.Setup(f => f.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);
            connectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);

            commandMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            commandMock.SetupSequence(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var callCount = 0;
            readerMock.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => callCount++ < 1);

            readerMock.Setup(r => r[It.IsAny<string>()]).Returns((string key) =>
            {
                return key switch
                {
                    "first_name" => "Jane",
                    "last_name" => "Smith",
                    "email" => "jane.smith@example.com",
                    "comments" => "Filtered",
                    "title" => "Manager",
                    "gender" => "F",
                    "country" => "UK",
                    "salary" => 2000m,
                    "registration_dttm" => "2/2/2021",
                    "birthdate" => "2/2/1985",
                    _ => String.Empty
                };
            });

            var filter = new EmployeesFilterRequest
            {
                Page = 1,
                PageSize = 1,
                Gender = "F",
                Country = "UK"
            };

            var result = await repoMock.Object.GetFilteredAsync(filter);

            Assert.Single(result.Employees);
            Assert.Equal("Jane", result.Employees[0].FirstName);
            Assert.Equal("UK", result.Employees[0].Country);
        }

        [Fact]
        public async Task GetFilteredAsync_AppliesSearchTermCorrectly()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns("test.parquet");
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var connectionFactoryMock = new Mock<IDuckDbConnectionFactory>();
            var connectionMock = new Mock<DuckDBConnection>();
            var commandMock = new Mock<DuckDBCommand>();
            var readerMock = new Mock<System.Data.Common.DbDataReader>();

            var repoMock = new Mock<ParquetDataRepository>(configMock.Object, loggerMock.Object, connectionFactoryMock.Object) { CallBase = true };
            
            repoMock.Protected()
                .Setup<Task<DbDataReader>>("ExecuteReaderAsync", ItExpr.IsAny<DbCommand>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(readerMock.Object);

            connectionFactoryMock.Setup(f => f.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);
            connectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);

            commandMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            commandMock.SetupSequence(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var callCount = 0;
            readerMock.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => callCount++ < 1);

            readerMock.Setup(r => r[It.IsAny<string>()]).Returns((string key) =>
            {
                return key switch
                {
                    "first_name" => "Search",
                    "last_name" => "Match",
                    "email" => "search.match@example.com",
                    "comments" => "SearchTerm",
                    "title" => "Developer",
                    "gender" => "M",
                    "country" => "US",
                    "salary" => 1500m,
                    "registration_dttm" => "3/3/2022",
                    "birthdate" => "3/3/1992",
                    _ => String.Empty
                };
            });

            var filter = new EmployeesFilterRequest
            {
                Page = 1,
                PageSize = 1,
                SearchTerm = "search"
            };

            var result = await repoMock.Object.GetFilteredAsync(filter);

            Assert.Single(result.Employees);
            Assert.Contains("search", result.Employees[0].FirstName, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetFilteredAsync_ReturnsEmptyList_WhenNoDataMatches()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Parquet:FilePath"]).Returns("test.parquet");
            var loggerMock = new Mock<ILogger<ParquetDataRepository>>();
            var connectionFactoryMock = new Mock<IDuckDbConnectionFactory>();
            var connectionMock = new Mock<DuckDBConnection>();
            var commandMock = new Mock<DuckDBCommand>();
            var readerMock = new Mock<System.Data.Common.DbDataReader>();

            var repoMock = new Mock<ParquetDataRepository>(configMock.Object, loggerMock.Object, connectionFactoryMock.Object) { CallBase = true };
            repoMock.Protected()
                .Setup<Task<DbDataReader>>("ExecuteReaderAsync", ItExpr.IsAny<DbCommand>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(readerMock.Object);

            connectionFactoryMock.Setup(f => f.CreateConnection(It.IsAny<string>())).Returns(connectionMock.Object);
            connectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);

            commandMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            commandMock.SetupSequence(c => c.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            readerMock.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var filter = new EmployeesFilterRequest { Page = 1, PageSize = 10, SearchTerm = "no-match" };

            var result = await repoMock.Object.GetFilteredAsync(filter);

            Assert.Empty(result.Employees);
            Assert.Equal(0, result.TotalRecordCount);
        }
    }
}