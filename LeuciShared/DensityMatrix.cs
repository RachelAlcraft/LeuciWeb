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
        private DensityBinary _densityDiffBinary;
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
        public SpaceTransformation Space;

        public double DenMin = 0;
        public double DenMax = 0;
        
        public double[][]? SliceDensity;
        public double[][]? SliceRadient;
        public double[][]? SliceLaplacian;
        public double[]? SlicePositionX;
        public double[]? SlicePositionY;
        public double[]? SliceProjAtomsX;
        public double[]? SliceProjAtomsY;
        public double[]? SlicePlaneAtomsX;
        public double[]? SlicePlaneAtomsY;
        public double[]? SliceAxis;       
        private Interpolator _interpMap;
        private string _interp;
        private int _fos;
        private int _fcs;

        public static async Task<DensityMatrix> CreateAsync(string pdbcode,string empath,string diffpath,string interp, int fos, int fcs)
        {            
            DensityMatrix x = new DensityMatrix();
            x.InitializeAsync(empath,diffpath,interp,fos,fcs);
            return x;
        }
        private DensityMatrix() { }
        private void InitializeAsync(string edFile,string difFile,string interp, int fos, int fcs)
        {
            //_emcode = emcode;
            //string edFile = "wwwroot/App_Data/" + _emcode + ".ccp4";
            //await DownloadAsync(edFile);
            _fos = fos;
            _fcs = fcs;
            _densityBinary = new DensityBinary(edFile);
            _densityDiffBinary = new DensityBinary(difFile);
            _A = _densityBinary.Z3_cap;//Convert.ToInt32(_densityBinary.Words["03_NZ"]);
            _B = _densityBinary.Y2_cap;//Convert.ToInt32(_densityBinary.Words["02_NY"]);
            _C = _densityBinary.X1_cap;//Convert.ToInt32(_densityBinary.Words["01_NX"]);
            Info = _densityBinary.Info;            
            _cublet = new Cubelet(_A, _B, _C);            
            changeInterp(interp);            
        }

        public void changeInterp(string interp)
        {
            // main density is 2Fo-Fc
            // diff density is Fo-Fc
            int m = 0;
            int d = 0;
            m = _fos;
            d = -1 * _fos;
            m += _fcs;
            d += -2 * _fcs;

            Single[] fofc = new Single[_densityBinary.Blength];            
            for (int i = 0; i < _densityBinary.Blength; ++i)
            {
                Single valueM = BitConverter.ToSingle(_densityBinary.Bytes, _densityBinary.Bstart + i * 4);
                Single valueD = BitConverter.ToSingle(_densityDiffBinary.Bytes, _densityBinary.Bstart + i * 4);                
                fofc[i] = m * valueM + d * valueD;
            }

            _interp = interp;
            if (_interp == "BSPLINEWHOLE")
                _interpMap = new BetaSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A,3);
            else if (_interp == "LINEAR")                
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A, 1);
            else if (_interp == "CUBIC")
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A,3);                
            else if (_interp == "BSPLINE3")                
                _interpMap = new OptBSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3, 64);
            else
                _interpMap = new Nearest(fofc, 0, _densityBinary.Blength,_C, _B, _A);
        }        
        private void createData()
        {
            if (!_densityBinary.INIT)            
                changeInterp(_interp);                            
        }
        private void createNewInterpData(Dictionary<int,double> shorterlist,int x,int y,int z)
        {
            if (!_densityBinary.INIT)            
                _densityBinary.Init();
            //if (_interp == "BSPLINE")
            //    (_interpMap as BetaSpline).makeSubMatrix(0,0,0,0,0,0);
                        
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
        public void create_scratch_slice(double width, double gap, bool sd, double sdcap, double sdfloor,
                                VectorThree central, VectorThree linear, VectorThree planar,
                                VectorThree acentral, VectorThree alinear, VectorThree aplanar)
        {
            ////////////// general settings for the view /////////////////////
            // we want general info of the max and min given the sd setting
            DenMin = Convert.ToDouble(_densityBinary.Words["20_DMIN"]);
            DenMax = Convert.ToDouble(_densityBinary.Words["21_DMAX"]);
            //ThreeSd = _densityBinary.Mean + (sdcap * _densityBinary.Sd);
            if (sd)
            {
                DenMin = (Convert.ToDouble(_densityBinary.Words["20_DMIN"]) - _densityBinary.Mean) / _densityBinary.Sd;
                DenMax = (Convert.ToDouble(_densityBinary.Words["21_DMAX"]) - _densityBinary.Mean) / _densityBinary.Sd;
                //ThreeSd = sdcap;
                //ThreeSdMin = sdfloor;
            }
            //ThreeSd = Math.Round(ThreeSd, 2);
            //ThreeSdMin = Math.Round(ThreeSdMin, 2);
            ////////////////////////////////////////////////////////////////////

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
            
            //Dictionary<int, double> doubles = _densityBinary.getShorterList(poses);                        
            //then create a smaller interpolator                        
            //createNewInterpData(doubles,32,32,32);
            
            

            int nums = Convert.ToInt32(width / gap);
            int halfLength = Convert.ToInt32((nums) / 2);
            DMin = 100;
            LMin = 100;
            DMax = -100;
            LMax = -100;

            Space = new SpaceTransformation(central, linear, planar);
                        
            VectorThree posC = Space.reverseTransformation(central);
            VectorThree posL = Space.reverseTransformation(linear);
            VectorThree posP = Space.reverseTransformation(planar);
            VectorThree posCp = posC.getPointPosition(gap, width);
            VectorThree posLp = posL.getPointPosition(gap, width);
            VectorThree posPp = posP.getPointPosition(gap, width);

            VectorThree aposC = Space.reverseTransformation(acentral);
            VectorThree aposL = Space.reverseTransformation(alinear);
            VectorThree aposP = Space.reverseTransformation(aplanar);
            VectorThree aposCp = aposC.getPointPosition(gap, width);
            VectorThree aposLp = aposL.getPointPosition(gap, width);
            VectorThree aposPp = aposP.getPointPosition(gap, width);

            List<VectorThree> lSlicePosition = new List<VectorThree>(); // central linear and planar            
            List<VectorThree> lSlicePositionA = new List<VectorThree>(); // for off plane
            List<VectorThree> lSlicePositionP = new List<VectorThree>(); // for on plane

            if (posCp.A < nums && posCp.B < nums)            
                lSlicePosition.Add(posCp);                            
            if (posLp.A < nums && posLp.B < nums)            
                lSlicePosition.Add(posLp);                            
            if (posPp.A < nums && posPp.B < nums)            
                lSlicePosition.Add(posPp);

            // the atoms may be on of off plane
            if (aposCp.A < nums && aposCp.B < nums)
            {
                if (Math.Abs(aposCp.C) < 0.01)
                    lSlicePositionP.Add(aposCp);
                else
                    lSlicePositionA.Add(aposCp);
            }
            if (aposLp.A < nums && aposLp.B < nums)
            {
                if (Math.Abs(aposLp.C) < 0.01)
                    lSlicePositionP.Add(aposLp);
                else
                    lSlicePositionA.Add(aposLp);
            }
            if (aposPp.A < nums && aposPp.B < nums)
            {
                if (Math.Abs(aposPp.C) < 0.01)
                    lSlicePositionP.Add(aposPp);
                else
                    lSlicePositionA.Add(aposPp);
            }

            SlicePositionX = new double[lSlicePosition.Count];
            SlicePositionY = new double[lSlicePosition.Count];
            for (int i = 0; i < lSlicePosition.Count; i++)
            {
                SlicePositionX[i] = lSlicePosition[i].A;
                SlicePositionY[i] = lSlicePosition[i].B;
            }

            SliceProjAtomsX = new double[lSlicePositionA.Count];
            SliceProjAtomsY = new double[lSlicePositionA.Count];
            for (int i = 0; i < lSlicePositionA.Count; i++)
            {
                SliceProjAtomsX[i] = lSlicePositionA[i].A;
                SliceProjAtomsY[i] = lSlicePositionA[i].B;
            }
            SlicePlaneAtomsX = new double[lSlicePositionP.Count];
            SlicePlaneAtomsY = new double[lSlicePositionP.Count];
            for (int i = 0; i < lSlicePositionP.Count; i++)
            {
                SlicePlaneAtomsX[i] = lSlicePositionP[i].A;
                SlicePlaneAtomsY[i] = lSlicePositionP[i].B;
            }

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
                    VectorThree transformed = Space.applyTransformation(new VectorThree(x0, y0, z0));
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
                        if (_interp.Contains("BSPLINE") || _interp == "LINEAR" || _interp == "CUBIC")
                        {
                            double radient = _interpMap.getRadient(crs.A, crs.B, crs.C);
                            SliceRadient[m][n] = radient;
                        }
                        if (_interp.Contains("BSPLINE") || _interp == "CUBIC")
                        {
                            double laplacian = _interpMap.getLaplacian(crs.A, crs.B, crs.C);
                            SliceLaplacian[m][n] = laplacian;
                            LMin = Math.Min(LMin, laplacian);
                            LMax = Math.Max(LMax, laplacian);
                        }

                    }
                    else
                    {
                        SliceDensity[m][n] = -1;                        
                        if (_interp.Contains("BSPLINE") || _interp == "CUBIC")
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

    }
}
