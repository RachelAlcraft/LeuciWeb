using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class PdbAtoms
    {
        private Dictionary<string, VectorThree> _atoms = new Dictionary<string, VectorThree>();        
        public PdbAtoms(string[] lines) //constructor for pdb file
        {
            _atoms.Clear();
            foreach (string line in lines)
            {
                if (line.Substring(0, 4) == "ATOM" || line.Substring(0, 6) == "HETATM")
                {                                                                                
                    //The coordinates are at fixed points in the pdb file
                    //ATOM     71  H CYS A   4      12.789   8.657  11.035  1.00  2.08           H
                    string AtomNo = line.Substring(6, 5).Trim();
                    string AtomType = line.Substring(12, 4).Trim();
                    string occupant = line.Substring(16, 1).Trim();
                    string AA = line.Substring(17, 3).Trim();
                    string Chain = line.Substring(21, 1).Trim();
                    string ResidueNo = line.Substring(22, 5).Trim();
                    string insertion = line.Substring(26, 1).Trim();
                    string strX = line.Substring(30, 8).Trim();
                    string strY = line.Substring(38, 8).Trim();
                    string strZ = line.Substring(46, 8).Trim();
                    double X = Convert.ToDouble(strX);
                    double Y = Convert.ToDouble(strY);
                    double Z = Convert.ToDouble(strZ);
                    double Occupancy = Convert.ToDouble(line.Substring(54, 6).Trim());
                    double BFactor = Convert.ToDouble(line.Substring(60, 6).Trim());
                    string Elmnt = line.Substring(66).Trim();
                    //ChimeraX style = A:5@CA
                    //Chimera "select :A:13@CZ.A"
                    if (occupant == "")
                        occupant = "A";
                    string chimera = Chain + ":" + ResidueNo + "@" + AtomType + "." + occupant;
                    _atoms.Add(chimera, new VectorThree(X,Y,Z));
                }

            }
        }
        // TODO add constructor for mmcif file

        public VectorThree getCoords(string atom)
        {
            if (_atoms.ContainsKey(atom))
                return _atoms[atom];
            if (_atoms.ContainsKey(atom + ".A"))
                return _atoms[atom + ".A"];
            return new VectorThree(0,0,0);
        }

        public string[] getFirstThreeCoords()
        {
            //the very first can be nont very string ED so get the... 3rd?
            string[] v3 = new string[3];
            int count = 0;
            int found_ca = 0;
            foreach (var atm in _atoms)
            {
                if (atm.Key.IndexOf("@C.A") > 0)
                {
                    if (found_ca > 3)
                    {
                        v3[0] = atm.Key;
                        count++;
                    }                    
                }
                else if (atm.Key.IndexOf("@CA.A") > 0)
                {
                    if (found_ca > 2)
                    {
                        v3[1] = atm.Key;
                        count++;
                    }
                    found_ca++;
                }
                else if (atm.Key.IndexOf("@O.A") > 0)
                {
                    if (found_ca > 3)
                    {
                        v3[2] = atm.Key;
                        count++;
                    }
                }
                if (count >= 3)
                    return v3;
            }
            //if we get here then it didin;t work, it could be a CA only pdb structure.
            count = 0;
            foreach (var atm in _atoms)
            {
                v3[count] = atm.Key;
                count++;
                if (count >= 3)
                    return v3;
            }            
            return v3;
        }

        public List<string> getNearAtoms(VectorThree near, double within)
        {
            List<String> ats = new List<string>();
            List<double> diss = new List<double>();
            foreach (var atm in _atoms)
            {
                double distance = near.distance(atm.Value);
                if (Math.Abs(distance) <= within)
                {
                    var index = diss.BinarySearch(distance);
                    if (index < 0) index = ~index;
                    diss.Insert(index, distance);
                    ats.Insert(index, atm.Key + ":" + Convert.ToString(Math.Round(distance, 4)));                    
                }
            }
            return ats;

        }
    }
}
