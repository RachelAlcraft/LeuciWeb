using ChartDirector;
using Leucippus.Models;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using System.Collections.Generic;
using System.Numerics;
using static Plotly.NET.StyleParam;

namespace Leucippus.Controllers
{
    public class PlotlyController : Controller
    {
        //ElectronDensity ed = new ElectronDensity("");
        //string _pdbcode = "";
        //string _plane = "";
        //int _layer = 0;
        public async Task <IActionResult> Index(string pdbcode = "")
        {
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

            ViewBag.PdbCode = MatrixServer.Instance.ed.PdbCode;
            ViewBag.Info = MatrixServer.Instance.ed.Info;
            ViewBag.EbiLink = MatrixServer.Instance.ed.EbiLink;

            return View();            
        }
        
        public async Task<IActionResult> Matrix(string pdbcode = "", int layer = -1, string plane="")
        {            
            //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
            bool newCalcs = true;
            
            if (!MatrixServer.Instance.init)
            {
                pdbcode = "6eex";
                layer = 0;
                plane = "XY";
                await MatrixServer.Instance.setPdbCode("6eex");                
                newCalcs = true;

            }
            else
            {
                if (pdbcode == "")
                    pdbcode = MatrixServer.Instance.PdbCode;
                if (plane == "")
                    plane = MatrixServer.Instance.ed.Plane;
                if (layer == -1)
                    layer = MatrixServer.Instance.ed.Layer;
            }
            if (pdbcode == "" && layer == -1 && plane == "")            
                newCalcs = false;
                                                                                        
            if (MatrixServer.Instance.PdbCode != pdbcode)
            {
                await MatrixServer.Instance.setPdbCode(pdbcode);
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
            double px = -1, double py = -1, double pz = -1)
        {
            bool recalc = true;
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
                    recalc = false;
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
                MatrixServer.Instance.ed.getSlice();
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


            

            return View();
        }

    }
}
