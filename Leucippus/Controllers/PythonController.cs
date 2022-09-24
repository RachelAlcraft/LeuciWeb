using Microsoft.AspNetCore.Mvc;


// Video on adding a python controller https://www.youtube.com/watch?v=7HNDoD0SMOU

namespace Leucippus.Controllers
{
    public class PythonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
