using Microsoft.AspNetCore.Mvc;

namespace FileManagementPortal1.UI.Controllers
{
    public class FoldersController : Controller
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

        public IActionResult Create(int id)
        {
            
            ViewBag.FolderId = id;
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.FolderId =  id;
            return View();
        }
    }
}