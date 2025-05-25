using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManagementPortal1.UI.Controllers
{
    //[Authorize]
    //public class FilesController : Controller
    //{
    //    public IActionResult Index()
    //    {
    //        // Kullanıcı admin mi?
    //        ViewBag.IsAdmin = User.IsInRole("Admin");
    //        return View();
    //    }

    //    public IActionResult Upload()
    //    {
    //        return View();
    //    }

    //    public IActionResult Edit(string id)
    //    {
    //        ViewBag.Id = id;
    //        return View();
    //    }
    //}
    public class FilesController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View();
        }


        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult Edit(string id)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}

