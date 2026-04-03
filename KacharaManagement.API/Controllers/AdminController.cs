using KacharaManagement.Core.Entities;
using KacharaManagement.Business.Interfaces;
using KacharaManagement.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace KacharaManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IGothamService _gothamService;
        public AdminController(IAdminService adminService, IGothamService gothamService)
        {
            _adminService = adminService;
            _gothamService = gothamService;
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
        public async Task<IActionResult> Logs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? level = null, [FromQuery] string? source = null, [FromQuery] string? search = null)
        {
            var logs = await _adminService.GetLogsAsync(page, pageSize, level, source, search);
            return Ok(logs);
        }

        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? source = null, [FromQuery] bool? alert = null, [FromQuery] bool? needsTruck = null, [FromQuery] string? bin1State = null, [FromQuery] string? bin2State = null, [FromQuery] string? bin3State = null, [FromQuery] string? search = null)
        {
            var history = await _gothamService.GetPagedHistoryAsync(page, pageSize, source, alert, needsTruck, bin1State, bin2State, bin3State, search);
            return Ok(history);
        }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview([FromQuery] int historyLimit = 50)
        {
            var history = await _gothamService.GetHistoryAsync(historyLimit);
            var latestHistory = history.FirstOrDefault();
            var latestLogPage = await _adminService.GetLogsAsync(1, 1);
            var logs = await _adminService.GetLogsAsync(1, 200);

            var summary = new DashboardOverviewResponse
            {
                LatestHistory = latestHistory,
                LatestLog = latestLogPage.Items.FirstOrDefault(),
                TotalHistoryCount = history.Count,
                AlertCount = history.Count(x => x.Alert == 1),
                NeedsTruckCount = history.Count(x => x.NeedsTruck),
                LogCount = logs.TotalCount
            };

            return Ok(summary);
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