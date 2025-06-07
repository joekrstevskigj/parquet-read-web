using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ParquetReadWeb_API.DTOs.Requests;
using ParquetReadWeb_API.DTOs.Responses;
using ParquetReadWeb_API.Interfaces.Repositories;
using ParquetReadWeb_API.Models;
using ParquetReadWeb_API.Services;

namespace TestParquetWeb.ServiceTests
{
    public class EmployeeServiceTest
    {
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<IParquetDataRepository> _repoMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly string _filePath = "test.parquet";

        public EmployeeServiceTest()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _repoMock = new Mock<IParquetDataRepository>();
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["Parquet:FilePath"]).Returns(_filePath);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFilePathMissing()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Parquet:FilePath"]).Returns((string?)null);

            Assert.Throws<ArgumentNullException>(() =>
                new EmployeeService(_cacheMock.Object, _repoMock.Object, config.Object));
        }
        [Fact]
        public async Task GetEmployeesAsync_ReturnsCachedResult_WhenInitialLoadAndCacheHit()
        {
            var filter = new EmployeesFilterRequest();
            var expected = new EmployeesResultResponse { TotalRecordCount = 1 };
            var cacheKey = $"employees_initial_load_{File.GetLastWriteTimeUtc(_filePath):yyyyMMddHHmmss}";

            object? outValue = expected;
            _cacheMock
                .Setup(c => c.TryGetValue(cacheKey, out outValue!))
                .Returns(true);

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Equal(expected.TotalRecordCount, result.TotalRecordCount);
        }

        [Fact]
        public async Task GetEmployeesAsync_CachesResult_WhenInitialLoadAndCacheMiss()
        {
            var filter = new EmployeesFilterRequest();
            var cacheKey = $"employees_initial_load_{File.GetLastWriteTimeUtc(_filePath):yyyyMMddHHmmss}";
            object? dummy;
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out dummy)).Returns(false);

            var repoResult = new EmployeesResult
            {
                Employees = new List<PersonRecordModel>
                {
                    new PersonRecordModel { FirstName = "A", LastName = "B" }
                },
                TotalRecordCount = 1
            };
            _repoMock.Setup(r => r.GetFilteredAsync(It.IsAny<EmployeesFilterRequest>()))
                .ReturnsAsync(repoResult);

            var cacheEntryMock = new Mock<ICacheEntry>();
            object? setValue = null;
            cacheEntryMock.SetupProperty(e => e.Value);
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object)
                .Callback<object>(k => setValue = k);

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Single(result.Employees);
            Assert.Equal(1, result.TotalRecordCount);
            _cacheMock.Verify(c => c.CreateEntry(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetEmployeesAsync_CallsRepository_WhenNotInitialLoad()
        {
            var filter = new EmployeesFilterRequest { Page = 2 };
            var repoResult = new EmployeesResult
            {
                Employees = new List<PersonRecordModel>
                {
                    new PersonRecordModel { FirstName = "C", LastName = "D" }
                },
                TotalRecordCount = 1
            };
            _repoMock.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(repoResult);

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Single(result.Employees);
            Assert.Equal("C", result.Employees[0].FirstName);
            _repoMock.Verify(r => r.GetFilteredAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetEmployeesAsync_HandlesEmptyRepositoryResult()
        {
            var filter = new EmployeesFilterRequest { Page = 2 };
            _repoMock.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(new EmployeesResult());

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Empty(result.Employees);
            Assert.Equal(0, result.TotalRecordCount);
        }

        [Fact]
        public async Task GetEmployeesAsync_MapsDatesCorrectly()
        {
            var filter = new EmployeesFilterRequest { Page = 2 };
            var repoResult = new EmployeesResult
            {
                Employees = new List<PersonRecordModel>
                {
                    new PersonRecordModel
                    {
                        FirstName = "E",
                        LastName = "F",
                        RegistrationDate = new DateTime(2020, 1, 2),
                        BirthDate = new DateTime(1990, 5, 6)
                    }
                },
                TotalRecordCount = 1
            };
            _repoMock.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(repoResult);

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Equal("1/2/2020", result.Employees[0].RegistrationDate);
            Assert.Equal("5/6/1990", result.Employees[0].BirthDate);
        }

        [Fact]
        public async Task GetEmployeesAsync_HandlesNullDates()
        {
            var filter = new EmployeesFilterRequest { Page = 2 };
            var repoResult = new EmployeesResult
            {
                Employees = new List<PersonRecordModel>
                {
                    new PersonRecordModel
                    {
                        FirstName = "E",
                        LastName = "F",
                        RegistrationDate = null,
                        BirthDate = null
                    }
                },
                TotalRecordCount = 1
            };
            _repoMock.Setup(r => r.GetFilteredAsync(filter)).ReturnsAsync(repoResult);

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            var result = await service.GetEmployeesAsync(filter);

            Assert.Null(result.Employees[0].RegistrationDate);
            Assert.Null(result.Employees[0].BirthDate);
        }

        [Fact]
        public async Task GetEmployeesAsync_ThrowsException_WhenRepositoryThrows()
        {
            var filter = new EmployeesFilterRequest { Page = 2 };
            _repoMock.Setup(r => r.GetFilteredAsync(filter)).ThrowsAsync(new Exception("fail"));

            var service = new EmployeeService(_cacheMock.Object, _repoMock.Object, _configMock.Object);

            await Assert.ThrowsAsync<Exception>(() => service.GetEmployeesAsync(filter));
        }
    }
}