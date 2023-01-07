using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;

namespace Leucippus.Controllers
{
    public class SuperController : Controller
    {       
        public async Task<IActionResult> Index(            
            string update = "N",
            string motif = "{atom:C}{atom:CA,offset:0}{atom:O,offset:0}",
            string pdbcodes = "1ejg,3nir",            
            int fos = 2,
            int fcs = -1,
            string interp = "LINEAR",            
            string exclusions = "A:1,A:2",
            string inclusions = null)
        {
            ViewBag.Error = "";
            ViewBag.Matches = "";
            ViewBag.Lines = "";
            ViewBag.Distances = "";

            ViewBag.DensitySlices = new List<double[][]>();
            ViewBag.DensityNames = new List<string>();


            string[] pdbcodelist = pdbcodes.Split(",");
            List<string[]> match_motif = new List<string[]>();
            if (update.Contains("Y"))
            {                
                foreach (var pdbcode in pdbcodelist)
                {
                    bool ok = await DensitySingleton.Instance.loadFDFiles(pdbcode);
                    DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, interp, fos, fcs, 2,true);
                    List<string[]> lines_motif;
                    List<double[]> dis_motif;
                    List<VectorThree[]> coords_motif;
                    match_motif = DensitySingleton.Instance.FD.PA.getMatchesMotif(motif, out lines_motif, out dis_motif,out coords_motif);
                    foreach (var mm in match_motif)
                    {
                        foreach (var m in mm)
                            ViewBag.Matches += m + "\n";
                        ViewBag.Matches += "\n";
                    }

                    foreach (var ll in lines_motif)
                    {
                        foreach (var l in ll)
                            ViewBag.Lines += pdbcode + ":" + l + "\n";
                        ViewBag.Lines += "\n";
                    }

                    foreach (var dd in dis_motif)
                    {
                        foreach (var d in dd)
                            ViewBag.Distances += Convert.ToString(Math.Round(d, 4)) + "\n";
                        ViewBag.Distances += "\n";
                    }
                }
            }
            if (update.Contains("C"))
            {
                double[][]? sliceDensity = null;
                double[][]? sliceRadient = null;
                double[][]? sliceLaplacian = null;

                double minV = 1000;
                double maxV = -1000;
                double minL = 1000;
                double maxL = -1000;

                foreach (var pdbcode in pdbcodelist)
                {
                    bool ok = await DensitySingleton.Instance.loadFDFiles(pdbcode);
                    DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, interp, fos, fcs,2);
                    List<VectorThree[]> match_coords = new List<VectorThree[]>();
                    List<string[]> xlines = new List<string[]>();
                    List<double[]> xdisses = new List<double[]>();                    
                    match_motif = DensitySingleton.Instance.FD.PA.getMatchesMotif(motif, out xlines, out xdisses, out match_coords);
                    //match_motif = DensitySingleton.Instance.FD.PA.getMatchMotif(motif, exclusions, inclusions, out match_coords, out lines);
                    
                    
                    

                    // create each matrix
                    foreach (VectorThree[] coord in match_coords) //we are taking the first 3 as being central linear planar
                    {
                        dm.create_scratch_slice(5, 20,
                            true, -1, -1,
                            coord[0], coord[1], coord[2],
                            coord[0], coord[1], coord[2],
                            DensitySingleton.Instance.FD.PA, -1, -1);

                        ViewBag.DensitySlices.Add(dm.SliceDensity);
                        ViewBag.DensityNames.Add("Density" + Convert.ToString(ViewBag.DensitySlices.Count));

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
                }
                // matrix
                ViewBag.SliceDensity = sliceDensity;
                ViewBag.SliceRadient = sliceRadient;
                ViewBag.SliceLaplacian = sliceLaplacian;
                ViewBag.MinV = minV;
                ViewBag.MaxV = maxV;
                ViewBag.MinL = minL;
                ViewBag.MaxL = maxL;                                
            }

            // View items            
            ViewBag.PdbCodes = pdbcodes;
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
