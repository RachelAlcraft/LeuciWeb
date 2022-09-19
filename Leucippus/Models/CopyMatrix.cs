using System.Drawing.Drawing2D;

namespace Leucippus.Models
{
    public class CopyMatrix
    {        
        private EdMatrix edmatrix = new EdMatrix();
        public CopyMatrix()
        {
            
        }
        public void set(int i, int j, double val)
        {
            //matrix[i, j] = val;
            edmatrix.matrix[i, j] = val;
        }
        public CopyMatrix getInverse()
        {
            EdMatrix tmpMatEd = new EdMatrix();
            tmpMatEd = edmatrix.getInverse();
            CopyMatrix retMat = new CopyMatrix();
            retMat.edmatrix.matrix = tmpMatEd.matrix;
            return retMat;
        }

        public EdVector multiply(EdVector v)
        {
            EdVector retVec2 = edmatrix.multiply(v);
            return retVec2;

        }

    }

}
