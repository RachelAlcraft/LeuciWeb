using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        
    }
}
