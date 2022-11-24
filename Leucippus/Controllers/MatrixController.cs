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
                    string[] first_three = DensitySingleton.Instance.FD.PA.getFirstThreeCoords();
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
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.FD.PA, 0,true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.FD.PA, 0,true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.FD.PA, 0,true);
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
        public async Task<IActionResult> Slice(
            string tabview="A", // A=atoms,S=settgins,N=neighbour X=advanced
            string refresh_mode = "R", //V= viewbag only F = force
            string pdbcode = "",
            string c_xyz = "", string l_xyz = "", string p_xyz = "",
            string ca = "", string la = "", string pa = "",int atom_offset=0,
            string denplot = "", string radplot = "", string lapplot = "",
            string denhue = "", string radhue = "", string laphue = "",
            string denbar = "", string radbar = "", string lapbar = "",
            double width = -1, double gap = -1, string interp = "", 
            string valsd = "",double sdcap = -100, double sdfloor = -100,
            int Fos=2, int Fcs=-1,string ydots="N", string gdots="N",
            int t1=0,int t2=0,int t3=0,int t4=0,
            string nav = "",double nav_distance=0.1,
            double hover_min=0,double hover_max=0)
        {
            bool view_change = false;
              
            if (t1 + t2 + t3 + t4 > 0)//then this is a view only change
            {
                view_change = true;
            }
                        
            if (view_change)//then this is a view only change
            {

                ViewBagMatrix.Instance.T1Display = "none";
                ViewBagMatrix.Instance.T2Display = "none";
                ViewBagMatrix.Instance.T3Display = "none";
                ViewBagMatrix.Instance.T4Display = "none";
                if (t1 == 1) //atoms
                {
                    ViewBagMatrix.Instance.T1Display = "block";
                    ViewBag.TabView = "A";
                    refresh_mode = "F";
                }
                if (t2 == 1) //settings
                {
                    ViewBagMatrix.Instance.T2Display = "block";
                    ViewBag.TabView = "S";
                }
                if (t3 == 1) //neighbours
                {
                    ViewBagMatrix.Instance.T3Display = "block";
                    ViewBag.TabView = "N";
                    refresh_mode = "F";
                }
                if (t4 == 1)//advanced
                {
                    ViewBagMatrix.Instance.T4Display = "block";
                    ViewBag.TabView = "X";
                }

            }
            else
            {
                //we should make sure we have the correct block selected
                ViewBagMatrix.Instance.T1Display = "none";
                ViewBagMatrix.Instance.T2Display = "none";
                ViewBagMatrix.Instance.T3Display = "none";
                ViewBagMatrix.Instance.T4Display = "none";
                ViewBag.TabView = tabview;
                if (tabview == "A")
                    ViewBagMatrix.Instance.T1Display = "block";
                else if (tabview == "N")
                    ViewBagMatrix.Instance.T3Display = "block";
                else if (tabview == "S")
                    ViewBagMatrix.Instance.T2Display = "block";
                else if (tabview == "X")
                    ViewBagMatrix.Instance.T4Display = "block";

            }

            
            try
            {
                ViewBag.Error = "";                
                ViewBagMatrix.Instance.PdbCode = pdbcode;
                ViewBagMatrix.Instance.Interp = interp;
                ViewBagMatrix.Instance.setFoFc(Fos, Fcs);
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
                ViewBagMatrix.Instance.ValSd = valsd;
                ViewBagMatrix.Instance.SdCap = sdcap;
                ViewBagMatrix.Instance.SdCap = Math.Round(ViewBagMatrix.Instance.SdCap, 2);
                ViewBagMatrix.Instance.SdFloor = sdfloor;
                ViewBagMatrix.Instance.SdFloor = Math.Round(ViewBagMatrix.Instance.SdFloor, 2);
                ViewBagMatrix.Instance.YellowDots = ydots;
                ViewBagMatrix.Instance.GreenDots = gdots;
                ViewBagMatrix.Instance.HoverMin = hover_min;
                ViewBagMatrix.Instance.HoverMax = hover_max;
                ViewBagMatrix.Instance.NavDistance = nav_distance;

                if (ViewBagMatrix.Instance.Refresh && c_xyz == "" && l_xyz == "" && p_xyz == "")
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.FD.PA, atom_offset, true);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.FD.PA, atom_offset,true);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.FD.PA, atom_offset,true);
                }

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp, ViewBagMatrix.Instance.Fos, ViewBagMatrix.Instance.Fcs);

                ViewBagMatrix.Instance.SetCentral(c_xyz, ca, DensitySingleton.Instance.FD.PA, atom_offset, c_xyz + ca == "");
                ViewBagMatrix.Instance.SetLinear(l_xyz, la, DensitySingleton.Instance.FD.PA, atom_offset, l_xyz + la == "");
                ViewBagMatrix.Instance.SetPlanar(p_xyz, pa, DensitySingleton.Instance.FD.PA, atom_offset, p_xyz + pa == "");

                double nav_space = ViewBagMatrix.Instance.Gap;
                double hov_min = ViewBagMatrix.Instance.HoverMin;
                double hov_max = ViewBagMatrix.Instance.HoverMax;

                if (interp == "" && ViewBagMatrix.Instance.DensityType == "cryo-em")
                {
                    nav_space = ViewBagMatrix.Instance.Width / 20; //we reduce for cryo-em defaults like nav mode
                    hov_min = -1;
                    hov_max = -1;
                    ViewBagMatrix.Instance.Interp = "BSPLINE3";
                }

                if (tabview == "N")
                {
                    nav_space = ViewBagMatrix.Instance.Width / 10; //we reduce dramatically for nearest neighbor as how often do we need to look?
                }
                else
                {
                    hov_min = -1;
                    hov_max = -1;
                }
                if (atom_offset != 0)
                {
                    nav_space = ViewBagMatrix.Instance.Width / 20; //we dramatically reduce the pixels for navigation mode.
                    hov_min = -1;
                    hov_max = -1;
                }

                if (nav != "" && nav != null)
                {
                    if (nav == "plus")
                    {
                        double ratio = ViewBagMatrix.Instance.Width / ViewBagMatrix.Instance.Gap;
                        ViewBagMatrix.Instance.Width += 0.1;
                        ViewBagMatrix.Instance.Gap = ViewBagMatrix.Instance.Width / ratio;
                    }
                    else if (nav == "minus")
                    {
                        double ratio = ViewBagMatrix.Instance.Width / ViewBagMatrix.Instance.Gap;
                        ViewBagMatrix.Instance.Width -= 0.1;
                        ViewBagMatrix.Instance.Gap = ViewBagMatrix.Instance.Width / ratio;
                    }
                    else
                    {
                        dm.Space = new SpaceTransformation(ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar);
                        ViewBagMatrix.Instance.Central = dm.Space.extraNav(ViewBagMatrix.Instance.Central, nav, nav_distance);
                        ViewBagMatrix.Instance.Linear = dm.Space.extraNav(ViewBagMatrix.Instance.Linear, nav, nav_distance);
                        ViewBagMatrix.Instance.Planar = dm.Space.extraNav(ViewBagMatrix.Instance.Planar, nav, nav_distance);

                        string cXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.C, 4)) + ")";
                        string lXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.C, 4)) + ")";
                        string pXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.C, 4)) + ")";

                        ViewBagMatrix.Instance.SetCentral(cXYZ2, ca, DensitySingleton.Instance.FD.PA, atom_offset, c_xyz + ca == "");
                        ViewBagMatrix.Instance.SetLinear(lXYZ2, la, DensitySingleton.Instance.FD.PA, atom_offset,l_xyz + la == "");
                        ViewBagMatrix.Instance.SetPlanar(pXYZ2, pa, DensitySingleton.Instance.FD.PA, atom_offset, p_xyz + pa == "");
                    }

                    nav_space = ViewBagMatrix.Instance.Width / 20; //we dramatically reduce the pixels for navigation mode.
                    hov_min = -1;
                    hov_max = -1;

                }

                bool recalc = ViewBagMatrix.Instance.Refresh;
                if (recalc || refresh_mode == "F")
                {
                    dm.create_scratch_slice(ViewBagMatrix.Instance.Width, nav_space,
                    ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap, ViewBagMatrix.Instance.SdFloor,
                    ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear, ViewBagMatrix.Instance.Planar,
                    ViewBagMatrix.Instance.CAtom, ViewBagMatrix.Instance.LAtom, ViewBagMatrix.Instance.PAtom, DensitySingleton.Instance.FD.PA,
                    hov_min, hov_max
                    );
                    //dm.create_slice(ViewBagMatrix.Instance.Width, ViewBagMatrix.Instance.Gap, ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap,ViewBagMatrix.Instance.Central, ViewBagMatrix.Instance.Linear,ViewBagMatrix.Instance.Planar);
                }
                ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
                ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;


                ViewBag.SliceDensity = dm.SliceDensity;
                ViewBag.SlicePositionX = dm.SlicePositionX;
                ViewBag.SlicePositionY = dm.SlicePositionY;
                ViewBag.Annotations = dm.Annotations;
                ViewBag.SliceProjGreenAtomsX = dm.SliceProjGreenAtomsX;
                ViewBag.SliceProjGreenAtomsY = dm.SliceProjGreenAtomsY;
                ViewBag.SliceProjBlueAtomsX = dm.SliceProjBlueAtomsX;
                ViewBag.SliceProjBlueAtomsY = dm.SliceProjBlueAtomsY;
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

                ViewBag.cXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Central.C, 4)) + ")";
                ViewBag.lXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Linear.C, 4)) + ")";
                ViewBag.pXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.Planar.C, 4)) + ")";

                ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
                ViewBag.Fos = ViewBagMatrix.Instance.Fos;
                ViewBag.Fcs = ViewBagMatrix.Instance.Fcs;
                ViewBag.YellowDots = ViewBagMatrix.Instance.YellowDots;
                ViewBag.GreenDots = ViewBagMatrix.Instance.GreenDots;
                ViewBag.HoverMin = hover_min;
                ViewBag.HoverMax = hover_max;

                ViewBag.DenPlot = ViewBagMatrix.Instance.DenPlot;
                ViewBag.RadPlot = ViewBagMatrix.Instance.RadPlot;
                ViewBag.LapPlot = ViewBagMatrix.Instance.LapPlot;
                ViewBag.ValSd = ViewBagMatrix.Instance.ValSd;
                ViewBag.SdCap = ViewBagMatrix.Instance.SdCap;
                ViewBag.SdFloor = ViewBagMatrix.Instance.SdFloor;

                ViewBag.DenMax = Math.Round(dm.DenMax, 2);
                ViewBag.DenMin = Math.Round(dm.DenMin, 2);


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
                ViewBag.DenBar = ViewBagMatrix.Instance.IsDenBar ? "Y" : "N";
                ViewBag.RadBar = ViewBagMatrix.Instance.IsRadBar ? "Y" : "N";
                ViewBag.LapBar = ViewBagMatrix.Instance.IsLapBar ? "Y" : "N";
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

                ViewBag.NavDistance = ViewBagMatrix.Instance.NavDistance;
                ViewBag.RefreshMode = "R";
                ViewBag.AtomOffset = 0;


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
                DataFiles dfs = new DataFiles("wwwroot/App_Data/");
                List<DataFile> dataFiles = dfs.Files;
                ViewBag.BbkPdbs = dfs.BbkPdbs;
                ViewBag.SmallPdbs = dfs.SmallPdbs;
                ViewBag.HighPdbs = dfs.HighPdbs;
                ViewBag.SmallEmPdbs = dfs.SmallEmPdbs;
                ViewBag.HighEmPdbs = dfs.HighEmPdbs;
                ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
                ViewBag.EmCode = ViewBagMatrix.Instance.EmCode;
                ViewBag.EbiLink = ViewBagMatrix.Instance.EbiLink;

                //Load the density matrix
                DensitySingleton.Instance.FD = new FileDownloads(ViewBag.PdbCode);
                string pdbStatus = await DensitySingleton.Instance.FD.existsPdbMatrixAsync();
                if (pdbStatus == "Y")
                {
                    string ccp4Status = await DensitySingleton.Instance.FD.existsCcp4Matrix();
                    if (ccp4Status == "Y")
                    {

                        DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, "LINEAR", 2, -1);

                        ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
                        ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;
                        ViewBag.Resolution = DensitySingleton.Instance.FD.Resolution;
                        ViewBagMatrix.Instance.Info = dm.Info;
                        ViewBag.Info = ViewBagMatrix.Instance.Info;
                        ViewBag.DensityType = ViewBagMatrix.Instance.DensityType;
                    }
                    else
                    {
                        ViewBag.Error = "Ccp4 matrix: " + ccp4Status;
                    }
                }
                else
                {
                    ViewBag.Error = "Pdb file does not exist " + pdbcode;
                }

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
