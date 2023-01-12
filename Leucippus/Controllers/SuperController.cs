using Leucippus.Models;
using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection.PortableExecutable;

namespace Leucippus.Controllers
{
    public class SuperController : Controller
    {       
        public async Task<IActionResult> Index(            
            string update = "N",
            string motif = "{atom:C}{atom:CA,offset:0}{atom:O,offset:0}",
            string pdbcodes = "6eex",                 
            int fos = 2,
            int fcs = -1,
            double width = 6, 
            int samples = 100,
            string interp = "LINEAR",            
            string exclusions = "A:1,A:2",
            string inclusions = null)
        {
            ViewBag.Error = "";
            //ViewBag.Matches = "";
            //ViewBag.Lines = "";
            //ViewBag.Distances = "";

            ViewBag.DensitySlices = new List<double[][]>();
            ViewBag.DensityNames = new List<string>();
            ViewBag.SinglePositions = new List<SinglePosition>();
            ViewBag.SingleMatches = new List<SingleMatches>();

            SinglePosition superP = new SinglePosition();
                        
            string[] pdbcodelist = pdbcodes.Split(",");
            List<string[]> match_motif = new List<string[]>();
            if (update.Contains("Y"))
            {                
                foreach (var pdbcode in pdbcodelist)
                {
                    bool ok = await DensitySingleton.Instance.loadFDFiles(pdbcode);
                    DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, interp, fos, fcs, 2,true);
                    List<Atom[]> atoms_motif;
                    List<double[]> dis_motif;
                    List<VectorThree[]> coords_motif;
                    match_motif = DensitySingleton.Instance.FD.PA.getMatchesMotif(motif, out dis_motif,out atoms_motif);
                    for (int i = 0; i < match_motif.Count; ++i)
                    {
                        string[] mms = match_motif[i];
                        Atom[] ats = atoms_motif[i];
                        double[] diss = dis_motif[i];
                        SingleMatches sms = new SingleMatches();
                        for (int j = 0; j < ats.Length; ++j)
                        {
                            SingleMatch sm = new SingleMatch(pdbcode, ats[j].Line, Convert.ToString(Math.Round(diss[j],4)), mms[j]);
                            sms.singleMatches.Add(sm);
                        }
                        ViewBag.SingleMatches.Add(sms);

                        
                        //foreach (var m in mm)
                        //    ViewBag.Matches += m + "\n";
                        //ViewBag.Matches += "\n";
                    }

                    /*foreach (var al in atoms_motif)
                    {
                        foreach (var a in al)
                            ViewBag.Lines += pdbcode + ":" + a.Line + "\n";
                        ViewBag.Lines += "\n";
                    }

                    foreach (var dd in dis_motif)
                    {
                        foreach (var d in dd)
                            ViewBag.Distances += Convert.ToString(Math.Round(d, 4)) + "\n";
                        ViewBag.Distances += "\n";
                    }*/
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
                    //List<VectorThree[]> match_coords = new List<VectorThree[]>();
                    List<Atom[]> match_atoms = new List<Atom[]>();
                    List<double[]> match_disses = new List<double[]>();
                    match_motif = DensitySingleton.Instance.FD.PA.getMatchesMotif(motif, out match_disses,out match_atoms);
                    //match_motif = DensitySingleton.Instance.FD.PA.getMatchMotif(motif, exclusions, inclusions, out match_coords, out lines);
                    
                    
                    

                    // create each matrix
                    foreach (Atom[] atoms in match_atoms) //we are taking the first 3 as being central linear planar
                    {
                        VectorThree coordC = atoms[0].coords();
                        VectorThree coordL = atoms[1].coords();
                        VectorThree coordP = atoms[2].coords();
                        
                        SinglePosition singleP = new SinglePosition();
                        dm.create_scratch_slice(width, samples,
                            true, -1, -1,
                            coordC, coordL, coordP,
                            coordC, coordL, coordP,
                            DensitySingleton.Instance.FD.PA, -1, -1);

                        ViewBag.DensitySlices.Add(dm.SliceDensity);
                        ViewBag.DensityNames.Add("Density" + Convert.ToString(ViewBag.DensitySlices.Count));                        
                        // The superpositions have the same axis
                        superP.xAxis = dm.SliceAxis;
                        superP.yAxis = dm.SliceAxis;

                        // the single positions are saved individually
                        singleP.description = pdbcode + " Central=" + atoms[0].Summary + " Linear=" + atoms[1].Summary + " Planar=" + atoms[2].Summary;
                        singleP.xAxis = dm.SliceAxis;
                        singleP.yAxis = dm.SliceAxis;
                        singleP.copyDensity(dm.SliceDensity);
                        singleP.copyRadient(dm.SliceRadient);
                        singleP.copyLaplacian(dm.SliceLaplacian);                        
                        singleP.minD = dm.DMin;
                        singleP.maxD = dm.DMax;
                        singleP.minL = dm.LMin;
                        singleP.maxL = dm.LMax;
                        singleP.contourDen = "SinglePosition" + Convert.ToString(ViewBag.SinglePositions.Count+1) + "DEN";
                        singleP.contourRad = "SinglePosition" + Convert.ToString(ViewBag.SinglePositions.Count + 1) + "RAD";
                        singleP.contourLap = "SinglePosition" + Convert.ToString(ViewBag.SinglePositions.Count + 1) + "LAP";
                        ViewBag.SinglePositions.Add(singleP);

                        if (sliceDensity == null)
                        {
                            sliceDensity = dm.SliceDensity;
                            sliceRadient = dm.SliceRadient;
                            sliceLaplacian = dm.SliceLaplacian;
                            minV = dm.DMin;
                            maxV = dm.DMax;
                            minL = dm.LMin;
                            maxL = dm.LMax;
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
                //ViewBag.SliceDensity = sliceDensity;
                //ViewBag.SliceRadient = sliceRadient;
                //ViewBag.SliceLaplacian = sliceLaplacian;

                //ViewBag.MinV = minV;
                //ViewBag.MaxV = maxV;
                //ViewBag.MinL = minL;
                //ViewBag.MaxL = maxL;
                superP.densityMatrix = sliceDensity;
                superP.radientMatrix = sliceRadient;
                superP.laplacianMatrix = sliceLaplacian;
                superP.minD = minV;
                superP.maxD = maxV;
                superP.minL = minL;
                superP.maxL = maxL;
                superP.contourDen = "SuperPositionDEN";
                superP.contourRad = "SuperPositionRAD";
                superP.contourLap = "SuperPositionLAP";
                ViewBag.SuperPosition = superP;
            }

            // View items            
            ViewBag.PdbCodes = pdbcodes;
            ViewBag.Motif = motif;
            ViewBag.Exclusions = exclusions;
            ViewBag.Inclusions = inclusions;
            ViewBag.Width = width;
            ViewBag.Samples = samples;
            ViewBag.Fos = fos;
            ViewBag.Fcs = fcs;
            ViewBag.Interp = interp;

            return View();
        }
    }
        
}
