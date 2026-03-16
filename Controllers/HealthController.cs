using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private static readonly DateTime _startTime = DateTime.UtcNow;

        // [HttpGet("detailed")]
        // public async Task<IActionResult> GetDetailed()
        // {
        //     var dbHealthy = true;
        //     // try { await _context.Database.CanConnectAsync(); } catch { dbHealthy = false; }
        //     return Ok(new { status = dbHealthy ? "ok" : "degraded", uptime = (DateTime.UtcNow - _startTime).ToString() });
        // }

        [HttpGet]
        public IActionResult Get()
        {
            var uptime = DateTime.UtcNow - _startTime;
            return Ok(new
            {
                status = "ok",
                product = "SnapQuote",
                service = "identity",
                uptime = uptime.ToString(@"dd\.hh\:mm\:ss")
            });
        }
    }
}
