using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
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

        public double DenMin = 0;
        public double DenMax = 0;
        public double ThreeSd = 0;


        public double[][]? SliceDensity;
        public double[][]? SliceRadient;
        public double[][]? SliceLaplacian;
        public double[]? SliceAxis;       
        private Interpolator _interpMap;
        private string _interp;

        public static async Task<DensityMatrix> CreateAsync(string pdbcode,string empath,string interp)
        {
            DensityMatrix x = new DensityMatrix();
            x.InitializeAsync(empath,interp);
            return x;
        }
        private DensityMatrix() { }
        private void InitializeAsync(string edFile,string interp)
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
                _interpMap = new BetaSpline(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C,_B, _A);
            else if (interp == "LINEAR")
                _interpMap = new Linear(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
            else
                _interpMap = new Nearest(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);

        }

        public void changeInterp(string interp)
        {
            _interp = interp;
            if (_interp == "BSPLINE")
                _interpMap = new BetaSpline(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
            else if (_interp == "LINEAR")
                _interpMap = new Linear(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
            else
                _interpMap = new Nearest(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength,_C, _B, _A);
        }        
        private void createData()
        {
            if (!_densityBinary.INIT)
            {
                _densityBinary.Init();
                if (_interp == "BSPLINE")
                    _interpMap = new BetaSpline(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
                else if (_interp == "LINEAR")
                    _interpMap = new Linear(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
                else
                    _interpMap = new Nearest(_densityBinary.Bytes, _densityBinary.Bstart, _densityBinary.Blength, _C, _B, _A);
            }
        }
        private void createNewInterpData(Dictionary<int,double> shorterlist,int x,int y,int z)
        {
            if (!_densityBinary.INIT)            
                _densityBinary.Init();
            if (_interp == "BSPLINE")
                (_interpMap as BetaSpline).makeSubMatrix(0,0,0,0,0,0);
                        
        }
        public void calculatePlane(string plane, int layer)
        {
            createData();
            //TODO check if it needs to be recalced            
            int[] XY = _cublet.getPlaneDims(plane, layer);
            LayerMax = _cublet.LayerMax;            
            if (layer > LayerMax)
                layer = LayerMax;
            Layer = layer;
            
            List<int[]> coords = _cublet.getPlaneCoords3d(plane, layer);
            List<double> doubless = new List<double>();
            for (int i = 0; i < coords.Count; ++i)
            {
                int[] coord = coords[i];
                //doubless.Add(_interpMap.getExactValueMat(coord[0], coord[1], coord[2]));
                doubless.Add(_interpMap.getExactValueBinary(coord[0], coord[1], coord[2]));
            }
            MatD = _cublet.makeSquare(doubless, XY);
                        
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

        public void create_slice(double width, double gap, bool sd,double sdcap,
                                VectorThree central, VectorThree linear, VectorThree planar)            
        {
            createData();
            // we want general info of the max and min given the sd setting
            DenMin = Convert.ToDouble(_densityBinary.Words["20_DMIN"]);
            DenMax = Convert.ToDouble(_densityBinary.Words["21_DMAX"]);
            ThreeSd = _densityBinary.Mean + (sdcap * _densityBinary.Sd);
            if (sd)
            {
                DenMin = (Convert.ToDouble(_densityBinary.Words["20_DMIN"]) - _densityBinary.Mean)/ _densityBinary.Sd;
                DenMax = (Convert.ToDouble(_densityBinary.Words["21_DMAX"]) - _densityBinary.Mean)/ _densityBinary.Sd;
                ThreeSd = sdcap;
            }
            ThreeSd = Math.Round(ThreeSd, 2);

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
                        if (sd)//convert to standard deviations
                        {
                            density = (density - _densityBinary.Mean) / _densityBinary.Sd;
                        }
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
                        if (_interp == "BSPLINE")
                        {
                            SliceRadient[m][n] = -1;
                            SliceLaplacian[m][n] = -1;
                        }
                        else if (_interp == "LINEAR")
                        {
                            SliceRadient[m][n] = -1;
                        }                                                    
                    }

                }                
            }            
        }

        public void create_scratch_slice(double width, double gap, bool sd, double sdcap,
                                VectorThree central, VectorThree linear, VectorThree planar)
        {

            // we want to first build a smaller cube around the centre
            VectorThree crs_centre = _densityBinary.getCRSFromXYZ(central);
            int xmin = (int)Math.Floor(crs_centre.A) - 16;
            int xmax = (int)Math.Floor(crs_centre.A) + 16;
            int ymin = (int)Math.Floor(crs_centre.B) - 16;
            int ymax = (int)Math.Floor(crs_centre.B) + 16;
            int zmin = (int)Math.Floor(crs_centre.C) - 16;
            int zmax = (int)Math.Floor(crs_centre.C) + 16;
            List<int[]> coords = _cublet.getCubeCoords3d(xmin, xmax, ymin, ymax, zmin,zmax);

            //then find the coordinates for it as positions
            List<int> poses = new List<int>();
            foreach (int[] xyz in coords)
            {
                poses.Add(_interpMap.getExactPos(xyz[0], xyz[1], xyz[2]));
            }
            Dictionary<int, double> doubles = _densityBinary.getShorterList(poses);
                        
            //then create a smaller interpolator                        
            createNewInterpData(doubles,32,32,32);
            
            

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



            for (int m = 0; m < nums; ++m)
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
                        if (sd)//convert to standard deviations
                        {
                            density = (density - _densityBinary.Mean) / _densityBinary.Sd;
                        }
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
                        if (_interp == "BSPLINE")
                        {
                            SliceRadient[m][n] = -1;
                            SliceLaplacian[m][n] = -1;
                        }
                        else if (_interp == "LINEAR")
                        {
                            SliceRadient[m][n] = -1;
                        }
                    }

                }
            }

            ////////////// general settings for the view /////////////////////
            // we want general info of the max and min given the sd setting
            DenMin = Convert.ToDouble(_densityBinary.Words["20_DMIN"]);
            DenMax = Convert.ToDouble(_densityBinary.Words["21_DMAX"]);
            ThreeSd = _densityBinary.Mean + (sdcap * _densityBinary.Sd);
            if (sd)
            {
                DenMin = (Convert.ToDouble(_densityBinary.Words["20_DMIN"]) - _densityBinary.Mean) / _densityBinary.Sd;
                DenMax = (Convert.ToDouble(_densityBinary.Words["21_DMAX"]) - _densityBinary.Mean) / _densityBinary.Sd;
                ThreeSd = sdcap;
            }
            ThreeSd = Math.Round(ThreeSd, 2);
        }

    }
}
