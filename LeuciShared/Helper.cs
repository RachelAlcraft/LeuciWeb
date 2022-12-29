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
    }
}
