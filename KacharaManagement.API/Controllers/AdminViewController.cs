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

        [HttpGet("logout")]
        public IActionResult Logout() => Redirect("/admin/login");

        [HttpGet("logs")]
        public IActionResult Logs() => View("~/Views/Admin/Logs.cshtml");

        [HttpGet("history")]
        public IActionResult History() => View("~/Views/Admin/History.cshtml");
    }
}