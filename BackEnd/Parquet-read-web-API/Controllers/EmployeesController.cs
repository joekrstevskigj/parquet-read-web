using Microsoft.AspNetCore.Mvc;
using ParquetReadWeb_API.DTOs.Requests;
using ParquetReadWeb_API.Interfaces.Services;

namespace ParquetReadWeb_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService service)
        {
            _employeeService = service;
        }

        [HttpGet(nameof(GetEmployeesData))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeesData([FromQuery] EmployeesFilterRequest filter)
        {
            var employeesResult = await _employeeService.GetEmployeesAsync(filter).ConfigureAwait(false);

            if(employeesResult.TotalRecordCount <= 0)
            {
                return NotFound(employeesResult);
            }

            return Ok(employeesResult);
        }
    }
}