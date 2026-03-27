using KacharaManagement.Core.Entities;
using KacharaManagement.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KacharaManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] KacharaManagement.API.Models.LoginRequest request)
        {
            var user = await _adminService.LoginAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });
            return Ok(new { message = "Login successful" });
        }
        [HttpGet("logs")]
        public async Task<IActionResult> Logs([FromQuery] int limit = 100)
        {
            var logs = await _adminService.GetLogsAsync(limit);
            var result = logs.Select(x => new {
                x.CreatedAt,
                x.Level,
                x.Message,
                x.Source
            });
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AdminUser user)
        {
            var created = await _adminService.CreateAdminUserAsync(user);
            if (!created)
                return BadRequest(new { message = "User already exists or invalid data." });
            return Ok(new { message = "Admin user created successfully." });
        }
    }
}