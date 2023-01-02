using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class Atom
    {
        public string Line;
        public string AtomNo;
        public string AtomType;
        public string AA;
        public string Chain;
        public string ResidueNo;
        public double X;
        public double Y;
        public double Z;
        public double Occupancy;
        public double BFactor;
        public string Element;
        public Atom(string line)
        {            
            Line = line;            
            //The coordinates are at fixed points in the pdb file
            //ATOM     71  H CYS A   4      12.789   8.657  11.035  1.00  2.08           H
            AtomNo = line.Substring(6, 5).Trim();
            AtomType = line.Substring(12, 4).Trim();
            string occupant = line.Substring(16, 1).Trim();
            AA = line.Substring(17, 3).Trim();
            Chain = line.Substring(21, 1).Trim();
            ResidueNo = line.Substring(22, 5).Trim();
            string insertion = line.Substring(26, 1).Trim();
            string strX = line.Substring(30, 8).Trim();
            string strY = line.Substring(38, 8).Trim();
            string strZ = line.Substring(46, 8).Trim();
            X = Convert.ToDouble(strX);
            Y = Convert.ToDouble(strY);
            Z = Convert.ToDouble(strZ);
            Occupancy = Convert.ToDouble(line.Substring(54, 6).Trim());
            BFactor = Convert.ToDouble(line.Substring(60, 6).Trim());
            Element = line.Substring(66).Trim();            
        }
    }
}
