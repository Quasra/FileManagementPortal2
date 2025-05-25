using Microsoft.AspNetCore.Mvc;

namespace FileManagementPortal1.UI.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}