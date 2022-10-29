using ChartDirector;
using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;
using ScottPlot.Statistics.Interpolation;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading;
using static Plotly.NET.StyleParam;


namespace Leucippus.Controllers
{
    public class MatrixController : Controller
    {        
        public async Task <IActionResult> Index(string pdbcode = "")
        {
            try
            {
                ViewBag.Error = "";
                ViewBagMatrix.Instance.PdbCode = pdbcode;

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp);
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
            catch(Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }
            
        }
        
        public async Task<IActionResult> Plane(string pdbcode = "",int layer = -1, string plane="",string planeplot="")
        {
            try
            {
                ViewBag.Error = "";
                //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
                bool newCalcs = true;
                ViewBagMatrix.Instance.PdbCode = pdbcode;
                if (ViewBagMatrix.Instance.Refresh)
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.PA, true);
                }

                ViewBagMatrix.Instance.Plane = plane;
                ViewBagMatrix.Instance.Layer = layer;
                ViewBagMatrix.Instance.PlanePlot = planeplot;

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp);
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
                ViewBag.LayerMax = dm.LayerMax - 1;
                ViewBag.MinV = dm.DMin;
                ViewBag.MaxV = dm.DMax;
                ViewBag.PlanePlot = ViewBagMatrix.Instance.PlanePlot;

                return View();
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }

        }
        public async Task<IActionResult> Slice(string pdbcode = "",
            string c_xyz = "", string l_xyz = "", string p_xyz = "",
            string ca = "", string la = "", string pa = "",
            string denplot = "", string radplot = "", string lapplot = "",
            string denhue = "", string radhue = "", string laphue = "",
            string denbar = "", string radbar = "", string lapbar = "",
            double width = -1, double gap = -1, string interp = "", 
            string valsd = "",double sdcap = -1)
        {
            try
            {
                ViewBag.Error = "";

                ViewBagMatrix.Instance.PdbCode = pdbcode;
                ViewBagMatrix.Instance.Interp = interp;

                if (ViewBagMatrix.Instance.Refresh)
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.PA, true);
                }

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp);

                ViewBagMatrix.Instance.DenPlot = denplot;
                ViewBagMatrix.Instance.RadPlot = radplot;
                ViewBagMatrix.Instance.LapPlot = lapplot;
                ViewBagMatrix.Instance.DenHue = denhue;
                ViewBagMatrix.Instance.RadHue = radhue;
                ViewBagMatrix.Instance.LapHue = laphue;
                ViewBagMatrix.Instance.DenBar = denbar;
                ViewBagMatrix.Instance.RadBar = radbar;
                ViewBagMatrix.Instance.LapBar = lapbar;
                ViewBagMatrix.Instance.Width = width;
                ViewBagMatrix.Instance.Gap = gap;
                ViewBagMatrix.Instance.SetCentral(c_xyz, ca, DensitySingleton.Instance.PA);
                ViewBagMatrix.Instance.SetLinear(l_xyz, la, DensitySingleton.Instance.PA);
                ViewBagMatrix.Instance.SetPlanar(p_xyz, pa, DensitySingleton.Instance.PA);
                ViewBagMatrix.Instance.ValSd = valsd;
                ViewBagMatrix.Instance.SdCap = sdcap;
                ViewBagMatrix.Instance.SdCap = Math.Round(ViewBagMatrix.Instance.SdCap, 2);

                bool recalc = ViewBagMatrix.Instance.Refresh;
                dm.create_scratch_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap, ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar);
                if (recalc)
                {
                    //dm.create_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap,ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear,ViewBagMatrix.Instance.Planar);
                }
                ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
                ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;

                ViewBag.SliceDensity = dm.SliceDensity;
                ViewBag.SliceRadient = dm.SliceRadient;
                ViewBag.SliceLaplacian = dm.SliceLaplacian;
                ViewBag.SliceAxis = dm.SliceAxis;
                ViewBag.LMax = dm.LMax;
                ViewBag.LMin = dm.LMin;


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
                ViewBag.ValSd = ViewBagMatrix.Instance.ValSd;
                ViewBag.SdCap = ViewBagMatrix.Instance.SdCap;

                ViewBag.DenMax = Math.Round(dm.DenMax, 2);
                ViewBag.DenMin = dm.DenMin;
                ViewBag.ThreeSd = Math.Round(dm.ThreeSd, 2);

                ViewBag.Width = ViewBagMatrix.Instance.Width;
                ViewBag.Gap = ViewBagMatrix.Instance.Gap;
                ViewBag.Interp = ViewBagMatrix.Instance.Interp;
                ViewBag.BrowseAtoms = DensitySingleton.Instance.FD.PdbViewLink;
                ViewBag.CDistance = ViewBagMatrix.Instance.CDistance;
                ViewBag.LDistance = ViewBagMatrix.Instance.LDistance;
                ViewBag.PDistance = ViewBagMatrix.Instance.PDistance;
                ViewBag.DenHue = ViewBagMatrix.Instance.DenHue;
                ViewBag.RadHue = ViewBagMatrix.Instance.RadHue;
                ViewBag.LapHue = ViewBagMatrix.Instance.LapHue;
                ViewBag.DenBar = ViewBagMatrix.Instance.IsDenBar;
                ViewBag.RadBar = ViewBagMatrix.Instance.IsRadBar;
                ViewBag.LapBar = ViewBagMatrix.Instance.IsLapBar;
                ViewBag.CColor = "black";
                ViewBag.LColor = "black";
                ViewBag.PColor = "black";
                if (ViewBag.CDistance > 0)
                    ViewBag.CColor = "silver";
                if (ViewBag.LDistance > 0)
                    ViewBag.LColor = "silver";
                if (ViewBag.PDistance > 0)
                    ViewBag.PColor = "silver";

                ViewBagMatrix.Instance.Reset();
                return View();
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }
        }

        public async Task<IActionResult> Browse(string pdbcode = "")
        {
            ViewBag.SmallPdbs = new List<DataFile>();
            ViewBag.HighPdbs = new List<DataFile>();
            ViewBag.SmallEmPdbs = new List<DataFile>();
            ViewBag.HighEmPdbs = new List<DataFile>();
            try
            {
                ViewBag.Error = "";
                ViewBagMatrix.Instance.PdbCode = pdbcode;
                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp);
                if (ViewBagMatrix.Instance.Refresh)
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.PA, true);
                }

                DataFiles dfs = new DataFiles("wwwroot/App_Data/");
                List<DataFile> dataFiles = dfs.Files;
                ViewBag.SmallPdbs = dfs.SmallPdbs;
                ViewBag.HighPdbs = dfs.HighPdbs;
                ViewBag.SmallEmPdbs = dfs.SmallEmPdbs;
                ViewBag.HighEmPdbs = dfs.HighEmPdbs;
                //List<string> sms = new List<string>();
                //sms.Add("6eex");
                //sms.Add("6fgz");
                //sms.Add("6q53");
                //sms.Add("2fd7");
                //sms.Add("6y50");           
                //ViewBag.Pdbs = sms;
                //ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;

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
            catch (Exception e)
            {                
                ViewBag.Error = "Error from server:" + e.Message;
                return View();
            }
        }

    }
}
