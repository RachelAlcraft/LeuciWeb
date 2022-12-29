using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * RSA Created 25 Dec 2022
 * -----Updates --------
 * ---------------------
 * This class takes the data from a given map style density matrix
 * Creates a cube of the CRS points that surround the given structure
 * Decides on a symmetry transform for each point
 * And constructs a pseudo crs matrix
 * which has XYZ and CRS directly mapped (why not)
 * And covers tighly with small buffer around the structure
 * Reports the symmetry used
 * For electron microsocpy it might just be side by side grids
 * 
 */ 

namespace LeuciShared
{
    public class AsymmetricUnit
    {
        //the dimensions of the asymmetric unit
        public VectorThree NCRS = new VectorThree();         
        // the upper and lower bounds of the asymmetric unit in xyx and crs
        private VectorThree _lXYZ = new VectorThree(1000,1000,1000);
        private VectorThree _uXYZ = new VectorThree(-1000, -1000, -1000);
        public VectorThree lCRS = new VectorThree(1000, 1000, 1000);
        public VectorThree uCRS = new VectorThree(-1000, -1000, -1000);


        public Dictionary<string, VectorThree> CrsMapping = new Dictionary<string, VectorThree>();
        private DensityMatrix _dm;
        private PdbAtoms _pa;
        public AsymmetricUnit(DensityMatrix dm, PdbAtoms pa)
        {
            _dm = dm;
            _pa = pa;
            createDimensions();
        }
        private void createDimensions()
        {
            // we want to upper and lower bounds of each of x,y and z.
            //public Dictionary<string, VectorThree> Atoms = new Dictionary<string, VectorThree>();
            foreach (var atm in _pa.Atoms)
            {
                VectorThree xyz = new VectorThree(atm.Value.A,atm.Value.B, atm.Value.C);
                _lXYZ.A = Math.Min(_lXYZ.A, xyz.A);
                _lXYZ.B = Math.Min(_lXYZ.B, xyz.B);
                _lXYZ.C = Math.Min(_lXYZ.C, xyz.C);
                _uXYZ.A = Math.Max(_uXYZ.A, xyz.A);
                _uXYZ.B = Math.Max(_uXYZ.B, xyz.B);
                _uXYZ.C = Math.Max(_uXYZ.C, xyz.C);

                VectorThree crs = _dm.getCRSFromXYZ(xyz);
                lCRS.A = (int)Math.Min(lCRS.A, crs.A);
                lCRS.B = (int)Math.Min(lCRS.B, crs.B);
                lCRS.C = (int)Math.Min(lCRS.C, crs.C);
                uCRS.A = (int)Math.Max(uCRS.A, crs.A);
                uCRS.B = (int)Math.Max(uCRS.B, crs.B);
                uCRS.C = (int)Math.Max(uCRS.C, crs.C);
            }
            // ad CRS buffer zone
            int bfr = 0;
            lCRS -= new VectorThree(bfr, bfr, bfr);
            uCRS += new VectorThree(bfr, bfr, bfr);

            NCRS = uCRS - lCRS;

            // loop through the CRS values and create the mapping
            for (int c = (int)lCRS.A; c <= uCRS.A; ++c)
            {
                for (int r = (int)lCRS.B; r <= uCRS.B; ++r)
                {
                    for (int s = (int)lCRS.C; s <= uCRS.C; ++s)
                    {
                        VectorThree crs = new VectorThree(c, r, s);
                        // TODO
                        // now we do a symmetry operation to convert it into a crs that is in the given data
                        // TODO
                        string key = crs.getKey(0);
                        CrsMapping[key] = crs;
                    }
                }
            }            
        }
        
    }

    
}
