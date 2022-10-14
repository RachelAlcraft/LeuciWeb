using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class Cubelet
    {
        private int _a;
        private int _b;
        private int _c;
        public int Layer { get; set; }
        public string Plane { get; set; }
        /*
         * If this structure was init as 4,3,2
         * it would be 3 points wide, 4 high and 5 deep, (points not width)
         * so it would have 60 numbers in 1d
         * 
         * in this, c is the fastest changing axis.
         * 
         * if I gave it (0,0,0) and wanted a 2x2 cube with it in the corner
         * it would be - starting at 0
         * (0,0,0)
         * (1,0,0)
         * ...
         * (1,1,1)
         * 
         * In 1d it would be
         * 0  (0,0,0) 
         * 1  (0,0,1) 
         * 2  (0,1,0) 
         * 3  (0,1,1) 
         * 4  (0,2,0) 
         * 5  (0,2,1)          
         * 6  (1,0,0) 
         * 7  (1,0,1) 
         * 8  (1,1,0) 
         * 9  (1,1,1) 
         * 10 (1,2,0) 
         * 11 (1,2,1) 
         * 12 (2,0,0)          
         * etc
         * 
         * so the indices of the cube in 1d would be
         * 0,1,2,3,6,7,8,9
         * 
         * 
         * 
         */
        public Cubelet(int a, int b, int c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public int get1D(int x, int y, int z)
        {
            int x_bit = z;
            int y_bit = _a + y;
            int z_bit = (_a * _b) + x;
            return x_bit + y_bit + z_bit;
        }
        public int[] get3D(int i)
        {
            //int div = 5 / 3; //quotient is 1
            //int mod = 5 % 3; //remainder is 2
            int x = i / (_b * _c);
            int newi = i - (x * _b * _c);
            int y = newi % (_a);
            int z = newi - (y * _b);
            return new int[] { x, y, z };
        }

        public List<int[]> getPlaneCoords(string plane, int layer)
        {
            List<int[]> coords = new List<int[]>();
            int eX = _a;
            int eY = _b;
            int eZ = _c;

            int endX = 0;
            int endY = 0;
            if (layer < 0)
                layer = 0;

            int layermax = 0;

            if (plane == "XY")
            {
                endX = eX;
                endY = eY;
                layermax = eZ;
                if (layer >= eZ)
                    layer = eZ - 1;
            }
            else if (plane == "YZ")
            {
                endX = eY;
                endY = eZ;
                layermax = eX;
                if (layer >= eX)
                    layer = eX - 1;
            }
            else if (plane == "ZX")
            {
                endX = eZ;
                endY = eX;
                layermax = eY;
                if (layer >= eY)
                    layer = eY - 1;
            }

            Layer = layer;
            Plane = plane;
            
            for (int x = 0; x < endX; ++x)
            {
                for (int y = 0; y < endY; ++y)
                {
                    if (plane == "XY")                    
                        coords.Add(new int[] { x, y, layer });                   
                    else if (plane == "YZ")
                        coords.Add(new int[] { layer, x, y});
                    else if (plane == "ZX")
                        coords.Add(new int[] { y, layer, x});            
                }
            }
            return coords;
        }
        public int[] getPlaneDims(string plane, int layer)
        {
            int[] dims = new int[2] { 0, 0 };                        
            if (plane == "XY")
            {
                dims[0]= _a;
                dims[1] = _b;                
            }
            else if (plane == "YZ")
            {
                dims[0] = _b;
                dims[1] = _c;                
            }
            else if (plane == "ZX")
            {
                dims[0] = _c;
                dims[1] = _a;                
            }
            return dims;
        }

        public double[][] makeSquare(double[] doubles, int[] XY)
        {
            double[][] ret = new double[XY[0]][];//, XY[1]];
            int count = 0;
            for (int a = 0; a < XY[0]; ++a)
            {
                ret[a] = new double[XY[1]];
                for (int b = 0; b < XY[1]; ++b)
                {
                    ret[a][b] = doubles[count];
                    ++count;
                }
            }
            return ret;
        }
    }
    
}
