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
            
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode);
            if (DensitySingleton.Instance.NewMatrix)
            {
                string[] first_three = DensitySingleton.Instance.PA.getFirstThreeCoords();
                ViewBagMatrix.Instance.CentralAtom = first_three[0];
                ViewBagMatrix.Instance.LinearAtom = first_three[1];
                ViewBagMatrix.Instance.PlanarAtom = first_three[2];
            }

            ViewBagMatrix.Instance.Info = dm.Info;
            ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
            ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;
            
            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
            ViewBag.EmCode = ViewBagMatrix.Instance.EmCode;
            ViewBag.Info = ViewBagMatrix.Instance.Info;
            ViewBag.EbiLink = ViewBagMatrix.Instance.EbiLink;
            ViewBag.DensityType = ViewBagMatrix.Instance.DensityType;
            ViewBag.Resolution = DensitySingleton.Instance.FD.Resolution;

            return View();            
        }
        
        public async Task<IActionResult> Plane(int layer = -1, string plane="",string planeplot="")
        {            
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
            bool newCalcs = true;

            ViewBagMatrix.Instance.Plane = plane;
            ViewBagMatrix.Instance.Layer = layer;
            ViewBagMatrix.Instance.PlanePlot = planeplot;
                        
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode);
            dm.calculatePlane(ViewBagMatrix.Instance.Plane, ViewBagMatrix.Instance.Layer);
            
            if (ViewBagMatrix.Instance.EmCode == "" && layer == -1 && plane == "")            
                newCalcs = false;

            ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
            ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;

            ViewBag.MtxX = dm.MatA;
            ViewBag.MtxY = dm.MatB;
            ViewBag.MtxZ = dm.MatC;
            ViewBag.MtxV = dm.MatD;
            
            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;            
            ViewBag.Plane = ViewBagMatrix.Instance.Plane;
            ViewBag.Layer = dm.Layer;
            ViewBag.LayerMax = dm.LayerMax-1;
            ViewBag.MinV = dm.MinV;
            ViewBag.MaxV = dm.MaxV;
            ViewBag.PlanePlot = ViewBagMatrix.Instance.PlanePlot;

            return View();

        }
        public async Task<IActionResult> Slice(string pdbcode = "",            
            string c_xyz = "", string l_xyz = "", string p_xyz = "",
            string ca = "", string la = "", string pa = "",
            string denplot ="", string radplot="", string lapplot = "",
            double width = -1, double gap = -1)
        {

            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode);

            ViewBagMatrix.Instance.DenPlot = denplot;
            ViewBagMatrix.Instance.RadPlot = radplot;
            ViewBagMatrix.Instance.LapPlot = lapplot;
            ViewBagMatrix.Instance.Width = width;
            ViewBagMatrix.Instance.Gap = gap;
            ViewBagMatrix.Instance.SetCentral(c_xyz, ca, DensitySingleton.Instance.PA);
            ViewBagMatrix.Instance.SetLinear(l_xyz, la, DensitySingleton.Instance.PA);
            ViewBagMatrix.Instance.SetPlanar(p_xyz, pa, DensitySingleton.Instance.PA);

            bool recalc = ViewBagMatrix.Instance.Refresh;
                        
            
                                    
            if (recalc)
            {                
                dm.create_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear,ViewBagMatrix.Instance.Planar);
            }

            ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
            ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;

            ViewBag.SliceDensity = dm.SliceDensity;
            ViewBag.SliceRadiant = dm.SliceRadiant;
            ViewBag.SliceLaplacian = dm.SliceLaplacian;
            ViewBag.SliceAxis = dm.SliceAxis;

            ViewBag.cAtom = ViewBagMatrix.Instance.CentralAtom;
            ViewBag.lAtom = ViewBagMatrix.Instance.LinearAtom;
            ViewBag.pAtom = ViewBagMatrix.Instance.PlanarAtom;

            ViewBag.cXYZ = "(" + Convert.ToString(ViewBagMatrix.Instance.Central.A) + "," + Convert.ToString(ViewBagMatrix.Instance.Central.B) + "," + Convert.ToString(ViewBagMatrix.Instance.Central.C) + ")";
            ViewBag.lXYZ = "(" + Convert.ToString(ViewBagMatrix.Instance.Linear.A) + "," + Convert.ToString(ViewBagMatrix.Instance.Linear.B) + "," + Convert.ToString(ViewBagMatrix.Instance.Linear.C) + ")";
            ViewBag.pXYZ = "(" + Convert.ToString(ViewBagMatrix.Instance.Planar.A) + "," + Convert.ToString(ViewBagMatrix.Instance.Planar.B) + "," + Convert.ToString(ViewBagMatrix.Instance.Planar.C) + ")";
            
            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;

            ViewBag.DenPlot = ViewBagMatrix.Instance.DenPlot;
            ViewBag.RadPlot = ViewBagMatrix.Instance.RadPlot;
            ViewBag.LapPlot = ViewBagMatrix.Instance.LapPlot;
            ViewBag.Width = ViewBagMatrix.Instance.Width;
            ViewBag.Gap = ViewBagMatrix.Instance.Gap;
            ViewBag.BrowseAtoms = DensitySingleton.Instance.FD.PdbViewLink;
            ViewBag.CDistance = ViewBagMatrix.Instance.CDistance;
            ViewBag.LDistance = ViewBagMatrix.Instance.LDistance;
            ViewBag.PDistance = ViewBagMatrix.Instance.PDistance;

            ViewBagMatrix.Instance.Reset();
            return View();
        }

    }
}
