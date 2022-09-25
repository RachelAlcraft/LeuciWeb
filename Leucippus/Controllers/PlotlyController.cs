using ChartDirector;
using Leucippus.Models;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using System.Collections.Generic;
using System.Numerics;

namespace Leucippus.Controllers
{
    public class PlotlyController : Controller
    {
        ElectronDensity ed = new ElectronDensity("");
        public IActionResult Index()
        {                        
            return View();            
        }

        public IActionResult Matrix(string pdbcode = "1ejg", int layer = 0, string plane="XY")
        {
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
            bool newCalcs = true;
            if (pdbcode != ed.PdbCode)
            {
                ed = new ElectronDensity(pdbcode);
                ed.Plane = plane;
                ed.Layer = layer;
                ed.DownloadAsync();
                newCalcs = true;                
            }
            else
            {
                if (ed.Layer != layer)
                {
                    newCalcs = true;
                    ed.Layer = layer;
                }
                if (ed.Plane != plane)
                {
                    newCalcs = true;
                    ed.Plane = plane;
                }                                
            }            
            if (newCalcs)
            {                
                ed.calculateWholeLayer(plane, layer);
                ViewBag.Info = ed.Info;
                ViewBag.MtxX = ed.MtxA;
                ViewBag.MtxY = ed.MtxB;
                ViewBag.MtxZ = ed.MtxC;
                ViewBag.MtxV = ed.MtxD;
            }
            ViewBag.PdbCode = ed.PdbCode;
            ViewBag.Plane = ed.Plane;
            ViewBag.Layer = ed.Layer;
            ViewBag.LayerMax = ed.LayerMax;

            return View();
        }
    }
}
