namespace Leucippus.Models
{
    // This takes the dimensions of a cube
    // and given a point returns the 3d coords around it
    // or the 1d list around it
    public class Cubelet
    {
        private int _a;
        private int _b;
        private int _c;
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
        public Cubelet (int a, int b, int c)
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
            return x_bit + y_bit + x_bit;
        }
        public int[] get3D(int i)
        {
            //int div = 5 / 3; //quotient is 1
            //int mod = 5 % 3; //remainder is 2
            int x = i / (_b * _c);
            int newi = i - (x* _b * _c);
            int y = newi % (_a);
            int z = newi - (y * _b);                        
            return new int[] { x,y,z };
        }
    }
}
