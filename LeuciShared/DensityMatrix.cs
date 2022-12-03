/*
 * This pattern allows an asynchronous constructir
 * https://stackoverflow.com/questions/8145479/can-constructors-be-async/31471915#31471915
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;

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
        // For the planes
        public double[] MatA = new double[0];
        public double[] MatB = new double[0];
        public double[] MatC = new double[0];
        public double[][] MatD = new double[0][];        
        public int LayerMax = 0;
        public int Layer = 0;
        // for the projections scatter of atoms
        public double[] ScatXY_X = new double[0];
        public double[] ScatXY_Y = new double[0];
        public double[] ScatXY_V = new double[0];
        public double[] ScatYZ_X = new double[0];
        public double[] ScatYZ_Y = new double[0];
        public double[] ScatYZ_V = new double[0];
        public double[] ScatZX_X = new double[0];
        public double[] ScatZX_Y = new double[0];
        public double[] ScatZX_V = new double[0];
        // for the projections heatmap
        public double[][] MatXY = new double[0][];
        public double[][] MatYZ = new double[0][];
        public double[][] MatZX = new double[0][];
        public double[] SideX = new double[0];
        public double[] SideY = new double[0];
        public double[] SideZ = new double[0];


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
        private int _fos;
        private int _fcs;
        private double _combMean;
        private double _combSd;
        private double _combMin;
        private double _combMax;

        public static async Task<DensityMatrix> CreateAsync(string pdbcode, string empath, string diffpath, string interp, int fos, int fcs)
        {
            DensityMatrix x = new DensityMatrix();
            x.InitializeAsync(empath, diffpath, interp, fos, fcs);
            return x;
        }
        private DensityMatrix() { }
        private void InitializeAsync(string edFile, string difFile, string interp, int fos, int fcs)
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
                _interpMap = new BetaSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3);
            else if (_interp == "LINEAR")
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A, 1);
            else if (_interp == "CUBIC")
                _interpMap = new Multivariate(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3);
            else if (_interp == "BSPLINE3")
                _interpMap = new OptBSpline(fofc, 0, _densityBinary.Blength, _C, _B, _A, 3, 64);
            else
                _interpMap = new Nearest(fofc, 0, _densityBinary.Blength, _C, _B, _A);
        }
        private void createData()
        {
            if (!_densityBinary.INIT)
            {
                changeInterp(_interp);
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

        public void projection()
        {            
            //var jSideX = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_X));
            //var jSideY = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_Y));
            //var jSideV = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_V));

            createData();            
            int[] XY = _cublet.getPlaneDims("XY", 0);
            int XYMax = _cublet.LayerMax;
            int[] YZ = _cublet.getPlaneDims("YZ", 0);
            int YZMax = _cublet.LayerMax;
            int[] ZX = _cublet.getPlaneDims("ZX", 0);
            int ZXMax = _cublet.LayerMax;

            List<int[]> coordssXY = _cublet.getPlaneCoords3d("XY", 0);
            List<int[]> coordssYZ = _cublet.getPlaneCoords3d("YZ", 0);
            List<int[]> coordssZX = _cublet.getPlaneCoords3d("ZX", 0);
            DMin = 1000;
            DMax = -1000;
            
            int lenXY = XY[0] * YZ[0];
            int lenYZ = YZ[0] * ZX[0];
            int lenZX = ZX[0] * XY[0];

            // make the sides and dimensions

            // init the first dimension of the matrices
            MatXY = new double[XY[0]][];
            MatYZ = new double[YZ[0]][];
            MatZX = new double[ZX[0]][];
            SideX = new double[XY[0]];
            SideY = new double[YZ[0]];
            SideZ = new double[ZX[0]];            
            for (int i = 0; i < XY[0]; ++i)
            {
                SideX[i] = i;
                MatXY[i] = new double[XY[1]];
            }
            for (int i = 0; i < YZ[0]; ++i)
            {
                SideY[i] = i;
                MatYZ[i] = new double[YZ[1]];
            }
            for (int i = 0; i < ZX[0]; ++i)
            {
                SideZ[i] = i;
                MatZX[i] = new double[ZX[1]];
            }
                        
            //if (plane == "XY") coords.Add(new int[] { layer, y, x });            
            //else if (plane == "YZ") coords.Add(new int[] { y, x, layer });            
            //else if (plane == "ZX") coords.Add(new int[] { x, layer, y });
            
            for (int l = 0; l < YZMax; ++l)
            {
                List<int[]> coords = _cublet.getPlaneCoords3d("YZ", l);
                for (int i = 0; i < coords.Count; ++i)
                {
                    int[] coord = coords[i];
                    double val = _interpMap.getExactValueBinary(coord[0], coord[1], coord[2]);
                    DMin = Math.Min(DMin, val);
                    DMax = Math.Max(DMax, val);
                    try
                    {
                        MatXY[coord[2]][coord[1]] = Math.Max(val, MatXY[coord[2]][coord[1]]);
                        MatYZ[coord[1]][coord[0]] = Math.Max(val, MatYZ[coord[1]][coord[0]]);
                        MatZX[coord[0]][coord[2]] = Math.Max(val, MatZX[coord[0]][coord[2]]);
                    }
                    catch (Exception e)
                    {
                        string m = e.Message;
                    }

                }
            }
        }

        public void atomsProjection(PdbAtoms pa)
        {
            //var jSideX = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_X));
            //var jSideY = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_Y));
            //var jSideV = @Html.Raw(Json.Serialize(@ViewBag.ScatXY_V));

            List<double> XY_X = new List<double>();
            List<double> XY_Y = new List<double>();
            List<double> XY_V = new List<double>();
            List<double> YZ_X = new List<double>();
            List<double> YZ_Y = new List<double>();
            List<double> YZ_V = new List<double>();
            List<double> ZX_X = new List<double>();
            List<double> ZX_Y = new List<double>();
            List<double> ZX_V = new List<double>();
            createData();
            foreach (var atom in pa.Atoms)
            {
                VectorThree xyz = atom.Value;
                VectorThree crs = _densityBinary.getCRSFromXYZ(xyz);
                double val = _interpMap.getValue(crs.A, crs.B, crs.C);
                //XY plane
                var indexXY = XY_V.BinarySearch(val);
                if (indexXY < 0) indexXY = ~indexXY;
                XY_V.Insert(indexXY, val);
                XY_X.Insert(indexXY, xyz.A);
                XY_Y.Insert(indexXY, xyz.B);
                //YZ plane
                var indexYZ = YZ_V.BinarySearch(val);
                if (indexYZ < 0) indexYZ = ~indexYZ;
                YZ_V.Insert(indexYZ, val);
                YZ_X.Insert(indexYZ, xyz.B);
                YZ_Y.Insert(indexYZ, xyz.C);
                //ZX plane
                var indexZX = ZX_V.BinarySearch(val);
                if (indexZX < 0) indexZX = ~indexZX;
                ZX_V.Insert(indexZX, val);
                ZX_X.Insert(indexZX, xyz.C);
                ZX_Y.Insert(indexZX, xyz.A);
            }

            // XY plane
            ScatXY_X = new double[XY_X.Count];
            ScatXY_Y = new double[XY_Y.Count];
            ScatXY_V = new double[XY_V.Count];
            for (int i=0; i < XY_V.Count; ++i)
            {
                ScatXY_X[i] = XY_X[i];
                ScatXY_Y[i] = XY_Y[i];
                ScatXY_V[i] = XY_V[i];
            }
            // YZ plane
            ScatYZ_X = new double[YZ_X.Count];
            ScatYZ_Y = new double[YZ_Y.Count];
            ScatYZ_V = new double[YZ_V.Count];
            for (int i = 0; i < YZ_V.Count; ++i)
            {
                ScatYZ_X[i] = YZ_X[i];
                ScatYZ_Y[i] = YZ_Y[i];
                ScatYZ_V[i] = YZ_V[i];
            }
            // ZX plane
            ScatZX_X = new double[ZX_X.Count];
            ScatZX_Y = new double[ZX_Y.Count];
            ScatZX_V = new double[ZX_V.Count];
            for (int i = 0; i < ZX_V.Count; ++i)
            {
                ScatZX_X[i] = ZX_X[i];
                ScatZX_Y[i] = ZX_Y[i];
                ScatZX_V[i] = ZX_V[i];
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
        }

    }
}
