using ChartDirector;
using Leucippus.Models;
using Microsoft.AspNetCore.Mvc;
using Plotly.NET;
using System.Diagnostics;

namespace Leucippus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //ElectronDensity ed = new ElectronDensity("6eex");

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;            
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Explore(string PdbCode="6eex",double XX=25, double YY=25, double ZZ=25)
        {            
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {         
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult LoadCcp4()
        {
            return View();
        }
    }
}