using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class EdInterface
    {
        public EdInterface()
        {

        }
        public bool density_init(string pdbcode, string filepath)
        {
            return true; 
        }
        public bool density_words(
            int w01_NC, int w02_NR, int w03_NS, int w04_Mode,
            int w05_NCSTART, int w06_NRSTART, int w07_NSSTART,
            int w08_NX, int w09_NY, int w10_NZ,
            double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z,
            double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z,
            int w17_MAPC, int w18_MAPR, int w19_MAPS)
        {
            return true;
        }
        public void density_add(double val)
        {

        }
        public void density_calc()
        {

        }
        public void create_slice(double cx, double cy, double cz,
                                double lx, double ly, double lz,
                                double px, double py, double pz,
                                double width, double gap)
        {

        }
        public double get_slice_value(uint x, uint y)
        {
            return 0;
        }
        public double get_slice_radiant_value(uint x, uint y)
        {
            return 0;
        }
        public double get_slice_laplacian_value(uint x, uint y)
        {
            return 0;
        }
    }
}
