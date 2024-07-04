using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityCoreFullCustomized.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        // GET: api/<EmployeeController>
        [HttpGet("employee")]
        public IEnumerable<string> Get()
        {
            return new List<string> { "Aleksei", "Justino", "Mateus" };
        }

    }
}
