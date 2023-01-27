using Microsoft.AspNetCore.Mvc;

namespace Leucippus.Controllers
{
    public class CompareController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
