namespace Leucippus.Models
{
    using ChartDirector;
    using NuGet.Packaging.Signing;
    using System;
    using System.Runtime.InteropServices;

    
    /*


    public class DllInterface
    {
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_init(uint x, uint y);
        
        [DllImport("LeuciDll.dll")]
        public static extern bool fibonacci_next();
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_current();
        
        [DllImport("LeuciDll.dll")]
        public static extern uint fibonacci_index();

        [DllImport("LeuciDll.dll")]
        public static extern bool density_init(string pdbcode, string filepath);

        [DllImport("LeuciDll.dll")]
        public static extern bool density_words(
            int w01_NC,int w02_NR, int w03_NS, int w04_Mode,    
            int w05_NCSTART,int w06_NRSTART,int w07_NSSTART,
            int w08_NX,int w09_NY, int w10_NZ,
            double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z,
            double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z,
            int w17_MAPC, int w18_MAPR,int w19_MAPS);

        [DllImport("LeuciDll.dll")]
        public static extern void density_add(double val);
        
        [DllImport("LeuciDll.dll")]
        public static extern void density_calc();

        [DllImport("LeuciDll.dll")]
        public static extern void create_slice(double cx, double cy, double cz, double lx, double ly, double lz, double px, double py, double pz, double width, double gap);
        
        [DllImport("LeuciDll.dll")]
        public static extern double get_slice_value(uint x, uint y);

        [DllImport("LeuciDll.dll")]
        public static extern double get_slice_radiant_value(uint x, uint y);

        [DllImport("LeuciDll.dll")]
        public static extern double get_slice_laplacian_value(uint x, uint y);

        public void test()
        {
            uint a = fibonacci_init(1, 4);
            bool b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
            a = fibonacci_current();
            b = fibonacci_next();
        }
        public DllInterface(double[] matrix,string pdbcode, string file_path)
        {            
            bool same = density_init(pdbcode, file_path);
            for (int i = 0; i < matrix.Length; ++i)
            {
                density_add(matrix[i]);
            }
            density_calc();
        }

        public void setWords(int w01_NC, int w02_NR, int w03_NS, int w04_Mode,
                                int w05_NCSTART, int w06_NRSTART, int w07_NSSTART,
                                int w08_NX, int w09_NY, int w10_NZ,
                                double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z,
                                double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z,
                                int w17_MAPC, int w18_MAPR, int w19_MAPS)
        {
            density_words(w01_NC, w02_NR, w03_NS, w04_Mode,
                            w05_NCSTART, w06_NRSTART, w07_NSSTART,
                            w08_NX, w09_NY, w10_NZ,
                            w11_CELLA_X, w12_CELLA_Y, w13_CELLA_Z,
                            w14_CELLB_X, w15_CELLB_Y, w16_CELLB_Z,
                            w17_MAPC, w18_MAPR, w19_MAPS);
        }

        public void createSlice(double cx, double cy, double cz, double lx, double ly, double lz, double px, double py, double pz, double width, double gap)
        {
            create_slice(cx, cy, cz, lx, ly, lz, px, py, pz, width, gap);
        }

        public double[][] getSlice(double width, double gap)
        {
            int nums = (int)(width / gap);
            double[][] slice = new double[nums][];
            for (uint i=0; i<nums;++i)
            {
                slice[i] = new double[nums];
                for (uint j = 0; j < nums; ++j)
                    slice[i][j] = get_slice_value(i, j);
            }
            return slice;
        }

        public double[][] getRadiantSlice(double width, double gap)
        {
            int nums = (int)(width / gap);
            double[][] slice = new double[nums][];
            for (uint i = 0; i < nums; ++i)
            {
                slice[i] = new double[nums];
                for (uint j = 0; j < nums; ++j)
                    slice[i][j] = get_slice_radiant_value(i, j);
            }
            return slice;
        }

        public double[][] getLaplacianSlice(double width, double gap)
        {
            int nums = (int)(width / gap);
            double[][] slice = new double[nums][];
            for (uint i = 0; i < nums; ++i)
            {
                slice[i] = new double[nums];
                for (uint j = 0; j < nums; ++j)
                    slice[i][j] = get_slice_laplacian_value(i, j);
            }
            return slice;
        }
        public double[] getAxis(double width, double gap)
        {            
            int nums = (int)(width / gap) + 1;
            double[] axis = new double[nums];
            for (uint i = 0; i < nums; ++i)            
                axis[i] = i;                            
            return axis;
        }

    }*/
    
}
