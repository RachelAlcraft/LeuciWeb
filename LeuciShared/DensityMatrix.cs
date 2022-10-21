using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

/*
 * This pattern allows an asynchronous constructir
 * https://stackoverflow.com/questions/8145479/can-constructors-be-async/31471915#31471915
 */

namespace LeuciShared
{
    public class DensityMatrix
    {
        private const int MAXSIZE = 32;
        private string? _emcode;
        private int _A;
        private int _B;
        private int _C;
        public string Info = "";


        private DensityBinary _densityBinary;
        private Cubelet _cublet;        
        public double[] MatA = new double[0];
        public double[] MatB = new double[0];
        public double[] MatC = new double[0];
        public double[][] MatD = new double[0][];
        public int LayerMax = 0;
        public int Layer = 0;
        public double DMin = 0;
        public double DMax = 0;
        public double LMin = 0;
        public double LMax = 0;


        public double[][]? SliceDensity;
        public double[][]? SliceRadient;
        public double[][]? SliceLaplacian;
        public double[]? SliceAxis;       
        private Interpolator _interpMap;
        private string _interp;

        public static async Task<DensityMatrix> CreateAsync(string pdbcode,string empath,string interp)
        {
            DensityMatrix x = new DensityMatrix();
            await x.InitializeAsync(empath,interp);
            return x;
        }
        private DensityMatrix() { }
        private async Task InitializeAsync(string edFile,string interp)
        {
            //_emcode = emcode;
            //string edFile = "wwwroot/App_Data/" + _emcode + ".ccp4";
            //await DownloadAsync(edFile);
            _densityBinary = new DensityBinary(edFile);
            _A = _densityBinary.Z3_cap;//Convert.ToInt32(_densityBinary.Words["03_NZ"]);
            _B = _densityBinary.Y2_cap;//Convert.ToInt32(_densityBinary.Words["02_NY"]);
            _C = _densityBinary.X1_cap;//Convert.ToInt32(_densityBinary.Words["01_NX"]);
            Info = _densityBinary.Info;            
            _cublet = new Cubelet(_A, _B, _C);
            _interp = interp;
            if (interp == "BSPLINE")
                _interpMap = new BetaSpline(_densityBinary.getShortList(), _C, _B, _A);
            else
                _interpMap = new Nearest(_densityBinary.getShortList(), _C, _B, _A);
            
        }

        /*public async Task<bool> DownloadAsync(string edFile)
        {
            bool haveED = false;            
            if (!File.Exists(edFile))
            {
                string edWebPath = @"https://www.ebi.ac.uk/pdbe/coordinates/files/" + _emcode + ".ccp4";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (var s = await client.GetStreamAsync(edWebPath))
                        {
                            using (var fs = new FileStream(edFile, FileMode.CreateNew))
                            {
                                var tasks = s.CopyToAsync(fs);
                                await Task.WhenAll(tasks);
                                haveED = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        haveED = false;
                    }
                }
            }
            else
            {
                haveED = true;
            }
            return haveED;
        }*/
    
        public void calculatePlane(string plane, int layer)
        {
            //TODO check if it needs to be recalced            
            int[] XY = _cublet.getPlaneDims(plane, layer);
            LayerMax = _cublet.LayerMax;
            //double[] doubles = _densityBinary.makePlane(coords);
            if (layer > LayerMax)
                layer = LayerMax;            
            List<int[]> coords = _cublet.getPlaneCoords(plane, layer);
            Layer = layer;

            double[] doubles = new double[coords.Count];
            for (int i = 0; i < coords.Count; ++i)
            {
                int[] coord = coords[i];
                doubles[i] = _interpMap.getExactValue(coord[0], coord[1], coord[2]);
            }

            MatD = _cublet.makeSquare(doubles, XY);
            MatA = new double[XY[1]];
            MatB = new double[XY[0]];
            MatC = new double[XY[0]*XY[1]];
            int count = 0;
            for (int a=0; a < XY[0]; ++a)
            {
                MatB[a] = a;
                for (int b = 0; b < XY[1]; ++b)
                {
                    MatA[b] = b;
                    MatC[count] = MatD[a][b];                    
                    DMin = Math.Min(DMin, MatC[count]);
                    DMax = Math.Max(DMin, MatC[count]);
                    count++;

                }
            }            
        }

        public void create_slice(double width, double gap,
                                VectorThree central, VectorThree linear, VectorThree planar)            
        {            
            int nums = Convert.ToInt32(width / gap);
            int halfLength = Convert.ToInt32((nums) / 2);
            DMin = 100;
            LMin = 100;
            DMax = -100;
            LMax = -100;

            SpaceTransformation space = new SpaceTransformation(central, linear, planar);

            SliceDensity = new double[nums][];
            SliceRadient = new double[nums][];
            SliceLaplacian = new double[nums][];
            SliceAxis = new double[nums];
            if (_interp == "NEAREST")
            {
                SliceRadient = new double[0][];
                SliceLaplacian = new double[0][];
            }
            if (_interp == "LINEAR")
            {            
                SliceLaplacian = new double[0][];
            }
            


            for (int m = 0; m< nums; ++m)
            {
                int i = m - halfLength;
                List<double> row_d = new List<double>();
                List<double> row_r = new List<double>();
                List<double> row_l = new List<double>();
                SliceAxis[m] = m;

                if (_interp == "NEAREST")
                {
                    SliceDensity[m] = new double[nums];
                }
                else if (_interp == "LINEAR")
                {
                    SliceDensity[m] = new double[nums];
                    SliceRadient[m] = new double[nums];
                }
                else
                {
                    SliceDensity[m] = new double[nums];
                    SliceRadient[m] = new double[nums];
                    SliceLaplacian[m] = new double[nums];
                }

                for (int n = 0; n < nums; ++n)
                {
                    int j = n - halfLength;
                    double x0 = (i * gap);
                    double y0 = (j * gap);
                    double z0 = 0;
                    VectorThree transformed = space.applyTransformation(new VectorThree(x0, y0, z0));
                    VectorThree crs = _densityBinary.getCRSFromXYZ(transformed);
                    if (_densityBinary.AllValid(crs))
                    {
                        double density = _interpMap.getValue(crs.A, crs.B, crs.C);
                        SliceDensity[m][n] = density;
                        DMin = Math.Min(DMin, density);
                        DMax = Math.Max(DMax, density);
                        if (_interp == "BSPLINE" || _interp == "LINEAR")
                        {
                            double radient = _interpMap.getRadient(crs.A, crs.B, crs.C);
                            SliceRadient[m][n] = radient;
                        }
                        if (_interp == "BSPLINE")
                        {
                            double laplacian = _interpMap.getLaplacian(crs.A, crs.B, crs.C);
                            SliceLaplacian[m][n] = laplacian;
                            LMin = Math.Min(LMin, density);
                            LMax = Math.Max(LMax, density);
                        }
                        
                    }
                    else
                    {
                        SliceDensity[m][n] = -1;
                        SliceRadient[m][n] = -1;
                        SliceLaplacian[m][n] = -1;
                    }

                }                
            }            
        }
        
    }
}
