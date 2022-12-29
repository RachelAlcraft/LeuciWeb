using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace LeuciShared
{
    public class Symmetry
    {
        private int xFactor = 1;
        private int yFactor = 1;        
        private int zFactor = 1;
        private double xTranslation = 0;
        private double yTranslation = 0;
        private double zTranslation = 0;
        private int lenX = 0;
        private int lenY = 0;
        private int lenZ = 0;

        public Symmetry(int xF, int yF, int zF, double xT, double yT, double zT, int nx, int ny, int nz)
        {
            xFactor = xF;
            yFactor = yF;
            zFactor = zF;
            xTranslation = xT;
            yTranslation = yT;
            zTranslation = zT;
            lenX = nx;
            lenY = ny;
            lenZ = nz;
        }

        public VectorThree applySymmetry(VectorThree crs, bool switch_axes)
        {/*
          * Birkbeck Crystallography course by Tracey Barrett
          * https://px20.cryst.bbk.ac.uk/20core/symmall/sp19.htm
          

        VectorThree s = new VectorThree();
            s.A = crs.A * xFactor;
            s.B = crs.B * yFactor;
            s.C = crs.C * zFactor;

            s.A += lenX * xTranslation;
            s.B += lenY * yTranslation;
            s.C += lenZ * zTranslation;

            return s;
            */            
            if (switch_axes)
            {
                return new VectorThree(crs.C, crs.B, crs.A);
            }
            else
            {
                return new VectorThree(crs.A, crs.B, crs.C);
            }
        }
    }
}
