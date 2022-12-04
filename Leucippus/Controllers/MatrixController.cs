using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using Plotly.NET;


namespace Leucippus.Controllers
{
    public class MatrixController : Controller
    {
        public async Task<IActionResult> Index(string pdbcode = "")
        {
            try
            {
                ViewBag.Error = "";
                ViewBagMatrix.Instance.PdbCode = pdbcode;

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp, 2, -1);
                if (DensitySingleton.Instance.NewMatrix)
                {
                    string[] first_three = DensitySingleton.Instance.FD.PA.getFirstThreeCoords();
                    ViewBagMatrix.Instance.CentralAtomStrucString = first_three[0];
                    ViewBagMatrix.Instance.LinearAtomStrucString = first_three[1];
                    ViewBagMatrix.Instance.PlanarAtomStrucString = first_three[2];
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
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }

        }

        public async Task<IActionResult> Plane(string pdbcode = "", int layer = -1, string plane = "", string planeplot = "")
        {
            try
            {
                ViewBag.Error = "";
                //https://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_element_innerhtml            
                bool newCalcs = true;
                ViewBagMatrix.Instance.PdbCode = pdbcode;
                if (ViewBagMatrix.Instance.Refresh)
                {
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.FD.PA, 0);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.FD.PA, 0);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.FD.PA, 0);
                }

                ViewBagMatrix.Instance.Plane = plane;
                ViewBagMatrix.Instance.Layer = layer;
                ViewBagMatrix.Instance.PlanePlot = planeplot;

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp, 2, -1);
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
            string tabview = "S", // A=atoms,S=settgins,N=neighbour X=advanced
            string refresh_mode = "R", //V= viewbag only F = force
            string pdbcode = "",
            string c_xyz = "", string l_xyz = "", string p_xyz = "",
            string ca = "", string la = "", string pa = "", int atom_offset = 0,
            string denplot = "", string radplot = "", string lapplot = "",
            string denhue = "", string radhue = "", string laphue = "",
            string denbar = "", string radbar = "", string lapbar = "",
            double width = -1, double gap = -1, string interp = "",
            string valsd = "", double sdcap = -100, double sdfloor = -100,
            int Fos = 2, int Fcs = -1, string ydots = "N", string gdots = "N",
            int t1 = 0, int t2 = 0, int t3 = 0, int t4 = 0,
            string nav = "", double nav_distance = 0.1,
            double hover_min = 0, double hover_max = 0,string force_slow="N")
        {
            bool view_change = false;

            if (t1 + t2 + t3 + t4 > 0)//then this is a view only change
            {
                view_change = true;
            }

            // visual elements need to be set
            ViewBagMatrix.Instance.T1Display = "none";
            ViewBagMatrix.Instance.T2Display = "none";
            ViewBagMatrix.Instance.T3Display = "none";
            ViewBagMatrix.Instance.T4Display = "none";
            ViewBag.TabName = "Navigate";
            ViewBag.TabAClick = "inherit";
            ViewBag.TabSClick = "inherit";
            ViewBag.TabNClick = "inherit";
            ViewBag.TabXClick = "inherit";
            ViewBag.TabAName = "none";
            ViewBag.TabSName = "none";
            ViewBag.TabNName = "none";
            ViewBag.TabXName = "none";

            if (view_change)//then this is a view only change
            {
                if (t1 == 1) //atoms
                {
                    ViewBag.TabView = "A";
                    ViewBag.TabName = "Navigate";
                    refresh_mode = "F";
                }
                if (t2 == 1) //settings
                {
                    ViewBag.TabView = "S";
                    ViewBag.TabName = "Display";
                }
                if (t3 == 1) //neighbours
                {
                    ViewBag.TabView = "N";
                    refresh_mode = "F";
                    ViewBag.TabName = "Neighbours";
                }
                if (t4 == 1)//advanced
                {
                    ViewBag.TabView = "X";
                    ViewBag.TabName = "Advanced";
                }

            }

            //we should make sure we have the correct block selected                            
            ViewBag.TabView = tabview;
            if (tabview == "A")
            {
                ViewBagMatrix.Instance.T1Display = "block";
                ViewBag.TabAClick = "none";
                ViewBag.TabAName = "contents";
                ViewBag.TabName = "Atoms";
            }
            else if (tabview == "N")
            {
                ViewBagMatrix.Instance.T3Display = "block";
                ViewBag.TabNClick = "none";
                ViewBag.TabNName = "contents";
                ViewBag.TabName = "Neighbours";
            }
            else if (tabview == "S")
            {
                ViewBagMatrix.Instance.T2Display = "block";
                ViewBag.TabSClick = "none";
                ViewBag.TabSName = "contents";
                ViewBag.TabName = "Display";
            }
            else if (tabview == "X")
            {
                ViewBagMatrix.Instance.T4Display = "block";
                ViewBag.TabXClick = "none";
                ViewBag.TabXName = "contents";
                ViewBag.TabName = "Advanced";
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
                    ViewBagMatrix.Instance.SetCentral("", "", DensitySingleton.Instance.FD.PA, atom_offset);
                    ViewBagMatrix.Instance.SetLinear("", "", DensitySingleton.Instance.FD.PA, atom_offset);
                    ViewBagMatrix.Instance.SetPlanar("", "", DensitySingleton.Instance.FD.PA, atom_offset);
                }

                if (DensitySingleton.Instance.needMatrix(ViewBagMatrix.Instance.PdbCode))
                {
                    bool ok = await loadFDFiles(ViewBagMatrix.Instance.PdbCode);
                    if (!ok)
                        return View();
                }

                double nav_space = ViewBagMatrix.Instance.Gap;
                double hov_min = ViewBagMatrix.Instance.HoverMin;
                double hov_max = ViewBagMatrix.Instance.HoverMax;
                string use_interp = ViewBagMatrix.Instance.Interp;

                if (interp == "" && ViewBagMatrix.Instance.DensityType == "cryo-em")
                {
                    nav_space = ViewBagMatrix.Instance.Width / 25; //we reduce for cryo-em defaults like nav mode
                    hov_min = -1;
                    hov_max = -1;
                }

                if (tabview != "S" && force_slow == "N") //for any tabview other than display we want to force a change to linear
                {
                    nav_space = ViewBagMatrix.Instance.Width / 15; //we reduce dramatically for nearest neighbor as how often do we need to look?
                    if (use_interp != "NEAREST")
                    {                        
                        use_interp = "LINEAR";
                    }

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
                    if (use_interp != "NEAREST")
                    {
                        use_interp = "LINEAR";
                    }
                }

                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, use_interp, ViewBagMatrix.Instance.Fos, ViewBagMatrix.Instance.Fcs);

                ViewBagMatrix.Instance.SetCentral(c_xyz, ca, DensitySingleton.Instance.FD.PA, atom_offset);
                ViewBagMatrix.Instance.SetLinear(l_xyz, la, DensitySingleton.Instance.FD.PA, atom_offset);
                ViewBagMatrix.Instance.SetPlanar(p_xyz, pa, DensitySingleton.Instance.FD.PA, atom_offset);


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
                        dm.Space = new SpaceTransformation(ViewBagMatrix.Instance.CentralPosVector, ViewBagMatrix.Instance.LinearPosVector, ViewBagMatrix.Instance.PlanarPosVector);
                        ViewBagMatrix.Instance.CentralPosVector = dm.Space.extraNav(ViewBagMatrix.Instance.CentralPosVector, nav, nav_distance);
                        ViewBagMatrix.Instance.LinearPosVector = dm.Space.extraNav(ViewBagMatrix.Instance.LinearPosVector, nav, nav_distance);
                        ViewBagMatrix.Instance.PlanarPosVector = dm.Space.extraNav(ViewBagMatrix.Instance.PlanarPosVector, nav, nav_distance);

                        string cXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.C, 4)) + ")";
                        string lXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.C, 4)) + ")";
                        string pXYZ2 = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.C, 4)) + ")";

                        ViewBagMatrix.Instance.SetCentral(cXYZ2, ca, DensitySingleton.Instance.FD.PA, atom_offset);
                        ViewBagMatrix.Instance.SetLinear(lXYZ2, la, DensitySingleton.Instance.FD.PA, atom_offset);
                        ViewBagMatrix.Instance.SetPlanar(pXYZ2, pa, DensitySingleton.Instance.FD.PA, atom_offset);
                    }

                    nav_space = ViewBagMatrix.Instance.Width / 25; //we dramatically reduce the pixels for navigation mode.
                    hov_min = -1;
                    hov_max = -1;

                }

                bool recalc = ViewBagMatrix.Instance.Refresh;
                if (recalc || refresh_mode == "F")
                {
                    dm.create_scratch_slice(ViewBagMatrix.Instance.Width, nav_space,
                    ViewBagMatrix.Instance.IsSD, ViewBagMatrix.Instance.SdCap, ViewBagMatrix.Instance.SdFloor,
                    ViewBagMatrix.Instance.CentralPosVector, ViewBagMatrix.Instance.LinearPosVector, ViewBagMatrix.Instance.PlanarPosVector,
                    ViewBagMatrix.Instance.CAtomStrucVector, ViewBagMatrix.Instance.LAtomStrucVector, ViewBagMatrix.Instance.PAtomStrucVector, DensitySingleton.Instance.FD.PA,
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


                ViewBag.cAtom = ViewBagMatrix.Instance.CentralAtomStrucString;
                ViewBag.lAtom = ViewBagMatrix.Instance.LinearAtomStrucString;
                ViewBag.pAtom = ViewBagMatrix.Instance.PlanarAtomStrucString;

                ViewBag.cXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.CentralPosVector.C, 4)) + ")";
                ViewBag.lXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.LinearPosVector.C, 4)) + ")";
                ViewBag.pXYZ = "(" + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.A, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.B, 4)) + "," + Convert.ToString(Math.Round(ViewBagMatrix.Instance.PlanarPosVector.C, 4)) + ")";

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
                ViewBag.ForceSlow = force_slow;

                // visual
                ViewBag.TabBackClr = "Gainsboro";


                ViewBagMatrix.Instance.Reset();

                return View();
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }

        }

        private async Task<bool> loadFDFiles(string pdbcode)
        {
            //Load the density matrix
            DensitySingleton.Instance.FD = new FileDownloads(pdbcode);
            string pdbStatus = await DensitySingleton.Instance.FD.existsPdbMatrixAsync();
            if (pdbStatus == "Y")
            {
                string ccp4Status = await DensitySingleton.Instance.FD.existsCcp4Matrix();
                if (ccp4Status == "Y")
                {

                    DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, "LINEAR", 2, -1);

                    ViewBagMatrix.Instance.EmCode = DensitySingleton.Instance.FD.EmCode;
                    ViewBagMatrix.Instance.DensityType = DensitySingleton.Instance.FD.DensityType;
                    ViewBag.Resolution = DensitySingleton.Instance.FD.Resolution;
                    ViewBagMatrix.Instance.Info = dm.Info;
                    ViewBag.Info = ViewBagMatrix.Instance.Info;
                    ViewBag.DensityType = ViewBagMatrix.Instance.DensityType;
                    return true;
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
            return false;
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
                //Load the density matrix

                //if (DensitySingleton.Instance.needMatrix(ViewBagMatrix.Instance.PdbCode))
                {
                    bool ok = await loadFDFiles(ViewBagMatrix.Instance.PdbCode);
                }
                ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
                ViewBag.EmCode = ViewBagMatrix.Instance.EmCode;
                ViewBag.EbiLink = ViewBagMatrix.Instance.EbiLink;

                return View();
            }
            catch (Exception e)
            {
                ViewBag.Error = "Error from server:" + e.Message;
                return View();
            }
        }

        public async Task<IActionResult> Projection(string pdbcode = "", double dmin = -1, double dmax = -1)
        {
            ViewBag.Error = "";                        
            ViewBagMatrix.Instance.PdbCode = pdbcode;
            
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(ViewBagMatrix.Instance.PdbCode, ViewBagMatrix.Instance.Interp, 2, -1);
            dm.projection();
            dm.atomsProjection(DensitySingleton.Instance.FD.PA);

            //var jSideX = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_X));
            //var jSideY = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_Y));
            //var jSideV = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_V));

            //scatter
            ViewBag.ScatXY_X = dm.ScatXY_X;
            ViewBag.ScatXY_Y = dm.ScatXY_Y;
            ViewBag.ScatXY_V = dm.ScatXY_V;
            ViewBag.ScatYZ_X = dm.ScatYZ_X;
            ViewBag.ScatYZ_Y = dm.ScatYZ_Y;
            ViewBag.ScatYZ_V = dm.ScatYZ_V;
            ViewBag.ScatZX_X = dm.ScatZX_X;
            ViewBag.ScatZX_Y = dm.ScatZX_Y;
            ViewBag.ScatZX_V = dm.ScatZX_V;

            //heatmap
            ViewBag.SideX = dm.SideX;
            ViewBag.SideY = dm.SideY;
            ViewBag.SideZ = dm.SideZ;
            ViewBag.MatXY = dm.MatXY;
            ViewBag.MatYZ = dm.MatYZ;
            ViewBag.MatZX = dm.MatZX;


            ViewBag.PdbCode = ViewBagMatrix.Instance.PdbCode;
            ViewBag.Plane = ViewBagMatrix.Instance.Plane;            
            ViewBag.MinV = dm.DMin;
            ViewBag.MaxV = dm.DMax;
            if (dmin == -1)
            {
                ViewBag.SdFloor = dm.DMin;
                ViewBag.DenMin = dm.DMin;
            }
            else
            {
                ViewBag.SdFloor = dmin;
                ViewBag.DenMin = dmin;
            }
            if (dmax == -1)
            {                
                ViewBag.SdCap = Math.Round(dm.DMax, 2);
                ViewBag.DenMax = Math.Round(dm.DMax,2);
            }
            else
            {
                ViewBag.SdCap = Math.Round(dmax, 2);
                ViewBag.DenMax = Math.Round(dmax,2);
            }
            
            return View();
        }

    }
}
