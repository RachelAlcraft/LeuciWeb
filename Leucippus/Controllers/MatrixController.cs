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

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp,2,-1);
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

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp,2,-1);
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
            string valsd = "",double sdcap = -100, double sdfloor = -100,
            int Fos=2, int Fcs=-1,string dots="Y", string gdots="Y",
            int t1=0,int t2=0,int t3=0,int t4=0,
            string nav = "",double nav_mag=0.1)
        {
            if (t1 + t2 + t3 + t4 > 0)//then this is a view only change
            {

                ViewBagMatrix.Instance.T1Display = "none";
                ViewBagMatrix.Instance.T2Display = "none";
                ViewBagMatrix.Instance.T3Display = "none";
                ViewBagMatrix.Instance.T4Display = "none";
                if (t1 == 1)
                    ViewBagMatrix.Instance.T1Display = "block";
                if (t2 == 1)
                    ViewBagMatrix.Instance.T2Display = "block";
                if (t3 == 1)
                    ViewBagMatrix.Instance.T3Display = "block";
                if (t4 == 1)
                    ViewBagMatrix.Instance.T4Display = "block";

                


            }
            else
            {
                ViewBagMatrix.Instance.PdbCode = pdbcode;
                ViewBagMatrix.Instance.Interp = interp;
                ViewBagMatrix.Instance.setFoFc(Fos, Fcs);
            }
            try
            {
                ViewBag.Error = "";
                
                if (ViewBagMatrix.Instance.Refresh)
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.PA, true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.PA, true);
                }

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp,ViewBagMatrix.Instance.Fos,ViewBagMatrix.Instance.Fcs);

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
                ViewBagMatrix.Instance.SdFloor = sdfloor;
                ViewBagMatrix.Instance.SdFloor = Math.Round(ViewBagMatrix.Instance.SdFloor, 2);
                ViewBagMatrix.Instance.YellowDots = dots;
                ViewBagMatrix.Instance.GreenDots = gdots;

                if (nav != "")
                {
                    dm.Space = new SpaceTransformation(ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar);
                    ViewBagMatrix.Instance.Central = dm.Space.extraNav(ViewBagMatrix.Instance.Central,nav,nav_mag);
                    ViewBagMatrix.Instance.Linear = dm.Space.extraNav(ViewBagMatrix.Instance.Linear, nav, nav_mag);
                    ViewBagMatrix.Instance.Planar = dm.Space.extraNav(ViewBagMatrix.Instance.Planar, nav, nav_mag);
                }

                bool recalc = ViewBagMatrix.Instance.Refresh;
                dm.create_scratch_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, 
                    ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap, ViewBagMatrix.Instance.SdFloor, 
                    ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar,
                    ViewBagMatrix.Instance.CAtom, ViewBagMatrix.Instance.LAtom, ViewBagMatrix.Instance.PAtom);

                if (recalc)
                {
                    //dm.create_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap,ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear,ViewBagMatrix.Instance.Planar);
                }
                ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
                ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;

                ViewBag.SliceDensity = dm.SliceDensity;
                ViewBag.SlicePositionX = dm.SlicePositionX;
                ViewBag.SlicePositionY = dm.SlicePositionY;
                ViewBag.SliceProjAtomsX = dm.SliceProjAtomsX;
                ViewBag.SliceProjAtomsY = dm.SliceProjAtomsY;
                ViewBag.SlicePlaneAtomsX = dm.SlicePlaneAtomsX;
                ViewBag.SlicePlaneAtomsY = dm.SlicePlaneAtomsY;
                ViewBag.SliceRadient = dm.SliceRadient;
                ViewBag.SliceLaplacian = dm.SliceLaplacian;
                ViewBag.SliceAxis = dm.SliceAxis;
                ViewBag.LMax = dm.LMax;
                ViewBag.LMin = dm.LMin;


                ViewBag.cAtom = ViewBagMatrix.Instance.CentralAtom;
                ViewBag.lAtom = ViewBagMatrix.Instance.LinearAtom;
                ViewBag.pAtom = ViewBagMatrix.Instance.PlanarAtom;

                ViewBag.cXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.A,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.B,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.C,4)) + ")";
                ViewBag.lXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.A,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.B,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.C,4)) + ")";
                ViewBag.pXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.A,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.B,4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.C,4)) + ")";

                ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
                ViewBag.Fos = ViewBagMatrix.Instance.Fos;
                ViewBag.Fcs = ViewBagMatrix.Instance.Fcs;
                ViewBag.YellowDots = ViewBagMatrix.Instance.YellowDots;
                ViewBag.GreenDots = ViewBagMatrix.Instance.GreenDots;

                ViewBag.DenPlot = ViewBagMatrix.Instance.DenPlot;
                ViewBag.RadPlot = ViewBagMatrix.Instance.RadPlot;
                ViewBag.LapPlot = ViewBagMatrix.Instance.LapPlot;
                ViewBag.ValSd = ViewBagMatrix.Instance.ValSd;
                ViewBag.SdCap = ViewBagMatrix.Instance.SdCap;
                ViewBag.SdFloor = ViewBagMatrix.Instance.SdFloor;

                ViewBag.DenMax = Math.Round(dm.DenMax, 2);
                ViewBag.DenMin = Math.Round(dm.DenMin,2);
                
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

                ViewBag.T1Display = ViewBagMatrix.Instance.T1Display;
                ViewBag.T2Display = ViewBagMatrix.Instance.T2Display;
                ViewBag.T3Display = ViewBagMatrix.Instance.T3Display;
                ViewBag.T4Display = ViewBagMatrix.Instance.T4Display;


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
                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp, 2, -1);
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
