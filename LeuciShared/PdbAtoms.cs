using System.Numerics;
using System.Security.Cryptography;

namespace LeuciShared
{
    public class PdbAtoms
    {
        public Dictionary<string, VectorThree> Atoms = new Dictionary<string, VectorThree>();
        public Dictionary<string, string> Lines = new Dictionary<string, string>();
        private List<string> Residues = new List<string>();
        private Dictionary<string, string> _aas = new Dictionary<string, string>();
        public bool Init = false;
        public VectorThree LowerCoords = new VectorThree(1000,1000,1000);
        public VectorThree UpperCoords = new VectorThree(-1000,-1000,-1000);
        public PdbAtoms(string[] lines) //constructor for pdb file
        {
            Atoms.Clear();
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
                    Atoms.Add(chimera, new VectorThree(X, Y, Z));
                    Lines.Add(chimera, line);
                    _aas.Add(chimera, AA);
                    LowerCoords.A = Math.Min(LowerCoords.A, X);
                    LowerCoords.B = Math.Min(LowerCoords.B, Y);
                    LowerCoords.C = Math.Min(LowerCoords.C, Z);
                    UpperCoords.A = Math.Max(UpperCoords.A, X);
                    UpperCoords.B = Math.Max(UpperCoords.B, Y);
                    UpperCoords.C = Math.Max(UpperCoords.C, Z);

                    string rid = Chain + ":" + ResidueNo;
                    if (!Residues.Contains(rid))                   
                        Residues.Add(rid);                    
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
            if (Atoms.ContainsKey(newatom))
                return newatom;
            if (Atoms.ContainsKey(newatom + ".A"))
                return newatom;

            return atom;
        }
        public VectorThree getCoords(string atom)
        {
            if (Atoms.ContainsKey(atom))
                return Atoms[atom];
            if (Atoms.ContainsKey(atom + ".A"))
                return Atoms[atom + ".A"];
            return new VectorThree(0, 0, 0);
        }

        public string getAA(string atom)
        {
            if (_aas.ContainsKey(atom))
                return _aas[atom];
            if (_aas.ContainsKey(atom + ".A"))
                return _aas[atom + ".A"];
            return "";
        }

        public string getLine(string atom)
        {
            if (Lines.ContainsKey(atom))
                return Lines[atom];
            if (Lines.ContainsKey(atom + ".A"))
                return Lines[atom + ".A"];
            return "";
        }

        public string[] getFirstThreeCoords()
        {
            //the very first can be nont very string ED so get the... 3rd?
            string[] v3 = new string[3];
            int count = 0;
            int found_ca = 0;
            foreach (var atm in Atoms)
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
            foreach (var atm in Atoms)
            {
                v3[count] = atm.Key;
                count++;
                if (count >= 3)
                    return v3;
            }
            return v3;
        }

        public List<string> getNearAtoms(VectorThree near, double hover_min, double hover_max)
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
                    foreach (var atm in Atoms)
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
                        for (int i = 1; i < diss.Count; ++i)
                        {
                            if (diss[i] > hover_min)
                                ats_v2.Add(ats[i]);
                        }
                        ats = ats_v2;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return ats;

        }                
        public List<string[]> getMatchesMotif(string motif, out List<string[]> lines,out List<double[]> disses, out List<VectorThree[]> coords)
        {/*This tries to do something similar to my python library and follow some rules to get matches
          * How I wish I had documented it!!!!
          * 
          * geoA = 'SG:{SG}+1'
            geoB = 'SG:{N}+2'
            geoC = 'SG:{SG@1}'
            geoD = 'SG:{SG@2}'
            geoE = 'SG:SG'
            (O) means element not atom type

            Redoing it for this as it does not have the same requirements or interface
            {e:O,a:O,d:2-3,n:1,r:1,o:0,aa:} # element or atom type, distance cap, nearest number, residue offset, residues away, amino acid it needs to be
            C:CA:O = {a:C}{a:CA}{a:O}
            C:CA:N+1 = {a:C}{a:CA}{a:N,r:1}
            SG:{N}+2 = {a:SG}{e:N,o:2} SGs to the nearest atom of type N that is at least 2 residues away
            {e.N}{e.O,d:2-3}

            They are both relative to the first one, not chained
          */            
            string[] mtfs = motif.Split("}");            
            string anchor_motif = mtfs[0].Substring(1);
            MotifMatch mtf_anchor = new MotifMatch(anchor_motif);
            
            List<string[]> motifs = new List<string[]>();
            lines = new List<string[]>();
            disses = new List<double[]>();
            coords = new List<VectorThree[]>();
            List<MotifMatch> motif_matches = new List<MotifMatch>(); //these are all the criteria to apply to first atoms
            for (int m = 1; m < mtfs.Length; ++m)
            {
                if (mtfs[m].Length > 5)
                {
                    string other_motif = mtfs[m].Substring(1);
                    MotifMatch mtf_other = new MotifMatch(other_motif);
                    motif_matches.Add(mtf_other);
                }
            }

            List<string> atm_anchors = new List<string>(); //this is the list of all the atoms that match the first criteria
            foreach (var atm in Atoms)
            {
                string line = Lines[atm.Key];
                Atom a = new Atom(line);
                if (mtf_anchor.match(a))
                    atm_anchors.Add(atm.Key);
            }

            
            foreach (string a_key in atm_anchors)
            {
                List<string> key_matches = new List<string>();
                List<string> line_matches = new List<string>();
                List<double> distance_matches = new List<double>();
                List<VectorThree> coord_matches = new List<VectorThree>();

                string line = Lines[a_key];
                Atom a = new Atom(line);
                key_matches.Add(a_key);
                line_matches.Add(line);
                distance_matches.Add(0);
                coord_matches.Add(a.coords());
                foreach (MotifMatch mm in motif_matches)
                {
                    List<string> atoms_for_motif = new List<string>();
                    List<string> lines_for_motif = new List<string>();
                    List<Atom> atms_for_motif = new List<Atom>();
                    List<double> distances_for_motif = new List<double>();

                    foreach (var atm2 in Atoms)
                    {
                        string line2 = Lines[atm2.Key];
                        Atom a2 = new Atom(line2);
                        if (mm.matchPair(a, a2))
                        {
                            atoms_for_motif.Add(atm2.Key);
                            lines_for_motif.Add(line2);
                            atms_for_motif.Add(a2);
                            distances_for_motif.Add(a2.distance(a));
                        }
                    }                    
                    if (atoms_for_motif.Count > 0)
                    {// at this point we need to do the distance check
                        string mkey = atoms_for_motif[0];
                        string mline = lines_for_motif[0];
                        double distance = distances_for_motif[0];
                        Atom matom = atms_for_motif[0];
                        if (atoms_for_motif.Count > 1)
                        {
                            if (mm.distanceSearch(a, atms_for_motif, atoms_for_motif, lines_for_motif, out mkey, out mline, out distance, out matom))
                            {
                                key_matches.Add(mkey);
                                line_matches.Add(mline);
                                distance_matches.Add(distance);
                                coord_matches.Add(matom.coords());
                            }
                        }
                        else
                        {
                            key_matches.Add(mkey);
                            line_matches.Add(mline);
                            distance_matches.Add(distance);
                            coord_matches.Add(matom.coords());
                        }
                    }
                                        
                }
                if (key_matches.Count == motif_matches.Count+1)
                {                    
                    string[] a_motif = new string[key_matches.Count];
                    string[] a_line = new string[key_matches.Count];
                    double[] a_dis = new double[key_matches.Count];
                    VectorThree[] a_coord = new VectorThree[key_matches.Count];
                    for (int i = 0; i < key_matches.Count; ++i)
                    {
                        a_motif[i] = key_matches[i];
                        a_line[i] = line_matches[i];
                        a_dis[i] = distance_matches[i];
                        a_coord[i] = coord_matches[i];
                    }
                    motifs.Add(a_motif);
                    lines.Add(a_line);
                    disses.Add(a_dis);
                    coords.Add(a_coord);
                }
                else
                {
                    
                }
            }
            return motifs;
        }
        public List<string[]> getMatchMotif(string motif, string exclusions, string inclusions, out List<VectorThree[]> coords, out List<string> lines)
        {
            // out init
            lines = new List<string>();
            coords = new List<VectorThree[]>();
            //the very first can be nont very string ED so get the... 3rd?
            List<string[]> motifs = new List<string[]>();            
            string[] mtf = motif.Split(":");
            int[] offs = new int[3];
            for (int i = 0; i < mtf.Length; ++i)
            {
                string mf = mtf[i];
                int offset = 0;
                if (mf.Contains("+"))
                {
                    string[] mm = mf.Split("+");
                    mf = mm[0];
                    offset = Convert.ToInt32(mm[1]);
                    mtf[i] = mf;
                }
                else if (mf.Contains("-"))
                {
                    string[] mm = mf.Split("-");
                    mf = mm[0];
                    offset = -1 * Convert.ToInt32(mm[1]);
                    mtf[i] = mf;
                }
                offs[i] = offset;
            }

            int current_rid = -1;
            int found = -1;
            if (exclusions == null)
                exclusions = "";
            exclusions += ",";
            if (inclusions == null)
                inclusions = "";
            inclusions += ",";
            foreach (var ridx in Residues)
            {
                if (!exclusions.Contains(ridx + ","))
                {
                    if (inclusions == "," || inclusions.Contains(ridx + ","))
                    {
                        string[] spl = ridx.Split(":");
                        string ch = spl[0];
                        int rida = Convert.ToInt32(spl[1]);
                        int rid0 = rida + offs[0];
                        int rid1 = rida + offs[1];
                        int rid2 = rida + offs[2];
                        string candiate0 = ch + ":" + rid0 + "@" + mtf[0] + ".A";
                        string candiate1 = ch + ":" + rid1 + "@" + mtf[1] + ".A";
                        string candiate2 = ch + ":" + rid2 + "@" + mtf[2] + ".A";
                        if (Atoms.ContainsKey(candiate0) && Atoms.ContainsKey(candiate1) && Atoms.ContainsKey(candiate2))
                        {
                            string[] match = new string[3];
                            match[0] = candiate0;
                            match[1] = candiate1;
                            match[2] = candiate2;
                            motifs.Add(match);
                            VectorThree[] v3 = new VectorThree[3];
                            v3[0] = getCoords(candiate0);
                            v3[1] = getCoords(candiate1);
                            v3[2] = getCoords(candiate2);
                            coords.Add(v3);
                            lines.Add(getLine(candiate0));
                            lines.Add(getLine(candiate1));
                            lines.Add(getLine(candiate2));
                            lines.Add(getLine(""));
                        }
                    }
                }                                
            }
            return motifs;            
        }
    }
}
