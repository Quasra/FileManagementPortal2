using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace FileManagementPortal1.UI.Controllers
{
    public class AccountController : Controller
    {
       
        public IActionResult Login()
        {
            var ApiBaseURL = "https://localhost:7064/";
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        public IActionResult Register()
        {
            var ApiBaseURL = "https://localhost:7064/";
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        public IActionResult ForgotPassword()
        {
            var ApiBaseURL = "https://localhost:7064/";
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }
        public IActionResult Profile()
        {
          
            return View();
        }

    }
}