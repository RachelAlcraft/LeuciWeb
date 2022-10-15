using ChartDirector;
using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using ScottPlot.Statistics.Interpolation;
using System.Collections.Generic;
using System.Numerics;
using static Plotly.NET.StyleParam;


namespace Leucippus.Controllers
{
    public class MatrixController : Controller
    {        
        public async Task <IActionResult> Index(string pdbcode = "")
        {
            ViewBagMatrix.Instance.PdbCode = pdbcode;
            
            FileDownloads fd = new FileDownloads(ViewBagMatrix.Instance.PdbCode);
            bool ok = await fd.downloadAll();
            ViewBagMatrix.Instance.EmCode = fd.EmCode;
            ViewBagMatrix.Instance.DensityType = fd.DensityType;

            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.EmCode,fd.EmFilePath);
            ViewBagMatrix.Instance.Info = dm.Info;
            
            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
            ViewBag.EmCode = ViewBagMatrix.Instance.EmCode;
            ViewBag.Info = ViewBagMatrix.Instance.Info;
            ViewBag.EbiLink = ViewBagMatrix.Instance.EbiLink;
            ViewBag.DensityType = ViewBagMatrix.Instance.DensityType;

            return View();            
        }
        
        public async Task<IActionResult> Plane(int layer = -1, string plane="",string planeplot="")
        {            
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
            bool newCalcs = true;

            ViewBagMatrix.Instance.Plane = plane;
            ViewBagMatrix.Instance.Layer = layer;
            ViewBagMatrix.Instance.PlanePlot = planeplot;
            
            FileDownloads fd = new FileDownloads(ViewBagMatrix.Instance.PdbCode);
            bool ok = await fd.downloadAll();
            ViewBagMatrix.Instance.EmCode = fd.EmCode;
            ViewBagMatrix.Instance.DensityType = fd.DensityType;

            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.EmCode,fd.EmFilePath);
            dm.calculatePlane(ViewBagMatrix.Instance.Plane, ViewBagMatrix.Instance.Layer);
            
            if (ViewBagMatrix.Instance.EmCode == "" && layer == -1 && plane == "")            
                newCalcs = false;
                                                                                                                
            ViewBag.MtxX = dm.MatA;
            ViewBag.MtxY = dm.MatB;
            ViewBag.MtxZ = dm.MatC;
            ViewBag.MtxV = dm.MatD;
            
            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;            
            ViewBag.Plane = ViewBagMatrix.Instance.Plane;
            ViewBag.Layer = ViewBagMatrix.Instance.Layer;
            ViewBag.LayerMax = ViewBagMatrix.Instance.Layer - 1;
            ViewBag.MinV = dm.MinV;
            ViewBag.MaxV = dm.MaxV;
            ViewBag.PlanePlot = ViewBagMatrix.Instance.PlanePlot;

            return View();

        }
        public async Task<IActionResult> Slice(string pdbcode = "", 
            double cx = -1, double cy = -1, double cz = -1,
            double lx = -1, double ly = -1, double lz = -1, 
            double px = -1, double py = -1, double pz = -1,
            string ca = "", string la = "", string pa = "",
            string denplot ="", string radplot="", string lapplot = "",
            double width = -1, double gap = -1)
        {
            ViewBagMatrix.Instance.DenPlot = denplot;
            ViewBagMatrix.Instance.RadPlot = radplot;
            ViewBagMatrix.Instance.LapPlot = lapplot;
            ViewBagMatrix.Instance.Width = width;
            ViewBagMatrix.Instance.Gap = gap;
            ViewBagMatrix.Instance.SetCentral(cx, cy, cz, ca);
            ViewBagMatrix.Instance.SetLinear(lx, ly, lz, la);
            ViewBagMatrix.Instance.SetPlanar(px, py, pz, pa);

            bool recalc = ViewBagMatrix.Instance.Refresh;

            FileDownloads fd = new FileDownloads(ViewBagMatrix.Instance.PdbCode);
            bool ok = await fd.downloadAll();
            ViewBagMatrix.Instance.EmCode = fd.EmCode;
            ViewBagMatrix.Instance.DensityType = fd.DensityType;

            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.EmCode, fd.EmFilePath);
            
            if (recalc)                        
                dm.create_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar);
                                    
            ViewBag.SliceDensity = dm.SliceDensity;
            ViewBag.SliceRadiant = dm.SliceRadiant;
            ViewBag.SliceLaplacian = dm.SliceLaplacian;
            ViewBag.SliceAxis = dm.SliceAxis;
            
            ViewBag.cx = ViewBagMatrix.Instance.Central.A;
            ViewBag.cy = ViewBagMatrix.Instance.Central.B;
            ViewBag.cz = ViewBagMatrix.Instance.Central.C;
            ViewBag.lx = ViewBagMatrix.Instance.Linear.A;
            ViewBag.ly = ViewBagMatrix.Instance.Linear.B;
            ViewBag.lz = ViewBagMatrix.Instance.Linear.C;
            ViewBag.px = ViewBagMatrix.Instance.Planar.A;
            ViewBag.py = ViewBagMatrix.Instance.Planar.B;
            ViewBag.pz = ViewBagMatrix.Instance.Planar.C;

            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;

            ViewBag.DenPlot = ViewBagMatrix.Instance.DenPlot;
            ViewBag.RadPlot = ViewBagMatrix.Instance.RadPlot;
            ViewBag.LapPlot = ViewBagMatrix.Instance.LapPlot;
            ViewBag.Width = ViewBagMatrix.Instance.Width;
            ViewBag.Gap = ViewBagMatrix.Instance.Gap;

            ViewBagMatrix.Instance.Reset();
            return View();
        }

    }
}
