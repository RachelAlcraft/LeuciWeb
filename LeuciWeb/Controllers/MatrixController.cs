using Microsoft.AspNetCore.Mvc;

namespace LeuciWeb.Controllers
{
    public class MatrixController : Controller
    {// 
     // GET: /Matrix/

        public IActionResult Index()
        {
            return View();
        }

        // 
        // GET: /Matrix/Welcome/ 

        public string Welcome()
        {
            return "This is the Welcome action method...";
        }
    }
}
