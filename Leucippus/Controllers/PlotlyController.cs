using ChartDirector;
using Leucippus.Models;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using System.Collections.Generic;

namespace Leucippus.Controllers
{
    public class PlotlyController : Controller
    {
        ElectronDensity ed = new ElectronDensity("1ejg");
        public IActionResult Index()
        {                        
            return View();            
        }

        public IActionResult Matrix(string PdbCode = "1ejg", double XX = 10, double YY = 25, double ZZ = 25)
        {
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml
            ed.PdbCode = "1ejg";
            ed.XX = XX;
            ed.YY = YY;
            ed.ZZ = ZZ;
            ViewBag.XX = XX;
            ViewBag.YY = YY;
            ViewBag.ZZ = ZZ;
            ed.DownloadAsync();
            ViewBag.Info = ed.Info;
            ViewBag.PdbCode = ed.PdbCode;            
            ViewBag.MtxX = ed.MtxX;
            ViewBag.MtxY = ed.MtxY;
            ViewBag.MtxZ = ed.MtxZ;

            return View();
        }
    }
}
