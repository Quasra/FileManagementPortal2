using Microsoft.AspNetCore.Mvc;

namespace FileManagementPortal1.UI.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}