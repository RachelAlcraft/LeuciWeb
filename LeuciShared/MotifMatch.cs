using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class MotifMatch
    {
        private string _motif;
        public MotifMatch(string motif)
        {
            _motif = motif;
        }
        public bool match(Atom atm)
        {
            //comma delim list of key:value pairs
            // element or atom type, distance cap, nearest number, residue offset, residues away, amino acid it needs to be
            /*
             * element:O,atom:O,distance:2-3,exclude:1,offset:1,away:0,aa:
             */
            bool matches = false;
            string[] mtfs = _motif.Split(",");
            string atom_find = "";
            foreach (string mtf in mtfs)
            {
                string[] kv = mtf.Split(":");
                if (kv[0].ToLower() == "atom")
                {
                    matches = kv[1].ToUpper() == atm.AtomType;
                    if (!matches)
                        return false;

                }
                else if (kv[0].ToLower() == "element")
                {
                    matches = kv[1].ToUpper() == atm.Element;
                    if (!matches)
                        return false;
                }
                else if (kv[0].ToLower() == "aa")
                {
                    matches = kv[1].ToUpper() == atm.AA;
                    if (!matches)
                        return false;
                }
            }
            return matches;
        }
        public bool matchPair(Atom atm1, Atom atm2)
        {
            //comma delim list of key:value pairs
            // element or atom type, distance cap, nearest number, residue offset, residues away, amino acid it needs to be
            /*
             * element:O,atom:O,distance:2-3,exclude:1,offset:1,away:0,aa:
             */            
            bool matches = false;
            string[] mtfs = _motif.Split(",");            
            string atom_find = "";
            //The chain always needs to be the same
            if (atm1.Chain != atm2.Chain)
                return false;
            foreach (string mtf in mtfs)
            {
                string[] kv = mtf.Split(":");                
                if (kv[0].ToLower() == "atom")
                {
                    matches = kv[1].ToUpper() == atm2.AtomType;
                    if (!matches)
                        return false;
                }
                else if (kv[0].ToLower() == "element")
                {
                    matches = kv[1].ToUpper() == atm2.Element;
                    if (!matches)
                        return false;
                }
                else if (kv[0].ToLower() == "aa")
                {                    
                    matches = kv[1].ToUpper() == atm2.AA;
                    if (!matches)
                        return false;
                }
                else if (kv[0].ToLower() == "offset")
                {
                    int off = Convert.ToInt32(kv[1]);
                    int res_diff = Convert.ToInt32(atm2.ResidueNo) - Convert.ToInt32(atm1.ResidueNo);                   
                    matches = res_diff == off;
                    if (!matches)
                        return false;
                }                
            }
            return matches;
        }

        public bool distanceSearch(Atom a, List<Atom> atoms, List<string> keys, List<string> lines, out string mkey, out string mline)
        {
            List<string> newkeys = new List<string>();
            List<string> newlines = new List<string>();
            List<double> disses = new List<double>();
            List<Atom> newatms = new List<Atom>();
            mkey = "";
            mline = "";
            int number_list = 0;
            for (int i = 0; i < atoms.Count; ++i)
            {
                Atom atm = atoms[i];
                double distance = a.distance(atm);

                string[] mtfs = _motif.Split(",");

                bool can_use = true;
                foreach (string mtf in mtfs)
                {
                    string[] kv = mtf.Split(":");
                    
                    if (kv[0].ToLower() == "lower")
                    {
                        double lower_dis = Convert.ToDouble(kv[1]);
                        if (lower_dis <= distance)
                            can_use = false;
                    }
                    else if (kv[0].ToLower() == "upper")
                    {
                        double upper_dis = Convert.ToDouble(kv[1]);
                        if (upper_dis >= distance)
                            can_use = false;
                    }
                    else if (kv[0].ToLower() == "position")
                    {
                        number_list = Convert.ToInt32(kv[1]);
                        
                    }
                }

                if (can_use)
                {
                    var index = disses.BinarySearch(distance);
                    if (index < 0) index = ~index;
                    disses.Insert(index, distance);
                    newkeys.Insert(index, keys[i]);
                    newlines.Insert(index, lines[i]);
                    newatms.Insert(index, atoms[i]);
                }
            }
            if (number_list < newkeys.Count)
            {
                mkey = newkeys[number_list];
                mline = newlines[number_list];
                return true;
            }
            return false;
        }        
    }
}
