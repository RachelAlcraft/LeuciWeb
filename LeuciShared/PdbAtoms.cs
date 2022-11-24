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
        private Dictionary<string, string> _aas = new Dictionary<string, string>();
        public bool Init = false;
        public PdbAtoms(string[] lines) //constructor for pdb file
        {
            _atoms.Clear();
            Init = true;
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
                    _aas.Add(chimera, AA);
                }

            }
        }
        // TODO add constructor for mmcif file

        public string getIncAtom(string atom, int offset)
        {
            // atom is in the format A:710@C.A
            string[] beg = atom.Split(":");
            string[] pst = beg[1].Split("@");
            int atm = Convert.ToInt32(pst[0]);
            atm += offset;
            string newatom = beg[0] + ":" + Convert.ToString(atm) + "@" + pst[1];
            if (_atoms.ContainsKey(newatom))
                return newatom;
            if (_atoms.ContainsKey(newatom + ".A"))
                return newatom;

            return atom;
        }
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

        public List<string> getNearAtoms(VectorThree near, double hover_min,double hover_max)
        {
            // max -1 means none
            // ax and min of 0 mins nearest only
            // a range means just that, but ALSO the nearest.
            
            List<String> ats = new List<string>();
            List<double> diss = new List<double>();
            try
            {
                if (hover_max > -1) // none
                {
                    foreach (var atm in _atoms)
                    {
                        double distance = near.distance(atm.Value);
                        if (Math.Abs(distance) <= hover_max || hover_max == 0)
                        {                                                        
                            var index = diss.BinarySearch(distance);
                            if (index < 0) index = ~index;
                            diss.Insert(index, distance);
                            string aa = _aas[atm.Key];
                            ats.Insert(index, aa + " " + atm.Key + "=" + Convert.ToString(Math.Round(distance, 4)));                            
                        }
                    }
                    if (hover_max == 0 && hover_min == 0)//nearest only
                    {
                        string na = ats[0];
                        ats.Clear();
                        ats.Add(na);
                    }
                    else if (hover_min > 0 && diss.Count > 1)
                    {
                        List<String> ats_v2 = new List<string>();
                        ats_v2.Add(ats[0]);
                        for(int i=1; i < diss.Count; ++i)
                        {
                            if (diss[i] > hover_min)
                                ats_v2.Add(ats[i]);
                        }
                        ats = ats_v2;
                    }
                }
            }
            catch(Exception e)
            {

            }
            return ats;

        }
    }
}
