using LeuciShared;
using Microsoft.AspNetCore.Mvc;

namespace Leucippus.Controllers
{
    public class SuperController : Controller
    {       
        public async Task<IActionResult> Index(
            string pdbcode = "",
            string update = "N",
            int fos = 2,
            int fcs = -1,
            string interp = "LINEAR",
            string motif = "C:CA:O",
            string exclusions = "A:1,A:2",
            string inclusions = null)
        {
            ViewBag.Error = "";
            ViewBag.Matches = new List<string>();
            ViewBag.Lines = "";
            if (update == "Y")
            {
                DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, interp, fos, fcs,2);
                List<VectorThree[]> match_coords = new List<VectorThree[]>();
                List<string> lines = new List<string>();
                List<string[]> match_motif = DensitySingleton.Instance.FD.PA.getMatchMotif(motif, exclusions, inclusions, out match_coords, out lines);

                double[][]? sliceDensity = null;
                double[][]? sliceRadient = null;
                double[][]? sliceLaplacian = null;

                double minV = 1000;
                double maxV = -1000;
                double minL = 1000;
                double maxL = -1000;

                // create each matrix
                foreach (VectorThree[] coord in match_coords)
                {
                    dm.create_scratch_slice(5, 20,
                        true, -1, -1,
                        coord[0], coord[1], coord[2],
                        coord[0], coord[1], coord[2],
                        DensitySingleton.Instance.FD.PA, -1, -1);

                    if (sliceDensity == null)
                    {
                        sliceDensity = dm.SliceDensity;
                        sliceRadient = dm.SliceRadient;
                        sliceLaplacian = dm.SliceLaplacian;
                    }
                    else
                    {
                        for (int i = 0; i < sliceDensity.Length; i++)
                        {
                            for (int j = 0; j < sliceDensity[i].Length; j++)
                            {
                                sliceDensity[i][j] += dm.SliceDensity[i][j];
                                maxV = Math.Max(maxV, sliceDensity[i][j]);
                                minV = Math.Min(minV, sliceDensity[i][j]);

                                if (sliceRadient.Length == sliceDensity.Length)
                                    sliceRadient[i][j] += dm.SliceRadient[i][j];

                                if (sliceLaplacian.Length == sliceDensity.Length)
                                {
                                    sliceLaplacian[i][j] += dm.SliceLaplacian[i][j];
                                    maxL = Math.Max(maxL, sliceLaplacian[i][j]);
                                    minL = Math.Min(minL, sliceLaplacian[i][j]);
                                }
                            }
                        }
                    }
                }

                // matrix
                ViewBag.SliceDensity = sliceDensity;
                ViewBag.SliceRadient = sliceRadient;
                ViewBag.SliceLaplacian = sliceLaplacian;
                ViewBag.MinV = minV;
                ViewBag.MaxV = maxV;
                ViewBag.MinL = minL;
                ViewBag.MaxL = maxL;
                ViewBag.Matches = match_motif;
                foreach (var ln in lines)
                {
                    ViewBag.Lines += ln + "\n";
                }
            }

            // View items
            ViewBag.PdbCode = pdbcode;
            ViewBag.Motif = motif;
            ViewBag.Exclusions = exclusions;
            ViewBag.Inclusions = inclusions;
            ViewBag.Fos = fos;
            ViewBag.Fcs = fcs;
            ViewBag.Interp = interp;

            return View();
        }
    }
        
}
