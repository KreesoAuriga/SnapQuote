using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Authorization;
using WebApi.Filters;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [LogRequestBody]
    public class RequestLogsController : ControllerBase
    {
        private readonly DataContext _context;

        public RequestLogsController(DataContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Search([FromQuery] string search = null, [FromQuery] string query = null)
        {
            var filter = search ?? query;

            if (string.IsNullOrEmpty(filter) || filter == "*")
            {
                var allLogs = _context.RequestLogs.OrderByDescending(l => l.Timestamp).ToList();
                return Ok(allLogs);
            }

            var logs = _context.RequestLogs
                .Where(l => (l.EndpointPath != null && l.EndpointPath.Contains(filter)) ||
                           (l.RequestBody != null && l.RequestBody.Contains(filter)))
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            return Ok(logs);
        }

        // [HttpGet("paged")]
        // public IActionResult GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        // {
        //     var skip = (page - 1) * pageSize;
        //     var logs = _context.RequestLogs.OrderByDescending(l => l.Timestamp).Skip(skip).Take(pageSize).ToList();
        //     return Ok(logs);
        // }
    }
}
