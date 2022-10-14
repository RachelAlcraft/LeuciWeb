namespace Leucippus.Models
{
    /*public class EdMatrix
    {
        
        // Specifically to handle 3x3 matrices for 3d coordinates
        
        public double[,] matrix;
        public EdMatrix(double[,] inmatrix)
        {
            matrix = inmatrix;
        }

        public EdMatrix()
        {
            matrix = new double[3, 3];
        }

        public EdVector getColumn(int col)
        {
            EdVector colv = new EdVector();
            colv. vector[0] = matrix[0, col];
            colv.vector[1] = matrix[1, col];
            colv.vector[2] = matrix[2, col];
            return colv;
        }

        public EdVector multiply(EdVector v)
        {//http://mlwiki.org/index.php/Matrix-Vector_Multiplication            
            EdVector Ax = getColumn(0).scale(v.x);
            EdVector Ay = getColumn(1).scale(v.y);
            EdVector Az = getColumn(2).scale(v.z); 
            EdVector A = Ax.add(Ay).add(Az);
            return A;
        }
        public EdMatrix getInverse()
        {
            EdMatrix matforinv = new EdMatrix();
            matforinv.matrix = matrix;            
            double detWhole = determinant(matforinv.matrix);
            EdMatrix transpose = new EdMatrix();
            EdMatrix adjunct = new EdMatrix();
            EdMatrix inverse = new EdMatrix();
            if (detWhole != 0)
            {
                transpose.matrix[0, 0] = matforinv.matrix[0, 0];
                transpose.matrix[1, 1] = matforinv.matrix[1, 1];
                transpose.matrix[2, 2] = matforinv.matrix[2, 2];

                transpose.matrix[0, 1] = matforinv.matrix[1, 0];
                transpose.matrix[0, 2] = matforinv.matrix[2, 0];
                transpose.matrix[1, 0] = matforinv.matrix[0, 1];
                transpose.matrix[1, 2] = matforinv.matrix[2, 1];
                transpose.matrix[2, 0] = matforinv.matrix[0, 2];
                transpose.matrix[2, 1] = matforinv.matrix[1, 2];

                int factor = 1;

                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        double thisValue = transpose.matrix[i, j];
                        double[,] reduced = reduceMatrix(transpose.matrix, i, j);
                        double detReduced = transpose.determinant(reduced);
                        adjunct.matrix[i, j] = detReduced * factor;
                        inverse.matrix[i, j] = detReduced * factor / detWhole;
                        factor *= -1;
                    }
                }
                return inverse;
            }
            else
            {
                return new EdMatrix(); //empty
            }
        }

        private double[,] reduceMatrix(double[,] mat, int ii, int jj)
        {
            int cols = mat.GetUpperBound(1) + 1; //should (must) be square within this program
            double[,] reduced = new double[cols - 1, cols - 1];

            int col = -1;
            for (int i = 0; i < cols; ++i)
            {
                if (i != ii)
                {
                    ++col;
                    int row = -1;
                    for (int j = 0; j < cols; ++j)
                    {
                        if (j != jj)
                        {
                            ++row;
                            reduced[col, row] = mat[i, j];
                        }
                    }
                }
            }
            return reduced;
        }
        private double determinant(double[,] mat)
        {
            int cols = mat.GetUpperBound(1) + 1; //should (must) be square within this program
            if (cols == 2)
            {
                double n11 = mat[0, 0];
                double n12 = mat[0, 1];
                double n21 = mat[1, 0];
                double n22 = mat[1, 1];
                return (n11 * n22 - n12 * n21);
            }
            else
            {
                int factor = -1;
                double det = 0;
                for (int i = 0; i < cols; ++i)
                {
                    factor = factor * -1;
                    double row_val = mat[i, 0];
                    double[,] reduced = reduceMatrix(mat, i, 0);
                    double newdet = determinant(reduced);
                    det = det + (factor * row_val * newdet);
                }
                return det;
            }
        }
    }*/
}
