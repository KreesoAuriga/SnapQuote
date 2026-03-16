using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Filters;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [LogRequestBody]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);
            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _userService.Register(model);
            return Ok(new { message = "Registration successful. You can now sign in to SnapQuote." });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var currentUser = (User)HttpContext.Items["User"];
            var users = _userService.GetAll(currentUser);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRequest model)
        {
            _userService.Update(id, model);
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPut("{id}/change-password")]
        public IActionResult ChangePassword(int id, ChangePasswordRequest model)
        {
            _userService.ChangePassword(id, model);
            return Ok(new { message = "Password changed successfully" });
        }

        // [AllowAnonymous]
        // [HttpGet("active")]
        // public IActionResult GetActiveUsers()
        // {
        //     var users = _userService.GetAll(null).Where(x => x.Role != null);
        //     return Ok(users);
        // }

        // [HttpGet("export")]
        // public IActionResult ExportUsers([FromQuery] string format)
        // {
        //     var currentUser = (User)HttpContext.Items["User"];
        //     var users = _userService.GetAll(currentUser);
        //     if (format == "csv")
        //     {
        //         // var csv = string.Join("\n", users.Select(u => $"{u.Id},{u.Username},{u.FirstName}"));
        //         // return File(Encoding.UTF8.GetBytes(csv), "text/csv", "users.csv");
        //     }
        //     return BadRequest(new { message = "Unsupported format" });
        // }

        private string GetFullDisplayName(User user)
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }
    }
}
