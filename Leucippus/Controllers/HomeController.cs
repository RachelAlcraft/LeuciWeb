using ChartDirector;
using Leucippus.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Leucippus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        ElectronDensity ed = new ElectronDensity("1ejg");

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;            
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Explore(string PdbCode="1ejg",double XX=25, double YY=25, double ZZ=25)
        {
            ed.PdbCode = PdbCode;
            ed.XX = XX;
            ed.YY = YY;
            ed.ZZ = ZZ;
            ed.DownloadAsync();            
            ViewBag.Info = ed.Info;
            ViewBag.PdbCode = ed.PdbCode;
            ViewBag.XX = ed.XX;
            ViewBag.YY = ed.YY;
            ViewBag.ZZ = ed.ZZ;

            MatrixView mtx = new MatrixView();
            mtx.Create(ViewBag.Viewer = new RazorChartViewer(HttpContext, "chart1"),ed.MtxX,ed.MtxY,ed.MtxZ,PdbCode);

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Matrix()
        {
            ViewBag.Title = "Contour Chart";
            // The x and y coordinates of the grid
            double[] dataX = { -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            double[] dataY = { -10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // The values at the grid points. In this example, we will compute the values using the formula
            // z = x * sin(y) + y * sin(x).
            double[] dataZ = new double[dataX.Length * dataY.Length];
            for (int yIndex = 0; yIndex < dataY.Length; ++yIndex)
            {
                double y = dataY[yIndex];
                for (int xIndex = 0; xIndex < dataX.Length; ++xIndex)
                {
                    double x = dataX[xIndex];
                    dataZ[yIndex * dataX.Length + xIndex] = x * Math.Sin(y) + y * Math.Sin(x);
                }
            }
            MatrixView mtx = new MatrixView();
            mtx.Create(ViewBag.Viewer = new RazorChartViewer(HttpContext, "chart1"),dataX,dataY,dataZ,"Dummy");            
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