using ChartDirector;
using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using System.Collections.Generic;
using System.Numerics;
using static Plotly.NET.StyleParam;


namespace Leucippus.Controllers
{
    public class MatrixController : Controller
    {
        //ElectronDensity ed = new ElectronDensity("");
        //string _pdbcode = ViewBagMatrix.Instance.PdbCode;        
        //string _plane = "";
        //int _layer = 0;

        public async Task <IActionResult> Index(string pdbcode = "", string emcode = "")
        {
            ViewBagMatrix.Instance.PdbCode = pdbcode;
            ViewBagMatrix.Instance.EmCode = emcode;            
            
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.EmCode);
            ViewBagMatrix.Instance.Info = dm.Info;

            if (!MatrixServer.Instance.init)
            {
                pdbcode = "6eex";
                await MatrixServer.Instance.setPdbCode("6eex");                
            }
            else if (pdbcode == "")
            {                
                pdbcode = MatrixServer.Instance.PdbCode;
            }
            else if (MatrixServer.Instance.PdbCode != pdbcode)
            {
                await MatrixServer.Instance.setPdbCode(pdbcode);                
            }

            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
            ViewBag.EmCode = ViewBagMatrix.Instance.EmCode;
            ViewBag.Info = ViewBagMatrix.Instance.Info;
            ViewBag.EbiLink = MatrixServer.Instance.ed.EbiLink;

            

            return View();            
        }
        
        public async Task<IActionResult> Plane(int layer = -1, string plane="")
        {            
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
            bool newCalcs = true;

            ViewBagMatrix.Instance.Plane = plane;
            ViewBagMatrix.Instance.Layer = layer;
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.EmCode);
            dm.calculatePlane(ViewBagMatrix.Instance.Plane, ViewBagMatrix.Instance.Layer);

            if (!MatrixServer.Instance.init)
            {                
                layer = 0;
                plane = "XY";
                await MatrixServer.Instance.setPdbCode(ViewBagMatrix.Instance.EmCode);                
                newCalcs = true;

            }
            else
            {                
                if (plane == "")
                    plane = MatrixServer.Instance.ed.Plane;
                if (layer == -1)
                    layer = MatrixServer.Instance.ed.Layer;
            }
            if (ViewBagMatrix.Instance.EmCode == "" && layer == -1 && plane == "")            
                newCalcs = false;
                                                                                        
            if (MatrixServer.Instance.PdbCode != ViewBagMatrix.Instance.EmCode)
            {
                await MatrixServer.Instance.setPdbCode(ViewBagMatrix.Instance.EmCode);
                newCalcs = true;
            }
            else
            {
                if (MatrixServer.Instance.ed.Layer != layer)
                {
                    newCalcs = true;
                    MatrixServer.Instance.ed.Layer = layer;
                }
                if (MatrixServer.Instance.ed.Plane != plane)
                {
                    newCalcs = true;
                    MatrixServer.Instance.ed.Plane = plane;
                }
            }
            
            if (newCalcs)
            {
                MatrixServer.Instance.ed.calculateWholeLayer(plane, layer);                
            }            
            ViewBag.MtxX = MatrixServer.Instance.ed.MtxA;
            ViewBag.MtxY = MatrixServer.Instance.ed.MtxB;
            ViewBag.MtxZ = MatrixServer.Instance.ed.MtxC;
            ViewBag.MtxV = MatrixServer.Instance.ed.MtxD;
            
            ViewBag.MtxX2 = dm.MatA;
            ViewBag.MtxY2 = dm.MatB;
            ViewBag.MtxZ2 = dm.MatC;
            ViewBag.MtxV2 = dm.MatD;

            ViewBag.PdbCode = MatrixServer.Instance.ed.PdbCode;            
            ViewBag.Plane = MatrixServer.Instance.ed.Plane;
            ViewBag.Layer = MatrixServer.Instance.ed.Layer;
            ViewBag.LayerMax = MatrixServer.Instance.ed.LayerMax - 1;
            ViewBag.MinV = MatrixServer.Instance.ed.MinV;
            ViewBag.MaxV = MatrixServer.Instance.ed.MaxV;
            return View();

        }
        public async Task<IActionResult> Slice(string pdbcode = "", 
            double cx = -1, double cy = -1, double cz = -1,
            double lx = -1, double ly = -1, double lz = -1, 
            double px = -1, double py = -1, double pz = -1,
            string denplot ="", string radplot="", string lapplot = "",
            double width = -1, double gap = -1)
        {
            ViewBagMatrix.Instance.DenPlot = denplot;
            ViewBagMatrix.Instance.RadPlot = radplot;
            ViewBagMatrix.Instance.LapPlot = lapplot;
            ViewBagMatrix.Instance.Width = width;
            ViewBagMatrix.Instance.Gap = gap;

            bool recalc = ViewBagMatrix.Instance.Refresh;
            if (!MatrixServer.Instance.init)
            {
                if (pdbcode == "")
                    pdbcode = "6eex";                
                await MatrixServer.Instance.setPdbCode(pdbcode);
                recalc = true;
            }
            else
            {
                if (pdbcode == "")
                    pdbcode = MatrixServer.Instance.PdbCode;
                if (pdbcode != MatrixServer.Instance.PdbCode)
                {
                    MatrixServer.Instance.PdbCode = pdbcode;
                    await MatrixServer.Instance.setPdbCode(pdbcode);
                    recalc = true;
                }
                if (cx == -1)
                {
                    cx = MatrixServer.Instance.ed.CX;
                    cy = MatrixServer.Instance.ed.CY;
                    cz = MatrixServer.Instance.ed.CZ;
                    lx = MatrixServer.Instance.ed.LX;
                    ly = MatrixServer.Instance.ed.LY;
                    lz = MatrixServer.Instance.ed.LZ;
                    px = MatrixServer.Instance.ed.PX;
                    py = MatrixServer.Instance.ed.PY;
                    pz = MatrixServer.Instance.ed.PZ;
                    //recalc = false;
                }
                else
                {
                    MatrixServer.Instance.ed.CX = cx;
                    MatrixServer.Instance.ed.CY = cy;
                    MatrixServer.Instance.ed.CZ = cz;
                    MatrixServer.Instance.ed.LX = lx;
                    MatrixServer.Instance.ed.LY = ly;
                    MatrixServer.Instance.ed.LZ = lz;
                    MatrixServer.Instance.ed.PX = px;
                    MatrixServer.Instance.ed.PY = py;
                    MatrixServer.Instance.ed.PZ = pz;
                }
            }

            //if (MatrixServer.Instance.ed.SliceAxis.Length == 0)
                //recalc = true;
            if (recalc)
                MatrixServer.Instance.ed.getSlice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap);
            ViewBag.SliceDensity = MatrixServer.Instance.ed.SliceDensity;
            ViewBag.SliceRadiant = MatrixServer.Instance.ed.SliceRadiant;
            ViewBag.SliceLaplacian = MatrixServer.Instance.ed.SliceLaplacian;
            ViewBag.SliceAxis = MatrixServer.Instance.ed.SliceAxis;

            ViewBag.cx = MatrixServer.Instance.ed.CX;
            ViewBag.cy = MatrixServer.Instance.ed.CY;
            ViewBag.cz = MatrixServer.Instance.ed.CZ;
            ViewBag.lx = MatrixServer.Instance.ed.LX;
            ViewBag.ly = MatrixServer.Instance.ed.LY;
            ViewBag.lz = MatrixServer.Instance.ed.LZ;
            ViewBag.px = MatrixServer.Instance.ed.PX;
            ViewBag.py = MatrixServer.Instance.ed.PY;
            ViewBag.pz = MatrixServer.Instance.ed.PZ;

            ViewBag.PdbCode = MatrixServer.Instance.ed.PdbCode;

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
