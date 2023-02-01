/*
 * This pattern allows an asynchronous constructir
 * https://stackoverflow.com/questions/8145479/can-constructors-be-async/31471915#31471915
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;

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

        protected DensityBinary _densityBinary;
        private DensityBinary _densityDiffBinary;
        private Cubelet _cublet;
        // For the planes
        public double[] MatA = new double[0];
        public double[] MatB = new double[0];
        public double[] MatC = new double[0];
        public double[][] MatD = new double[0][];        
        public int LayerMax = 0;
        public int Layer = 0;
        // for the projections scatter of atoms
        public double[] ScatA_X = new double[0];
        public double[] ScatA_Y = new double[0];
        public double[] ScatA_Z = new double[0];
        public double[] ScatA_V = new double[0];        
        // for the crs projection of atoms 
        public double[] ScatA_C = new double[0];
        public double[] ScatA_R = new double[0];
        public double[] ScatA_S = new double[0];        
        
        // for the projections heatmap
        public double[][] MatP;
        public double[][] MatQ;
        public double[][] MatR;
        public double[] SideC = new double[0];
        public double[] SideR = new double[0];
        public double[] SideS = new double[0];
        //public double[][] AMatXY = new double[0][];
        //public double[][] AMatYZ = new double[0][];
        //public double[][] AMatZX = new double[0][];


        // for the slider and contours
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
        private int _copies;
        private bool ok_for_whole_spline = true;
        private double _fos;
        private double _fcs;
        private bool _symmetry = true;
        private double _combMean;
        private double _combSd;
        private double _combMin;
        private double _combMax;
        

        public static async Task<DensityMatrix> CreateAsync(string pdbcode, string empath, string diffpath, string interp, double fos, double fcs,bool symmetry, int copies)
        {
            DensityMatrix x = new DensityMatrix();
            x.InitializeAsync(empath, diffpath, interp, fos, fcs,symmetry,copies);
            return x;
        }
        private DensityMatrix() { }
        private void InitializeAsync(string edFile, string difFile, string interp, double fos, double fcs, bool symmetry, int copies)
        {
            //_emcode = emcode;
            //string edFile = "wwwroot/App_Data/" + _emcode + ".ccp4";
            //await DownloadAsync(edFile);
            _fos = fos;
            _fcs = fcs;
            _symmetry = symmetry;
            _copies = copies;

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
            //_A = _densityBinary.Z3_cap;//Convert.ToInt32(_densityBinary.Words["03_NZ"]);
            //_B = _densityBinary.Y2_cap;//Convert.ToInt32(_densityBinary.Words["02_NY"]);
            //_C = _densityBinary.X1_cap;//Convert.ToInt32(_densityBinary.Words["01_NX"]);
            _A = _densityBinary.Z3_cap;
            _B = _densityBinary.Y2_cap;
            _C = _densityBinary.X1_cap;
            Info = _densityBinary.Info;
            ok_for_whole_spline = (_A * _B * _C) < (350 * 350 * 350);
            _cublet = new Cubelet(_A, _B, _C);
            changeInterp(interp, _copies);
            
        }

        public VectorThree getMatrixDims()
        {
            return new VectorThree(_A, _B, _C);
        }

        public VectorThree getXYZFromCRS(VectorThree CRS)
        {
            return _densityBinary.getXYZFromCRS(CRS);
        }

        public VectorThree getCRSFromXYZ(VectorThree XYZ)
        {
            return _densityBinary.getCRSFromXYZ(XYZ);
        }

        public SinglePosition getSinglePosition()
        {
            SinglePosition sliceA = new SinglePosition();
            sliceA.xAxis = this.SliceAxis;
            sliceA.yAxis = this.SliceAxis;
            sliceA.densityMatrix = this.SliceDensity;
            sliceA.radientMatrix = this.SliceRadient;
            sliceA.laplacianMatrix = this.SliceLaplacian;
            sliceA.minD = this.DMin;
            sliceA.maxD = this.DMax;
            sliceA.minL = this.LMin;
            sliceA.maxL = this.LMax;            
            return sliceA;
        }

        public void changeInterp(string interp,int copies)
        {
            // main density is 2Fo-Fc
            // diff density is Fo-Fc
            _copies = copies;
            double m = 0;
            double d = 0;
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
                    fofc[i] = (Single)(m * valueM + d * valueD);
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
            if (_interp == "BSPLINE")
            {
                if (ok_for_whole_spline)
                    _interp = "BSPLINEWHOLE";
                else
                    _interp = "BSPLINE3";
            }
            
            if (_interp == "BSPLINEWHOLE")
                _interpMap = new BetaSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3,_copies,false,false);
            else if (_interp == "LINEAR")
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A, 1,_copies);
            else if (_interp == "CUBIC")
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3, _copies);
            else if (_interp == "BSPLINE3")
                _interpMap = new OptBSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3, 64, 0);
            else
                _interpMap = new Nearest(fofc, 0, _densityBinary.Blength, _C, _B, _A, _copies);

        }
        private void createData()
        {
            if (!_densityBinary.INIT)
            {
                changeInterp(_interp,_copies);
                //projection();
            }
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
            MatC = new double[XY[0] * XY[1]];
            int count = 0;
            for (int a = 0; a < XY[0]; ++a)
            {
                MatB[a] = a;
                for (int b = 0; b < XY[1]; ++b)
                {
                    MatA[b] = b;
                    MatC[count] = MatD[a][b];
                    DMin = Math.Min(DMin, MatC[count]);
                    DMax = Math.Max(DMax, MatC[count]);
                    count++;

                }
            }
        }

        public void xyzProjection(
            PdbAtoms pa,
            bool symmetry,
            out List<double> xProjSide,
            out List<double> yProjSide,
            out List<double> zProjSide,
            out Array xyProjMat,
            out Array yzProjMat,
            out Array zxProjMat            
            )
        {
            createData();
            double gap = 0.25;
            _interpMap.setReflect(symmetry);

            // make the sides dimensions
            xProjSide = new List<double>();
            yProjSide = new List<double>();
            zProjSide = new List<double>();

            for (double x = Math.Floor(pa.LowerCoords.A - 1); x < Math.Ceiling(pa.UpperCoords.A + 1); x+=gap )            
                xProjSide.Add(x);
            for (double y = Math.Floor(pa.LowerCoords.B - 1); y < Math.Ceiling(pa.UpperCoords.B + 1); y += gap)                
                yProjSide.Add(y);
            for (double z = Math.Floor(pa.LowerCoords.C - 1); z < Math.Ceiling(pa.UpperCoords.C + 1); z += gap)
                zProjSide.Add(z);
            
            xyProjMat = Array.CreateInstance(typeof(double), new int[2] { yProjSide.Count, xProjSide.Count });
            yzProjMat = Array.CreateInstance(typeof(double), new int[2] { zProjSide.Count, yProjSide.Count });
            zxProjMat = Array.CreateInstance(typeof(double), new int[2] { xProjSide.Count, zProjSide.Count });
                  
            for (int i = 0; i < xProjSide.Count; ++i)
            {
                double x = xProjSide[i];
                for (int j = 0; j < yProjSide.Count; ++j)
                {
                    double y = yProjSide[j];
                    for (int k = 0; k < zProjSide.Count; ++k)
                    {
                        double z = zProjSide[k];
                        try
                        {
                            VectorThree crs = getCRSFromXYZ(new VectorThree(x, y, z));
                            double density = _interpMap.getValue(crs.A, crs.B, crs.C);
                            double maxX = Math.Max(density, (double)xyProjMat.GetValue(j, i));
                            double maxY = Math.Max(density, (double)yzProjMat.GetValue(k, j));
                            double maxZ = Math.Max(density, (double)zxProjMat.GetValue(i, k));
                            xyProjMat.SetValue(maxX, j, i);
                            yzProjMat.SetValue(maxY, k, j);
                            zxProjMat.SetValue(maxZ, i, k);
                            
                        }
                        catch(Exception e)
                        {

                        }
                    }                    
                }                
            }                        
        }

        public void unitProjection()
        {                        
            createData();
            _interpMap.setReflect(false);            
            int sNum = _cublet.getPlaneDims("XY", 0)[0];
            int rNum = _cublet.getPlaneDims("YZ", 0)[0];
            int cNum = _cublet.getPlaneDims("ZX", 0)[0];
            
            DMin = 1000;
            DMax = -1000;

            // make the sides and dimensions

            // init the first dimension of the matrices
            Array MatCR = Array.CreateInstance(typeof(double), new int[2] { rNum, cNum });
            Array MatRS = Array.CreateInstance(typeof(double), new int[2] { sNum, rNum });
            Array MatSC = Array.CreateInstance(typeof(double), new int[2] { cNum, sNum });
            
            //AMatXY = new double[XY[0]][];
            //AMatYZ = new double[YZ[0]][];
            //AMatZX = new double[ZX[0]][];
            SideC = new double[cNum];
            SideR = new double[rNum];
            SideS = new double[sNum];
            for (int i = 0; i < cNum; i++)
                SideC[i] = i;
            for (int i = 0; i < rNum; i++)
                SideR[i] = i;
            for (int i = 0; i < sNum; i++)
                SideS[i] = i;

            for (int c = 0; c < cNum; ++c)
            {
                for (int r = 0; r < rNum; ++r)
                {
                    for (int s = 0; s < sNum; ++s)
                    {
                        double val = _interpMap.getExactValueBinary(c, r, s);
                        DMin = Math.Min(DMin, val);
                        DMax = Math.Max(DMax, val);
                        try
                        {
                            double maxCR = Math.Max(val, (double)MatCR.GetValue(r, c));
                            double maxRS = Math.Max(val, (double)MatRS.GetValue(s, r));
                            double maxSC = Math.Max(val, (double)MatSC.GetValue(c, s));
                            MatCR.SetValue(maxCR, r, c);
                            MatRS.SetValue(maxRS, s, r);
                            MatSC.SetValue(maxSC, c, s);
                        }
                        catch (Exception e)
                        {
                            string m = e.Message;
                        }
                    }
                }
            }
            MatP = Helper.convertArray(MatCR);
            MatQ = Helper.convertArray(MatRS);
            MatR = Helper.convertArray(MatSC);
        }

        public void asymmetricProjection(PdbAtoms pa,bool symmetry)
        {
            AsymmetricUnit au = new AsymmetricUnit(this, pa);

            createData();
            _interpMap.setReflect(symmetry);
            int XMax = (int)au.NCRS.C + 1;            
            int YMax = (int)au.NCRS.B + 1;
            int ZMax = (int)au.NCRS.A + 1;

            int Xoff = (int)au.lCRS.C;
            int Yoff = (int)au.lCRS.B;
            int Zoff = (int)au.lCRS.A;

            DMin = 1000;
            DMax = -1000;

            // make the sides and dimensions

            // init the dimensions of the matrices with boounds
            //See: "The following code example shows how to create and initialize a multidimensional Array with specified lower bounds."
            //https://learn.microsoft.com/en-us/dotnet/api/system.array.createinstance?view=net-7.0
            
            Array MatXY = Array.CreateInstance(typeof(double), new int[2] { XMax, YMax},  new int[2] { Xoff, Yoff });
            Array MatYZ = Array.CreateInstance(typeof(double), new int[2] { YMax, ZMax }, new int[2] { Yoff, Zoff });
            Array MatZX = Array.CreateInstance(typeof(double), new int[2] { ZMax, XMax }, new int[2] { Zoff, Xoff });
                        
            //AMatXY = new double[XMax][];
            //AMatYZ = new double[YMax][];
            //AMatZX = new double[ZMax][];
            SideC = new double[XMax];
            SideR = new double[YMax];
            SideS = new double[ZMax];
            
            for (int i = 0; i < XMax; ++i)
            {
                SideC[i] = i + Xoff;
                //AMatXY[i] = new double[YMax];
            }
            for (int i = 0; i < YMax; ++i)
            {
                SideR[i] = i + Yoff;
                //AMatYZ[i] = new double[ZMax];
            }
            for (int i = 0; i < ZMax; ++i)
            {
                SideS[i] = i + Zoff;
                //AMatZX[i] = new double[XMax];
            }
                                    
            foreach (var crsmap in au.CrsMapping)
            {

                VectorThree vfrom = new VectorThree(crsmap.Key);
                VectorThree vmap = crsmap.Value;
                double val = _interpMap.getExactValueBinary((int)vmap.A, (int)vmap.B, (int)vmap.C);
                DMin = Math.Min(DMin, val);
                DMax = Math.Max(DMax, val);
                try
                {
                    double maxX = Math.Max(val, (double)MatXY.GetValue((int)vfrom.C, (int)vfrom.B));
                    double maxY = Math.Max(val, (double)MatYZ.GetValue((int)vfrom.B, (int)vfrom.A));
                    double maxZ = Math.Max(val, (double)MatZX.GetValue((int)vfrom.A, (int)vfrom.C));
                    
                    MatXY.SetValue(maxX,(int)vfrom.C, (int)vfrom.B);
                    MatYZ.SetValue(maxY, (int)vfrom.B,(int)vfrom.A);
                    MatZX.SetValue(maxZ, (int)vfrom.A,(int)vfrom.C);                    
                }
                catch (Exception e)
                {
                    string m = e.Message;
                }

            }

            MatP = Helper.convertArray(MatXY);
            MatQ = Helper.convertArray(MatYZ);
            MatR = Helper.convertArray(MatZX);

        }

        public void atomsProjection(PdbAtoms pa, bool symmetry)
        {                        
            List<double> A_X = new List<double>();
            List<double> A_Y = new List<double>();
            List<double> A_Z = new List<double>();
            List<double> A_C = new List<double>();
            List<double> A_R = new List<double>();
            List<double> A_S = new List<double>();
            List<double> A_V = new List<double>();
                        
            createData();
            _interpMap.setReflect(symmetry);
            foreach (var atom in pa.Atoms)
            {
                VectorThree xyz = atom.Value;
                VectorThree crs = _densityBinary.getCRSFromXYZ(xyz);                
                double val = _interpMap.getValue(crs.A, crs.B, crs.C);
                //XY plane
                var indexV = A_V.BinarySearch(val);
                if (indexV < 0) indexV = ~indexV;
                A_V.Insert(indexV, val);
                A_X.Insert(indexV, xyz.A);
                A_Y.Insert(indexV, xyz.B);
                A_Z.Insert(indexV, xyz.C);
                A_C.Insert(indexV, crs.A);
                A_R.Insert(indexV, crs.B);
                A_S.Insert(indexV, crs.C);
            }
            
            ScatA_X = new double[pa.Atoms.Count];
            ScatA_Y = new double[pa.Atoms.Count];
            ScatA_Z = new double[pa.Atoms.Count];
            ScatA_V = new double[pa.Atoms.Count];
            ScatA_C = new double[pa.Atoms.Count];
            ScatA_R = new double[pa.Atoms.Count];
            ScatA_S = new double[pa.Atoms.Count];

            for (int i=0; i < pa.Atoms.Count; ++i)
            {
                ScatA_X[i] = A_X[i];
                ScatA_Y[i] = A_Y[i];
                ScatA_Z[i] = A_Z[i];
                ScatA_V[i] = A_V[i];
                ScatA_C[i] = A_C[i];
                ScatA_R[i] = A_R[i];
                ScatA_S[i] = A_S[i];

            }
            
        }
        public void create_scratch_slice(double width, int samples, bool sd,
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
            double gap = width / samples;
            ////////////////////////////////////////////////////////////////////

            // we want to first build a smaller cube around the centre
            VectorThree crs_centre = _densityBinary.getCRSFromXYZ(central);
            _interpMap.seedCentre(crs_centre, width); // SEED THE CENTRE OF THE INTERPOLATOR  for optimisation
            int xmin = (int)Math.Floor(crs_centre.A) - 16;
            int xmax = (int)Math.Floor(crs_centre.A) + 16;
            int ymin = (int)Math.Floor(crs_centre.B) - 16;
            int ymax = (int)Math.Floor(crs_centre.B) + 16;
            int zmin = (int)Math.Floor(crs_centre.C) - 16;
            int zmax = (int)Math.Floor(crs_centre.C) + 16;
            List<int[]> coords = _cublet.getCubeCoords3d(xmin, xmax, ymin, ymax, zmin, zmax);

            //then find the coordinates for it as positions
            List<int> poses = new List<int>();
            foreach (int[] xyz in coords)
            {
                poses.Add(_interpMap.getExactPos(xyz[0], xyz[1], xyz[2]));
            }

            //Dictionary<int, double> doubles = _densityBinary.getShorterList(poses);                        
            //then create a smaller interpolator                        
            //createNewInterpData(doubles,32,32,32);



            int nums = samples;// Convert.ToInt32(width / gap);
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
                    if (hover_min + hover_max != -2)
                    {
                        // first add to the annotations the XYZ and CRS position of this point
                        Annotations[m][n] += "<br>CRS=" + crs.getKey(0);
                        Annotations[m][n] += "<br>XYZ=" + transformed.getKey(4) + " ";
                    }
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
            _interpMap.unSeedCentre();
        }

    }
}
