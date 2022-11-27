namespace LeuciShared
{
    public class MatrixThreeThree
    {
        private List<double> _matrix = new List<double>();

        public MatrixThreeThree()
        {
            _matrix = new List<double>();
            for (int i = 0; i < 9; ++i)
            {
                _matrix.Add(0);
            }
        }
        public MatrixThreeThree(List<double> vals)
        {
            for (int i = 0; i < 9; ++i)
            {
                _matrix.Add(vals[i]);
            }
        }
        public MatrixThreeThree getInverse()
        {
            double detWhole = getDeterminant();
            List<double> transp = new List<double>();
            transp.Add(_matrix[0]);
            transp.Add(_matrix[3]);
            transp.Add(_matrix[6]);
            transp.Add(_matrix[1]);
            transp.Add(_matrix[4]);
            transp.Add(_matrix[7]);
            transp.Add(_matrix[2]);
            transp.Add(_matrix[5]);
            transp.Add(_matrix[8]);

            MatrixThreeThree transpose = new MatrixThreeThree(transp);
            MatrixThreeThree matinverse = new MatrixThreeThree();
            MatrixThreeThree matinverseSwitch = new MatrixThreeThree();

            int factor = 1;

            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    double thisValue = transpose.getValue(i, j);
                    double detReduced = transpose.getInnerDeterminant(i, j);
                    matinverse.putValue(detReduced * factor / detWhole, i, j);
                    factor *= -1;
                }
            }
            return matinverse;
        }

        public double getDeterminant()
        {
            int factor = -1;
            double det = 0;
            for (int i = 0; i < 3; ++i)
            {
                factor = factor * -1;
                double row_val = _matrix[3 * i];
                double newdet = getInnerDeterminant(i, 0);
                det = det + (factor * row_val * newdet);
            }
            return det;
        }

        public double getInnerDeterminant(int col, int row)
        {
            List<double> smallMat = new List<double>();
            if (col == 0)
            {
                if (row != 0)
                {
                    smallMat.Add(_matrix[1]);
                    smallMat.Add(_matrix[2]);
                }
                if (row != 1)
                {
                    smallMat.Add(_matrix[4]);
                    smallMat.Add(_matrix[5]);
                }
                if (row != 2)
                {
                    smallMat.Add(_matrix[7]);
                    smallMat.Add(_matrix[8]);
                }
            }
            else if (col == 1)
            {
                if (row != 0)
                {
                    smallMat.Add(_matrix[0]);
                    smallMat.Add(_matrix[2]);
                }
                if (row != 1)
                {
                    smallMat.Add(_matrix[3]);
                    smallMat.Add(_matrix[5]);
                }
                if (row != 2)
                {
                    smallMat.Add(_matrix[6]);
                    smallMat.Add(_matrix[8]);
                }
            }
            else// (col == 1)
            {
                if (row != 0)
                {
                    smallMat.Add(_matrix[0]);
                    smallMat.Add(_matrix[1]);
                }
                if (row != 1)
                {
                    smallMat.Add(_matrix[3]);
                    smallMat.Add(_matrix[4]);
                }
                if (row != 2)
                {
                    smallMat.Add(_matrix[6]);
                    smallMat.Add(_matrix[7]);
                }
            }

            double n11 = smallMat[0];
            double n12 = smallMat[1];
            double n21 = smallMat[2];
            double n22 = smallMat[3];

            return n11 * n22 - n12 * n21;
        }
        public double getValue(int row, int col)
        {
            int pos = row * 3 + col;
            return _matrix[pos];
        }
        public void putValue(double val, int row, int col)
        {
            int pos = row * 3 + col;
            _matrix[pos] = val;
        }
        public VectorThree multiply(VectorThree col, bool byRow)
        {
            //So, this is by row not by column, or,,, anyway which is which...
            double col0 = col.A;
            double col1 = col.B;
            double col2 = col.C;

            VectorThree scaled = new VectorThree();

            double s0 = col0 * _matrix[byRow ? 0 : 0];
            double s1 = col0 * _matrix[byRow ? 1 : 3];
            double s2 = col0 * _matrix[byRow ? 2 : 6];

            s0 += col1 * _matrix[byRow ? 3 : 1];
            s1 += col1 * _matrix[byRow ? 4 : 4];
            s2 += col1 * _matrix[byRow ? 5 : 7];

            s0 += col2 * _matrix[byRow ? 6 : 2];
            s1 += col2 * _matrix[byRow ? 7 : 5];
            s2 += col2 * _matrix[byRow ? 8 : 8];

            scaled.putByIndex(0, s0);
            scaled.putByIndex(1, s1);
            scaled.putByIndex(2, s2);

            return scaled;
        }
    }
}
