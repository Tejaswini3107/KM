using Microsoft.AspNetCore.Mvc;

namespace KacharaManagement.API.Controllers
{
    [Route("admin")]
    public class AdminViewController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login() => View("~/Views/Admin/Login.cshtml");
        [HttpGet("dashboard")]
        public IActionResult Dashboard() => View("~/Views/Admin/Dashboard.cshtml");

        [HttpGet("logs")]
        public IActionResult Logs() => View("~/Views/Admin/Logs.cshtml");
    }
}