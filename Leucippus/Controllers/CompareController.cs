using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;

namespace Leucippus.Controllers
{
    public class CompareController : Controller
    {
        public async Task<IActionResult> Index
        (
            string activetab = "tA",
            string mode = "CALC",
            string pdbcodeA = "1ejg",
            string pdbcodeB = "3nir",
            double width = 6,
            int samples = 100,
            string interp = "LINEAR",
            string matrix = "DENSITY",
            double fosA = 2,
            double fcsA = -1,
            double fosB = 2,
            double fcsB = -1,
            string nav = "",
            string c_xyzA = "", string l_xyzA = "", string p_xyzA = "",string caA = "", string laA = "", string paA = "",
            string c_xyzB = "", string l_xyzB = "", string p_xyzB = "", string caB = "", string laB = "", string paB = "",
            string xAxis = "",
            string yAxis = "",
            string slicedensityA = "",
            string slicedensityB = "",
            string slicedensityD = "",
            double minDA = 0, double minLA = 0, double maxDA = 0, double maxLA = 0,
            double minDB = 0, double minLB = 0, double maxDB = 0, double maxLB = 0,
            double minDD = 0, double minLD = 0, double maxDD = 0, double maxLD = 0,
           // visual elements
           string dfloorA = "100", string dcapA = "100", string hueA = "Best", string cbarA = "Y", string plotA = "heatmap",
           string dfloorB = "100", string dcapB = "100", string hueB = "Best", string cbarB = "Y", string plotB = "heatmap",
           string dfloorD = "100", string dcapD = "100", string hueD = "Best", string cbarD = "Y", string plotD = "heatmap"
        )
        {
            //get any data to display
            ViewBag.PdbCodeA = pdbcodeA;
            ViewBag.PdbCodeB = pdbcodeB;
            ViewBag.Width = width;
            ViewBag.Samples = samples;
            ViewBag.Interp = interp;
            ViewBag.Matrix = matrix;
            ViewBag.FosA = fosA;
            ViewBag.FcsA = fcsA;
            ViewBag.FosB = fosB;
            ViewBag.FcsB = fcsB;

            SinglePosition sliceA = SinglePosition.makeFromFlat(xAxis, yAxis, slicedensityA, slicedensityA, slicedensityA, minDA, maxDA, minLA, maxLA, dfloorA, dcapA, hueA, cbarA,plotA);
            SinglePosition sliceB = SinglePosition.makeFromFlat(xAxis, yAxis, slicedensityB, slicedensityB, slicedensityB, minDB, maxDB, minLB, maxLB, dfloorB, dcapB, hueB, cbarB, plotB);
            SinglePosition sliceD = SinglePosition.makeFromFlat(xAxis, yAxis, slicedensityD, slicedensityD, slicedensityD, minDD, maxDD, minLD, maxLD, dfloorD, dcapD, hueD, cbarD, plotD);

            sliceD.matricesDiff(sliceA, sliceB);
            

            ViewBag.SliceA = sliceA;
            ViewBag.SliceB = sliceB;
            ViewBag.SliceD = sliceD;

            // Tab defaults
            ViewBag.ActiveTab = activetab;
            ViewBag.ColorA = "black";
            ViewBag.ColorB = "black";
            ViewBag.ColorO = "black";
            ViewBag.DisplayA = "none";
            ViewBag.DisplayB = "none";
            ViewBag.DisplayO = "none";
            if (activetab == "tA")
            {
                ViewBag.ColorA = "crimson";
                ViewBag.DisplayA = "block";
            }
            else if (activetab == "tB")
            {
                ViewBag.ColorB = "crimson";
                ViewBag.DisplayB = "block";
            }
            else if (activetab == "tO")
            {
                ViewBag.ColorO = "crimson";
                ViewBag.DisplayO = "block";
            }
            if (nav != "ABC" && nav != "" && nav.Contains(":"))
            {
                string[] navs = nav.Split(":");
                if (navs[0] == "A")
                    applyNav(navs[1], pdbcodeA);
                else
                    applyNav(navs[1], pdbcodeB);
            }
            else if (mode == "CALC")
            {
                await applyRefresh(pdbcodeA, interp, fosA, fcsA, width, samples);
                DensityMatrix dmA = await DensitySingleton.Instance.getMatrix(pdbcodeA, interp, fosA, fcsA, 3);                                
                ViewBag.SliceA = dmA.getSinglePosition();
                ViewBag.SliceA.addVisual(sliceA);

                await applyRefresh(pdbcodeB, interp, fosB, fcsB, width, samples);
                DensityMatrix dmB = await DensitySingleton.Instance.getMatrix(pdbcodeB, interp, fosB, fcsB, 3);
                ViewBag.SliceB = dmB.getSinglePosition();
                ViewBag.SliceB.addVisual(sliceB);

                ViewBag.SliceD = Helper.singlePosDiff(ViewBag.SliceA,ViewBag.SliceB);
                ViewBag.SliceD.addVisual(sliceD);
            }

            

            // return any defaults
            ViewBag.Mode = "CALC";
            ViewBag.Nav = "ABC";
            return View();
        }

        public void applyNav(string nav, string pdbcode)
        {

        }
        public async Task applyRefresh(string pdbcode, string interp, double fos, double fcs, double width, int samples)
        {
            if (DensitySingleton.Instance.needMatrix(pdbcode))
            {
                bool ok = await MatrixHelper.loadFDFiles(pdbcode);                
            }
            DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, interp, fos, fcs, 3);

            string[] first3 = DensitySingleton.Instance.FD.PA.getFirstThreeCoords();
            VectorThree cAtom = DensitySingleton.Instance.FD.PA.getCoords(first3[0]);
            VectorThree lAtom = DensitySingleton.Instance.FD.PA.getCoords(first3[1]);
            VectorThree pAtom = DensitySingleton.Instance.FD.PA.getCoords(first3[2]);


            dm.create_scratch_slice(width, samples, true, 
                    cAtom, lAtom, pAtom,
                    cAtom, lAtom, pAtom,
                    DensitySingleton.Instance.FD.PA, -1, -1
                    );


        }
    }

    
}
