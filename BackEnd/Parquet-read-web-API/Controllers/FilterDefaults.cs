using Microsoft.AspNetCore.Mvc;
using ParquetReadWeb_API.DTOs.Requests;
using ParquetReadWeb_API.Interfaces.Services;

namespace ParquetReadWeb_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilterDefaults : ControllerBase
    {
        private readonly IFilterDefaultsService _filterDefaultsService;
        public FilterDefaults(IFilterDefaultsService filterDefaultsService)
        {
            _filterDefaultsService = filterDefaultsService;
        }

        [HttpGet(nameof(GetAllCountries))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllCountries()
        {
            var result = await _filterDefaultsService.GetAllCountriesAsync().ConfigureAwait(false);

            if (result.Countries.Count <= 0)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpGet(nameof(GetMaxSalary))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaxSalary()
        {
            var result = await _filterDefaultsService.GetMaxSalaryAsync().ConfigureAwait(false);

            return Ok(result);
        }
    }
}
