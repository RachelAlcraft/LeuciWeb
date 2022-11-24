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

        public string[][]? Annotations;
        public double[][]? SliceDensity;
        public double[][]? SliceRadient;
        public double[][]? SliceLaplacian;
        public double[]? SlicePositionX;
        public double[]? SlicePositionY;
        public double[]? SliceProjGreenAtomsX;
        public double[]? SliceProjGreenAtomsY;
        public double[]? SliceProjBlueAtomsX;
        public double[]? SliceProjBlueAtomsY;
        public double[]? SlicePlaneAtomsX;
        public double[]? SlicePlaneAtomsY;
        public double[]? SliceAxis;       
        private Interpolator _interpMap;
        private string _interp;
        private int _fos;
        private int _fcs;
        private double _combMean;
        private double _combSd;
        private double _combMin;
        private double _combMax;

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
            if (difFile != "")
            {
                // for non cryo-em, the max, min, mean and sd are dependent on the fo and fc values
                _densityDiffBinary = new DensityBinary(difFile);                
            }
            else
            {
                _densityDiffBinary = null;                
            }
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
            _combMax = -1000;
            _combMin = 1000;
            for (int i = 0; i < _densityBinary.Blength; ++i)
            {
                if (_densityDiffBinary != null)
                {
                    Single valueM = BitConverter.ToSingle(_densityBinary.Bytes, _densityBinary.Bstart + i * 4);
                    Single valueD = BitConverter.ToSingle(_densityDiffBinary.Bytes, _densityBinary.Bstart + i * 4);
                    fofc[i] = m * valueM + d * valueD;
                    _combMax = Math.Max(fofc[i], _combMax);
                    _combMin = Math.Min(fofc[i], _combMin);
                }
                else
                {
                    Single valueM = BitConverter.ToSingle(_densityBinary.Bytes, _densityBinary.Bstart + i * 4);
                    fofc[i] = valueM; // the fo and fc numebr are irrlevant for cryo-em                    
                    _combMax = Math.Max(fofc[i], _combMax);
                    _combMin = Math.Min(fofc[i], _combMin);
                }
            }

            _combMean = fofc.Average();
            double sum = fofc.Sum(d => Math.Pow(d - _combMean, 2));
            _combSd = Math.Sqrt((sum) / (fofc.Count() - 1));

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
                                VectorThree acentral, VectorThree alinear, VectorThree aplanar,
                                PdbAtoms PA, double hover_min, double hover_max)
        {
            ////////////// general settings for the view /////////////////////
            // we want general info of the max and min given the sd setting
            DenMin = _combMin;
            DenMax = _combMax;            
            if (sd)
            {
                DenMin = (_combMin - _combMean) / _combSd;
                DenMax = (_combMax - _combMean) / _combSd;                
            }            
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
            List<VectorThree> lSlicePositionAG = new List<VectorThree>(); // for off plane - behind (green)
            List<VectorThree> lSlicePositionAB = new List<VectorThree>(); // for off plane - in front (blue)
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
                else if (aposCp.C > 0)
                    lSlicePositionAG.Add(aposCp);
                else
                    lSlicePositionAB.Add(aposCp);
            }
            if (aposLp.A < nums && aposLp.B < nums)
            {
                if (Math.Abs(aposLp.C) < 0.01)
                    lSlicePositionP.Add(aposLp);
                else if (aposLp.C > 0)
                    lSlicePositionAG.Add(aposLp);
                else
                    lSlicePositionAB.Add(aposLp);
            }
            if (aposPp.A < nums && aposPp.B < nums)
            {
                if (Math.Abs(aposPp.C) < 0.01)
                    lSlicePositionP.Add(aposPp);
                else if (aposPp.C > 0)
                    lSlicePositionAG.Add(aposPp);
                else
                    lSlicePositionAB.Add(aposPp);
            }

            SlicePositionX = new double[lSlicePosition.Count];
            SlicePositionY = new double[lSlicePosition.Count];
            for (int i = 0; i < lSlicePosition.Count; i++)
            {
                SlicePositionX[i] = lSlicePosition[i].A;
                SlicePositionY[i] = lSlicePosition[i].B;
            }

            SliceProjGreenAtomsX = new double[lSlicePositionAG.Count];
            SliceProjGreenAtomsY = new double[lSlicePositionAG.Count];
            for (int i = 0; i < lSlicePositionAG.Count; i++)
            {
                SliceProjGreenAtomsX[i] = lSlicePositionAG[i].A;
                SliceProjGreenAtomsY[i] = lSlicePositionAG[i].B;
            }
            SliceProjBlueAtomsX = new double[lSlicePositionAB.Count];
            SliceProjBlueAtomsY = new double[lSlicePositionAB.Count];
            for (int i = 0; i < lSlicePositionAB.Count; i++)
            {
                SliceProjBlueAtomsX[i] = lSlicePositionAB[i].A;
                SliceProjBlueAtomsY[i] = lSlicePositionAB[i].B;
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
            Annotations = new string[nums][];
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
                    Annotations[m] = new string[nums];
                }
                else if (_interp == "LINEAR")
                {
                    SliceDensity[m] = new double[nums];                    
                    SliceRadient[m] = new double[nums];
                    Annotations[m] = new string[nums];
                }
                else
                {
                    SliceDensity[m] = new double[nums];                    
                    SliceRadient[m] = new double[nums];
                    SliceLaplacian[m] = new double[nums];
                    Annotations[m] = new string[nums];
                }

                for (int n = 0; n < nums; ++n)
                {                    
                    int j = n - halfLength;
                    double x0 = (i * gap);
                    double y0 = (j * gap);
                    double z0 = 0;
                    VectorThree transformed = Space.applyTransformation(new VectorThree(x0, y0, z0));
                    VectorThree crs = _densityBinary.getCRSFromXYZ(transformed);
                    List<string> atom_names = PA.getNearAtoms(transformed, hover_min, hover_max);
                    Annotations[m][n] = "";
                    foreach (string an in atom_names)                    
                        Annotations[m][n] += "<br>" + an;
                    

                    if (_densityBinary.AllValid(crs))
                    {
                        double density = _interpMap.getValue(crs.A, crs.B, crs.C);
                        if (sd)//convert to standard deviations
                        {
                            density = (density - _combMean) / _combSd;
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
