using Microsoft.AspNetCore.Mvc;

namespace Leucippus.Controllers
{
    public class JavaScriptController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
