namespace LeuciShared
{
    // ****** ABSTRACT CLASS ****************************************
    public abstract class Interpolator
    {
        //protected byte[] _binary;
        protected Single[] _singles;
        protected int _bStart;
        protected int _bLength;
        protected int XLen;
        protected int YLen;
        protected int ZLen;
        protected double h;

        // ABSTRACT INTERFACE ------------------------------------------------
        public abstract double getValue(double x, double y, double z);

        //--------------------------------------------------------------------

        //public Interpolator(byte[] binary,int bstart,int blength,int x, int y, int z)
        public Interpolator(Single[] binary, int bstart, int blength, int x, int y, int z)
        {//init without data for conversions
            //x = slowest, y = middle, z = fastest changing axis         
            XLen = x;
            YLen = y;
            ZLen = z;
            h = 0.00001;
            _singles = binary;
            //_binary = binary;
            _bStart = bstart;
            _bLength = blength;
        }
        public int getExactPos(int x, int y, int z)
        {
            int sliceSize = XLen * YLen;
            int pos = z * sliceSize;
            pos += XLen * y;
            pos += x;
            return pos;
        }
        public int getPosition(int x, int y, int z)
        {
            int sliceSize = XLen * YLen;
            int pos = z * sliceSize;
            pos += XLen * y;
            pos += x;
            return pos;
            //if (Matrix.ContainsKey(pos))
            //    return pos;
            //else
            //    return 0;//what should this return? -1, throw an error? TODO
        }
        public Single getExactValueBinary(int x, int y, int z)
        {
            int sliceSize = XLen * YLen;
            int pos = z * sliceSize;
            pos += XLen * y;
            pos += x;
            if (pos >= _singles.Length)
                return 0;
            if (pos < 0)
                return 0;
            try
            {
                //Single value = BitConverter.ToSingle(_binary, _bStart + pos * 4);
                Single value = _singles[pos];
                return value;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public double getRadient(double x, double y, double z)
        {
            double val = getValue(x, y, z);
            double dx = (getValue(x + h, y, z) - val) / h;
            double dy = (getValue(x, y + h, z) - val) / h;
            double dz = (getValue(x, y, z + h) - val) / h;
            double radient = (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz)) / 3;
            return radient;
        }
        public double getLaplacian(double x, double y, double z)
        {
            double val = getValue(x, y, z);
            double xx = getDxDx(x, y, z, val);
            double yy = getDyDy(x, y, z, val);
            double zz = getDzDz(x, y, z, val);
            return xx + yy + zz;
        }
        public double getDxDx(double x, double y, double z, double val)
        {
            double va = getValue(x - h, y, z);
            double vb = getValue(x + h, y, z);
            double dd = (va + vb - 2 * val) / (h * h);
            return dd;
        }
        public double getDyDy(double x, double y, double z, double val)
        {
            double va = getValue(x, y - h, z);
            double vb = getValue(x, y + h, z);
            double dd = (va + vb - 2 * val) / (h * h);
            return dd;
        }
        public double getDzDz(double x, double y, double z, double val)
        {
            double va = getValue(x, y, z - h);
            double vb = getValue(x, y, z + h);
            double dd = (va + vb - 2 * val) / (h * h);
            return dd;
        }

        protected Single[] getSmallerCubeMultivariate(double x, double y, double z, int width)
        {
            // 1. Build the points around the centre as a cube - 8 points
            Single[] vals = new Single[width * width * width];

            int count = 0;
            int xp = 0;
            int yp = 0;
            int zp = 0;
            for (int i = (-1 * width / 2) + 1; i < (width / 2) + 1; ++i)
            {
                xp = Convert.ToInt32(Math.Floor(x + i));
                for (int j = (-1 * width / 2) + 1; j < (width / 2) + 1; ++j)
                {
                    yp = Convert.ToInt32(Math.Floor(y + j));
                    for (int k = (-1 * width / 2) + 1; k < (width / 2) + 1; ++k)
                    {
                        zp = Convert.ToInt32(Math.Floor(z + k));
                        double p = -1;
                        if (xp >= 0 && yp >= 0 && zp >= 0 && xp < XLen && yp < YLen && zp < ZLen)
                            p = getExactValueBinary(xp, yp, zp);
                        vals[count] = Convert.ToSingle(p);
                        ++count;
                    }
                }
            }
            return vals;
        }

        protected Single[] getSmallerCubeThevenaz(int x, int y, int z, int width)
        {
            // 1. Build the points around the centre as a cube - 8 points
            Single[] vals = new Single[width * width * width];

            int count = 0;
            for (int i = z; i < z + width; ++i)
            {
                for (int j = y; j < y + width; ++j)
                {
                    for (int k = x; k < x + width; ++k)
                    {
                        Single p = getExactValueBinary(k, j, i);
                        vals[count] = p;
                        ++count;
                    }
                }
            }
            return vals;
        }
        protected Single[] getCubeWhole()
        {
            // 1. Build the points around the centre as a cube - 8 points
            Single[] vals = new Single[XLen * YLen * ZLen];

            int count = 0;
            for (int i = 0; i < ZLen; ++i)
            {
                for (int j = 0; j < YLen; ++j)
                {
                    for (int k = 0; k < XLen; ++k)
                    {
                        Single p = getExactValueBinary(k, j, i);
                        vals[count] = p;
                        ++count;
                    }
                }
            }
            return vals;
        }

        public bool isValid(double x, double y, double z)
        {
            int i = Convert.ToInt32(Math.Round(x));
            int j = Convert.ToInt32(Math.Round(y));
            int k = Convert.ToInt32(Math.Round(z));
            if (i < 0 || j < 0 || k < 0)
                return false;

            if (i > XLen || j > YLen || k > ZLen)
                return false;

            //int sliceSize = XLen * YLen;
            //int pos = k * sliceSize;
            //pos += XLen * j;
            //pos += i;
            //if (pos >= _singles.Length)
            //    return false;
            //else
            return true;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class Nearest : Interpolator
    {
        // ****** Nearest Neighbour Implementation ****************************************
        public Nearest(Single[] bytes, int start, int length, int x, int y, int z) : base(bytes, start, length, x, y, z)
        {

        }
        public override double getValue(double x, double y, double z)
        {
            if (!isValid(x, y, z))
                return 0;
            int i = Convert.ToInt32(Math.Round(x));
            int j = Convert.ToInt32(Math.Round(y));
            int k = Convert.ToInt32(Math.Round(z));
            if (i >= 0 && j >= 0 && k >= 0)
                return getExactValueBinary(i, j, k);
            else
                return 0;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
    public class Multivariate : Interpolator
    {
        protected int _degree;
        protected int _dimsize;
        protected int _points;
        protected bool _neednewVals = true;
        protected int _xFloor = 0;
        protected int _yFloor = 0;
        protected int _zFloor = 0;
        protected double[,,] _polyCoeffs = new double[0, 0, 0];
        // ****** Linear Implementation ****************************************
        public Multivariate(Single[] bytes, int start, int length, int x, int y, int z, int degree) : base(bytes, start, length, x, y, z)
        {
            // the degree must be an odd number
            _degree = degree;
            _points = _degree + 1;
            _dimsize = (int)Math.Pow(_points, 3);

        }
        public override double getValue(double x, double y, double z)
        {
            // The method of linear interpolation is a version of my own method for multivariate fitting, instead of trilinear interpolation
            // NOTE I could extend this to be multivariate not linear but it has no advantage over bspline - and is slower and not as good 
            // Document is here: https://rachelalcraft.github.io/Papers/MultivariateInterpolation/MultivariateInterpolation.pdf

            if (!isValid(x, y, z))
                return 0;

            bool recalc = _neednewVals;

            // we can reuse our last matrix if the points are within the same unit cube
            int xFloor = (int)Math.Floor(x);
            int yFloor = (int)Math.Floor(y);
            int zFloor = (int)Math.Floor(z);
            if (xFloor != _xFloor)
                recalc = true;
            if (yFloor != _yFloor)
                recalc = true;
            if (zFloor != _zFloor)
                recalc = true;

            if (recalc)
            {
                _xFloor = (int)Math.Floor(x);
                _yFloor = (int)Math.Floor(y);
                _zFloor = (int)Math.Floor(z);
                // 1. Build the points around the centre as a cube - 8 points
                Single[] vals = getSmallerCubeMultivariate(x, y, z, _points);
                //2. Multiply with the precomputed matrix to find the multivariate polynomial
                double[] ABC = multMatrixVector(getInverse(), vals);

                //3. Put the 8 values back into a cube
                _polyCoeffs = new double[_points, _points, _points];
                int pos = 0;
                for (int i = 0; i < _points; ++i)
                {
                    for (int j = 0; j < _points; ++j)
                    {
                        for (int k = 0; k < _points; ++k)
                        {
                            _polyCoeffs[i, j, k] = ABC[pos];
                            ++pos;
                        }
                    }
                }
                _neednewVals = false;
            }

            //4. Adjust the values to be within this cube
            int pstart = (-1 * _points / 2) + 1;
            double xn = x - Math.Floor(x) - pstart;
            double yn = y - Math.Floor(y) - pstart;
            double zn = z - Math.Floor(z) - pstart;

            //5. Apply the multivariate polynomial coefficents to find the value
            return getValueMultiVariate(zn, yn, xn, _polyCoeffs);
        }
        double getValueMultiVariate(double x, double y, double z, double[,,] coeffs)
        {/*This is using a value scheme that makes sens of our new fitted polyCube
          * In a linear case it will be a decimal between 0 and 1
          */
            double value = 0;

            for (int i = 0; i < coeffs.GetLength(0); ++i)
            {
                for (int j = 0; j < coeffs.GetLength(1); ++j)
                {
                    for (int k = 0; k < coeffs.GetLength(2); ++k)
                    {
                        double coeff = coeffs[i, j, k];
                        double val = coeff * Math.Pow(z, i) * Math.Pow(y, j) * Math.Pow(x, k);
                        value += val;
                    }
                }
            }
            return value;
        }
        private double[] multMatrixVector(double[,] A, Single[] V)
        {
            int length = V.Length;
            double[] results = new double[length];

            for (int row = 0; row < length; ++row)
            {
                double sum = 0;
                for (int col = 0; col < length; ++col)
                {
                    sum += A[row, col] * V[col];
                }
                results[row] = sum;
            }
            return results;
        }
        private double[,] getInverse()
        {
            if (_degree == 5)
                return InvariantVandermonde.Instance.inverse5;
            else if (_degree == 3)
                return InvariantVandermonde.Instance.inverse3;
            else
                return InvariantVandermonde.Instance.inverse1;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
    public class OptBSpline : Interpolator
    {
        protected int _degree;
        protected int _dimsize;
        protected int _points;
        protected BetaSpline? _bsp;
        protected int _xFloor = 0;
        protected int _yFloor = 0;
        protected int _zFloor = 0;

        // ****** Linear Implementation ****************************************
        public OptBSpline(Single[] bytes, int start, int length, int x, int y, int z, int degree, int points) : base(bytes, start, length, x, y, z)
        {
            // the degree must be an odd number
            _degree = degree;
            _points = points;
            if (_points > XLen)
                _points = XLen;
            if (_points > YLen)
                _points = YLen;
            if (_points > ZLen)
                _points = ZLen;

            _dimsize = (int)Math.Pow(_points, 3);
            _bsp = null;
        }
        public override double getValue(double x, double y, double z)
        {
            if (!isValid(x, y, z))
                return 0;

            // The method of linear interpolation is a version of my own method for multivariate fitting, instead of trilinear interpolation
            // NOTE I could extend this to be multivariate not linear but it has no advantage over bspline - and is slower and not as good 
            // Document is here: https://rachelalcraft.github.io/Papers/MultivariateInterpolation/MultivariateInterpolation.pdf

            //1.  do we need a new cube?
            // a) we do if we don't have one
            bool needNewMatrix = false;
            if (_bsp == null)
                needNewMatrix = true;
            // b) we do if we are within points/2 of the edges of the edges
            int myxFloor = (int)Math.Floor(x + (-1 * _points / 2) + 1);
            int myyFloor = (int)Math.Floor(y + (-1 * _points / 2) + 1);
            int myzFloor = (int)Math.Floor(z + (-1 * _points / 2) + 1);

            //if (myyFloor - _yFloor > _points / 4)
            double pointsBound = _points / 2;
            if (!(Math.Abs(myxFloor - _xFloor) < pointsBound))
                needNewMatrix = true;
            if (!(Math.Abs(myyFloor - _yFloor) < pointsBound))
                needNewMatrix = true;
            if (!(Math.Abs(myzFloor - _zFloor) < pointsBound))
                needNewMatrix = true;

            //if (_bsp != null)
            //    needNewMatrix = false;

            if (needNewMatrix)
            {
                _xFloor = (int)Math.Floor(x + (-1 * _points / 2) + 1);
                _yFloor = (int)Math.Floor(y + (-1 * _points / 2) + 1);
                _zFloor = (int)Math.Floor(z + (-1 * _points / 2) + 1);
                if (_xFloor < 0)
                    _xFloor = 0;
                if (_yFloor < 0)
                    _yFloor = 0;
                if (_zFloor < 0)
                    _zFloor = 0;
                if (_xFloor + _points > XLen)
                    _xFloor = XLen - _points;
                if (_yFloor + _points > YLen)
                    _yFloor = YLen - _points;
                if (_zFloor + _points > ZLen)
                    _zFloor = ZLen - _points;

                // 1. Build the points around the centre as a cube - x*x*x points                

                Single[] vals = getSmallerCubeThevenaz(_xFloor, _yFloor, _zFloor, _points);
                //3. Kind of recursive, make a smaller BSlipe interpolator out of this.                             
                _bsp = new BetaSpline(vals, 0, _points * _points * _points, _points, _points, _points, _degree, true);



                //3. alt test it all                
                /*Single[] vals = getSmallerCubeThevenaz(0, 0, 0, _points);
                _xFloor = 0;
                _yFloor = 0;
                _zFloor = 0;
                _bsp = new BetaSpline(vals, 0, _points * _points * _points, _points, _points, _points, _degree);*/

            }
            else
            {

            }

            //1. Find the values that would be in this cube 
            int pstart = (-1 * _points / 2) + 1;
            double xn = x - _xFloor;
            double yn = y - _yFloor;
            double zn = z - _zFloor;

            //5. Apply the multivariate polynomial coefficents to find the value
            return _bsp.getValue(xn, yn, zn);
            //return _bsp.getValue(x, y, z);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class BetaSpline : Interpolator
    {
        // ****** Thevenaz Spline Convolution Implementation ****************************************
        // Th?venaz, Philippe, Thierry Blu, and Michael Unser. ?Image Interpolation and Resampling?, n.d., 39.
        //   http://bigwww.epfl.ch/thevenaz/interpolation/
        // *******************************************************************************
        private double TOLERANCE;
        private int _degree;
        //private Dictionary<int,double> _coefficients = new Dictionary<int,double>();
        private Single[] _coefficients;
        //protected Dictionary<int, double> Matrix;
        //protected Single[] Matrix;

        public BetaSpline(Single[] bytes, int start, int length, int x, int y, int z, int degree) : base(bytes, start, length, x, y, z)
        {
            TOLERANCE = 2.2204460492503131e-016; // smallest such that 1.0+DBL_EPSILON != 1.0
            _degree = degree;
            _coefficients = getCubeWhole();
            //makeSubMatrix(0, 0, 0, 0, 0, 0);//dummy for now
            createCoefficients();
        }
        public BetaSpline(Single[] vals, int start, int length, int x, int y, int z, int degree, bool smaller) : base(vals, start, length, x, y, z)
        {
            TOLERANCE = 2.2204460492503131e-016; // smallest such that 1.0+DBL_EPSILON != 1.0
            _degree = degree;
            _coefficients = vals;
            //makeSubMatrix(0, 0, 0, 0, 0, 0);//dummy for now
            createCoefficients();
        }
        /*public void makeSubMatrix(int minx, int miny, int minz, int maxx, int maxy, int maxz)
        {
            //int count = 0;
            //Matrix = new Dictionary<int, double>();
            Matrix = new float[_binary.Length - _bStart];
            for (int i = _bStart; i < _binary.Length; i += 4)
            {
                Single value = BitConverter.ToSingle(_binary, i);
                //Matrix[count] = value;
                Matrix[i-_bStart] = value;
                //count++;
            }            
        }*/
        /*public double getExactValueMat(int x, int y, int z)
        {
            int sliceSize = XLen * YLen;
            int pos = z * sliceSize;
            pos += XLen * y;
            pos += x;            
            //if (Matrix.ContainsKey(pos))
            if (pos < Matrix.Length)
                return Matrix[pos];
            else
                return 0;
        }*/
        public override double getValue(double x, double y, double z)
        {
            if (!isValid(x, y, z))
                return 0;

            int weight_length = _degree + 1;
            List<int> xIndex = new List<int>();
            List<int> yIndex = new List<int>();
            List<int> zIndex = new List<int>();
            List<double> xWeight = new List<double>();
            List<double> yWeight = new List<double>();
            List<double> zWeight = new List<double>();
            for (int s = 0; s < weight_length; ++s)
            {
                xIndex.Add(0);
                yIndex.Add(0);
                zIndex.Add(0);
                xWeight.Add(0);
                yWeight.Add(0);
                zWeight.Add(0);
            }


            //Compte the interpolation indices
            int i = Convert.ToInt32(Math.Floor(x) - _degree / 2);
            int j = Convert.ToInt32(Math.Floor(y) - _degree / 2);
            int k = Convert.ToInt32(Math.Floor(z) - _degree / 2);

            for (int l = 0; l <= _degree; ++l)
            {
                xIndex[l] = i++; //if 71.1 passed in, for linear, we would want 71 and 72, 
                yIndex[l] = j++;
                zIndex[l] = k++;
            }

            /* compute the interpolation weights */

            if (_degree == 9)
            {
                xWeight = applyValue9(x, xIndex, weight_length);
                yWeight = applyValue9(y, yIndex, weight_length);
                zWeight = applyValue9(z, zIndex, weight_length);
            }
            else if (_degree == 7)
            {
                xWeight = applyValue7(x, xIndex, weight_length);
                yWeight = applyValue7(y, yIndex, weight_length);
                zWeight = applyValue7(z, zIndex, weight_length);
            }
            else if (_degree == 5)
            {
                xWeight = applyValue5(x, xIndex, weight_length);
                yWeight = applyValue5(y, yIndex, weight_length);
                zWeight = applyValue5(z, zIndex, weight_length);
            }
            else
            {
                xWeight = applyValue3(x, xIndex, weight_length);
                yWeight = applyValue3(y, yIndex, weight_length);
                zWeight = applyValue3(z, zIndex, weight_length);
            }

            //applying the mirror boundary condition becaue I am only interpolating within values??
            int Width2 = 2 * XLen - 2;
            int Height2 = 2 * YLen - 2;
            int Depth2 = 2 * ZLen - 2;
            for (k = 0; k <= _degree; k++)
            {
                xIndex[k] = (XLen == 1) ? (0) :
                    ((xIndex[k] < 0) ?
                        (-xIndex[k] - Width2 * ((-xIndex[k]) / Width2)) :
                        (xIndex[k] - Width2 * (xIndex[k] / Width2)));
                if (XLen <= xIndex[k])
                {
                    xIndex[k] = Width2 - xIndex[k];
                }

                yIndex[k] = (YLen == 1) ? (0) :
                    ((yIndex[k] < 0) ?
                        (-yIndex[k] - Height2 * ((-yIndex[k]) / Height2)) :
                        (yIndex[k] - Height2 * (yIndex[k] / Height2)));
                if (YLen <= yIndex[k])
                {
                    yIndex[k] = Height2 - yIndex[k];
                }

                zIndex[k] = (ZLen == 1) ? (0) :
                    ((zIndex[k] < 0) ?
                        (-zIndex[k] - Depth2 * ((-zIndex[k]) / Depth2)) :
                        (zIndex[k] - Depth2 * (zIndex[k] / Depth2)));
                if (ZLen <= zIndex[k])
                {
                    zIndex[k] = Depth2 - zIndex[k];
                }
            }

            //Perform interolation
            /* perform interpolation */
            int splineDegree = _degree;
            double w3 = 0.0;
            for (k = 0; k <= splineDegree; k++)
            {
                double w2 = 0.0;
                for (j = 0; j <= splineDegree; j++)
                {
                    double w1 = 0.0;
                    for (i = 0; i <= splineDegree; i++)
                    {
                        w1 += xWeight[i] * getCoef(xIndex[i], yIndex[j], zIndex[k]);
                    }
                    w2 += yWeight[j] * w1;
                }
                w3 += zWeight[k] * w2;
            }
            return w3;
        }

        private double getCoef(int x, int y, int z)
        {
            int pos = getPosition(x, y, z);
            try
            {
                //Single value = BitConverter.ToSingle(_coefficients, _bStart + pos * 4);
                Single value = _coefficients[pos];
                return value;
            }
            catch (Exception e)
            {
                return 0;
            }
            //if (_coefficients.ContainsKey(pos))
            //    return _coefficients[pos];
            //else            
            //return 0;
        }
        private void putCoef(int x, int y, int z, double v)
        {
            int pos = getPosition(x, y, z);
            /*byte[] bv = BitConverter.GetBytes(Convert.ToSingle(v));
            int start = _bStart + pos * 4;
            foreach (byte b in bv)
            {
                _coefficients[start] = b;
                start++;
            }*/
            _coefficients[pos] = Convert.ToSingle(v);
            //if (_coefficients.ContainsKey(pos))
            //    _coefficients[pos] = v;            
        }
        private void createCoefficients()
        {
            //foreach (var v in Matrix)
            //{
            //    _coefficients.Add(v.Key,v.Value);                
            //}
            //for (int b = 0; b < _binary.Length; ++b)
            //    _coefficients[b] = _binary[b];

            List<double> pole = getPole(_degree);
            int numPoles = Convert.ToInt32(pole.Count);

            //Convert the samples to interpolation coefficients
            //X-wise
            for (int y = 0; y < YLen; ++y)
            {
                for (int z = 0; z < ZLen; ++z)
                {
                    List<double> row = getRow3d(y, z, XLen);
                    List<double> line = convertToInterpolationCoefficients(pole, numPoles, XLen, row);
                    putRow3d(y, z, line, XLen);
                }
            }
            //Y-wise
            for (int x = 0; x < XLen; ++x)
            {
                for (int z = 0; z < ZLen; ++z)
                {
                    List<double> row = getColumn3d(x, z, YLen);
                    List<double> line = convertToInterpolationCoefficients(pole, numPoles, YLen, row);
                    putColumn3d(x, z, line, YLen);
                }
            }

            //Z-wise
            for (int x = 0; x < XLen; ++x)
            {
                for (int y = 0; y < YLen; ++y)
                {
                    List<double> row = getHole3d(x, y, ZLen);
                    List<double> line = convertToInterpolationCoefficients(pole, numPoles, ZLen, row);
                    putHole3d(x, y, line, ZLen);
                }
            }
        }

        private List<double> getRow3d(int y, int z, int length)
        {
            List<double> row = new List<double>();
            for (int x = 0; x < length; ++x)
                row.Add(getCoef(x, y, z));
            return row;
        }
        private void putRow3d(int y, int z, List<double> row, int length)
        {
            for (int x = 0; x < length; ++x)
                putCoef(x, y, z, row[x]);
        }
        private List<double> getColumn3d(int x, int z, int length)
        {
            List<double> col = new List<double>();
            for (int y = 0; y < length; ++y)
                col.Add(getCoef(x, y, z));
            return col;
        }
        private void putColumn3d(int x, int z, List<double> col, int length)
        {
            for (int y = 0; y < length; ++y)
                putCoef(x, y, z, col[y]);
        }
        private List<double> getHole3d(int x, int y, int length)
        {
            List<double> bore = new List<double>();
            for (int z = 0; z < length; ++z)
                bore.Add(getCoef(x, y, z));
            return bore;
        }
        private void putHole3d(int x, int y, List<double> bore, int length)
        {
            for (int z = 0; z < length; ++z)
                putCoef(x, y, z, bore[z]);
        }

        private List<double> getPole(int degree)
        {
            //Recover the poles from a lookup table #currently only 3 degree, will I want to calculate all the possibilities at the beginnning, 3,5,7,9?
            List<double> pole = new List<double>();
            if (degree == 9)
            {
                pole.Add(-0.60799738916862577900772082395428976943963471853991);
                pole.Add(-0.20175052019315323879606468505597043468089886575747);
                pole.Add(-0.043222608540481752133321142979429688265852380231497);
                pole.Add(-0.0021213069031808184203048965578486234220548560988624);
            }
            else if (degree == 7)
            {
                pole.Add(-0.53528043079643816554240378168164607183392315234269);
                pole.Add(-0.12255461519232669051527226435935734360548654942730);
                pole.Add(-0.0091486948096082769285930216516478534156925639545994);
            }
            else if (degree == 5)
            {
                pole.Add(Math.Sqrt(135.0 / 2.0 - Math.Sqrt(17745.0 / 4.0)) + Math.Sqrt(105.0 / 4.0) - 13.0 / 2.0);
                pole.Add(Math.Sqrt(135.0 / 2.0 + Math.Sqrt(17745.0 / 4.0)) - Math.Sqrt(105.0 / 4.0) - 13.0 / 2.0);
            }
            else//then it is 3
            {
                pole.Add(Math.Sqrt(3.0) - 2.0);
            }
            return pole;
        }

        private List<double> convertToInterpolationCoefficients(List<double> pole, int numPoles, int width, List<double> row)
        {
            /* special case required by mirror boundaries */
            if (width == 1)
                return row; ;

            double lambda = 1;
            int n = 0;
            int k = 0;
            //Compute the overall gain
            for (k = 0; k < numPoles; k++)
            {
                lambda = lambda * (1 - pole[k]) * (1 - 1 / pole[k]);
            }
            //Apply the gain
            for (n = 0; n < width; n++)
            {
                row[n] *= lambda;
            }
            //loop over the poles            
            for (k = 0; k < numPoles; k++)
            {
                /* causal initialization */
                row[0] = InitialCausalCoefficient(row, width, pole[k]);
                /* causal recursion */
                for (n = 1; n < width; n++)
                {
                    row[n] += (double)pole[k] * row[n - 1];
                }
                /* anticausal initialization */
                row[width - 1] = InitialAntiCausalCoefficient(row, width, pole[k]);
                /* anticausal recursion */
                for (n = width - 2; 0 <= n; n--)
                {
                    row[n] = pole[k] * (row[n + 1] - row[n]);
                }
            }
            return row;
        }

        private double InitialCausalCoefficient(List<double> c, int dataLength, double pole)

        { /* begin InitialCausalCoefficient */

            double Sum, zn, z2n, iz;
            int n, Horizon;

            /* this initialization corresponds to mirror boundaries */
            Horizon = dataLength;
            if (TOLERANCE > 0.0)
            {
                Horizon = (int)Math.Ceiling(Math.Log(TOLERANCE) / Math.Log(Math.Abs(pole)));
            }
            if (Horizon < dataLength)
            {
                /* accelerated loop */
                zn = pole;
                Sum = c[0];
                for (n = 1; n < Horizon; n++)
                {
                    Sum += zn * c[n];
                    zn *= pole;
                }
                return (Sum);
            }
            else
            {
                /* full loop */
                zn = pole;
                iz = 1.0 / pole;
                z2n = Math.Pow(pole, (double)(dataLength - 1));
                Sum = c[0] + z2n * (double)c[dataLength - 1];
                z2n *= z2n * iz; //is this a mistake, should it be just *=??? Checked it is how it is in their code. NO TRIED IT.                
                for (n = 1; n <= dataLength - 2; n++)
                {
                    Sum += (zn + z2n) * c[n];
                    zn *= pole;
                    //z2n *= z2n * iz;
                    z2n *= iz;
                }
                return (Sum / (1.0 - zn * zn));
            }
        }
        private double InitialAntiCausalCoefficient(List<double> c, int dataLength, double pole)
        {
            /* this initialization corresponds to mirror boundaries */
            if (dataLength < 2)
                return 0;
            else
                return ((pole / (pole * pole - 1.0)) * (pole * c[dataLength - 2] + c[dataLength - 1]));
        }
        private List<double> applyValue3(double val, List<int> idc, int weight_length)
        {
            List<double> ws = new List<double>();
            for (int i = 0; i < weight_length; ++i)
                ws.Add(0);
            double w = val - (double)idc[1];
            ws[3] = (1.0 / 6.0) * w * w * w;
            ws[0] = (1.0 / 6.0) + (1.0 / 2.0) * w * (w - 1.0) - ws[3];
            ws[2] = w + ws[0] - 2.0 * ws[3];
            ws[1] = 1.0 - ws[0] - ws[2] - ws[3];
            return ws;
        }

        private List<double> applyValue5(double val, List<int> idc, int weight_length)
        {
            List<double> ws = new List<double>();
            for (int i = 0; i < weight_length; ++i)
                ws.Add(0);
            double w = val - (double)idc[2];
            double w2 = w * w;
            ws[5] = (1.0 / 120.0) * w * w2 * w2;
            w2 -= w;
            double w4 = w2 * w2;
            w -= 1.0 / 2.0;
            double t = w2 * (w2 - 3.0);
            ws[0] = (1.0 / 24.0) * (1.0 / 5.0 + w2 + w4) - ws[5];
            double t0 = (1.0 / 24.0) * (w2 * (w2 - 5.0) + 46.0 / 5.0);
            double t1 = (-1.0 / 12.0) * w * (t + 4.0);
            ws[2] = t0 + t1;
            ws[3] = t0 - t1;
            t0 = (1.0 / 16.0) * (9.0 / 5.0 - t);
            t1 = (1.0 / 24.0) * w * (w4 - w2 - 5.0);
            ws[1] = t0 + t1;
            ws[4] = t0 - t1;
            return ws;
        }

        private List<double> applyValue7(double val, List<int> idc, int weight_length)
        {
            List<double> ws = new List<double>();
            for (int i = 0; i < weight_length; ++i)
                ws.Add(0);
            double w = val - (double)idc[3];
            ws[0] = 1.0 - w;
            ws[0] *= ws[0];
            ws[0] *= ws[0] * ws[0];
            ws[0] *= (1.0 - w) / 5040.0;
            double w2 = w * w;
            ws[1] = (120.0 / 7.0 + w * (-56.0 + w * (72.0 + w * (-40.0 + w2 * (12.0 + w * (-6.0 + w)))))) / 720.0;
            ws[2] = (397.0 / 7.0 - w * (245.0 / 3.0 + w * (-15.0 + w * (-95.0 / 3.0 + w * (15.0 + w * (5.0 + w * (-5.0 + w))))))) / 240.0;
            ws[3] = (2416.0 / 35.0 + w2 * (-48.0 + w2 * (16.0 + w2 * (-4.0 + w)))) / 144.0;
            ws[4] = (1191.0 / 35.0 - w * (-49.0 + w * (-9.0 + w * (19.0 + w * (-3.0 + w) * (-3.0 + w2))))) / 144.0;
            ws[5] = (40.0 / 7.0 + w * (56.0 / 3.0 + w * (24.0 + w * (40.0 / 3.0 + w2 * (-4.0 + w * (-2.0 + w)))))) / 240.0;
            ws[7] = w2;
            ws[7] *= ws[7] * ws[7];
            ws[7] *= w / 5040.0;
            ws[6] = 1.0 - ws[0] - ws[1] - ws[2] - ws[3] - ws[4] - ws[5] - ws[7];
            return ws;
        }

        private List<double> applyValue9(double val, List<int> idc, int weight_length)
        {
            List<double> ws = new List<double>();
            for (int i = 0; i < weight_length; ++i)
                ws.Add(0);
            double w = val - (double)idc[4];
            ws[0] = 1.0 - w;
            ws[0] *= ws[0];
            ws[0] *= ws[0];
            ws[0] *= ws[0] * (1.0 - w) / 362880.0;
            ws[1] = (502.0 / 9.0 + w * (-246.0 + w * (472.0 + w * (-504.0 + w * (308.0 + w * (-84.0 + w * (-56.0 / 3.0 + w * (24.0 + w * (-8.0 + w))))))))) / 40320.0;
            ws[2] = (3652.0 / 9.0 - w * (2023.0 / 2.0 + w * (-952.0 + w * (938.0 / 3.0 + w * (112.0 + w * (-119.0 + w * (56.0 / 3.0 + w * (14.0 + w * (-7.0 + w))))))))) / 10080.0;
            ws[3] = (44117.0 / 42.0 + w * (-2427.0 / 2.0 + w * (66.0 + w * (434.0 + w * (-129.0 + w * (-69.0 + w * (34.0 + w * (6.0 + w * (-6.0 + w))))))))) / 4320.0;
            double w2 = w * w;
            ws[4] = (78095.0 / 63.0 - w2 * (700.0 + w2 * (-190.0 + w2 * (100.0 / 3.0 + w2 * (-5.0 + w))))) / 2880.0;
            ws[5] = (44117.0 / 63.0 + w * (809.0 + w * (44.0 + w * (-868.0 / 3.0 + w * (-86.0 + w * (46.0 + w * (68.0 / 3.0 + w * (-4.0 + w * (-4.0 + w))))))))) / 2880.0;
            ws[6] = (3652.0 / 21.0 - w * (-867.0 / 2.0 + w * (-408.0 + w * (-134.0 + w * (48.0 + w * (51.0 + w * (-4.0 + w) * (-1.0 + w) * (2.0 + w))))))) / 4320.0;
            ws[7] = (251.0 / 18.0 + w * (123.0 / 2.0 + w * (118.0 + w * (126.0 + w * (77.0 + w * (21.0 + w * (-14.0 / 3.0 + w * (-6.0 + w * (-2.0 + w))))))))) / 10080.0;
            ws[9] = w2 * w2;
            ws[9] *= ws[9] * w / 362880.0;
            ws[8] = 1.0 - ws[0] - ws[1] - ws[2] - ws[3] - ws[4] - ws[5] - ws[6] - ws[7] - ws[9];
            return ws;
        }


    }

}
