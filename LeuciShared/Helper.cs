using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class Helper
    {
        public static double[][] convertArray(Array fromArr)
        {
            int dimA = fromArr.GetLength(0);
            int dimB = fromArr.GetLength(1);
            int lA = fromArr.GetLowerBound(0);
            int lB = fromArr.GetLowerBound(1);
            double[][] toArray = new double[dimA][];
            for (int a = 0; a < dimA; ++a)
            {
                toArray[a] = new double[dimB];
                for (int b = 0; b < dimB; ++b)
                    toArray[a][b] = (double)fromArr.GetValue(a+lA, b+lB);
            }
            return toArray;
        }

        public static double[] convertList(List<double> fromList)
        {
            int dimA = fromList.Count;            
            double[] toList = new double[dimA];
            for (int a = 0; a < dimA; ++a)                            
                toList[a] = fromList[a];            
            return toList;
        }

        public static double[][] subtractArrays(double[][] A, double[][] B)
        {
            if (A != null && A.Length > 0)
            {
                int dimA = A.Length;
                int dimB = A[0].Length;
                double[][] toArray = new double[dimA][];
                for (int a = 0; a < dimA; ++a)
                {
                    toArray[a] = new double[dimB];
                    for (int b = 0; b < dimB; ++b)
                        toArray[a][b] = A[a][b] - B[a][b];
                }
                return toArray;
            }
            else
            {
                return new double[0][];
            }
        }

        public static double[] listFromString(string strA)
        {
            if (strA == "")
                return new double[0];

            if (strA == null)
                return new double[0];

            string[] A = strA.Split(",");
            int dimA = A.Length;
            double[] toArray = new double[dimA];
            for (int b = 0; b < dimA; ++b)
            {
                toArray[b] = Convert.ToDouble(A[b]);                
            }
            return toArray;
        }
        public static double minmaxMatrix(double[][] A, bool isMin)
        {
            double min = 1000;
            double max = -1000;
            if (A.Length > 0)
            {
                int dimA = A.Length;
                int dimB = A[0].Length;                
                for (int a = 0; a < dimA; ++a)
                {
                    for (int b = 0; b < dimB; ++b)
                    {
                        min = Math.Min(min, A[a][b]);
                        max = Math.Max(max, A[a][b]);
                    }
                }                
            }
            if (isMin)
                return min;
            else
                return max;

        }
        public static double[][] unwrapList(string strA)
        {
            if (strA == "")
                return new double[0][];

            if (strA == null)
                return new double[0][];

            string[] A = strA.Split(",");
            if (A.Length > 0)
            {
                int dimA = (int)Math.Sqrt(A.Length);                
                double[][] toArray = new double[dimA][];
                int count = 0;
                for (int a = 0; a < dimA; ++a)
                {
                    toArray[a] = new double[dimA];
                    for (int b = 0; b < dimA; ++b)
                    {
                        toArray[a][b] = Convert.ToDouble(A[count]);
                        count++;
                    }
                }
                return toArray;
            }
            else
            {
                return new double[0][];
            }
        }

        public static SinglePosition singlePosDiff(SinglePosition p, SinglePosition q)
        {
            SinglePosition sliceD = new SinglePosition();
            sliceD.xAxis = p.xAxis;
            sliceD.yAxis = p.yAxis;
            sliceD.densityMatrix = Helper.subtractArrays(p.densityMatrix, q.densityMatrix);
            sliceD.radientMatrix = Helper.subtractArrays(p.radientMatrix, q.radientMatrix);
            sliceD.laplacianMatrix = Helper.subtractArrays(p.laplacianMatrix, q.laplacianMatrix);
            if (sliceD.xAxis.Length > 0)
            {
                sliceD.minD = Helper.minmaxMatrix(sliceD.densityMatrix,true);
                sliceD.maxD = Helper.minmaxMatrix(sliceD.densityMatrix, false);
                sliceD.minL = Helper.minmaxMatrix(sliceD.laplacianMatrix, true);
                sliceD.maxL = Helper.minmaxMatrix(sliceD.laplacianMatrix, false);
            }
            return sliceD;
        }


    }
}
