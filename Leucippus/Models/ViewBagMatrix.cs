

using LeuciShared;
using Microsoft.Build.Framework;

namespace Leucippus.Models
{
    public sealed class ViewBagMatrix
    {
        // https://csharpindepth.com/Articles/Singleton 4th version implementation of Singleton

        private static readonly ViewBagMatrix instance = new ViewBagMatrix();
        static ViewBagMatrix()
        {
        }
        private ViewBagMatrix()
        {
            PdbCode = "6eex";
            //FileDownloads fd = new FileDownloads("6eex");
            //fd.downloadAll();
            //EmCode = fd.EmCode;
            //DensityType = fd.DensityType;
        }

        private void incRefresh()
        {
            ++_refresh;
        }

        public static ViewBagMatrix Instance
        {
            get
            {
                return instance;
            }
        }

        private int _refresh = 1;
        public bool Refresh
        {
            get
            {
                return _refresh > 0;
            }
        }

        public void Reset()
        {
            _refresh = 0;
        }

        public string T1Display { get; set; } = "block";
        public string T2Display { get; set; } = "none";
        public string T3Display { get; set; } = "none";
        public string T4Display { get; set; } = "none";
        public string T5Display { get; set; } = "none";

        private string _pdbcode = "6eex";
        public string PdbCode
        {
            get { return _pdbcode; }
            set
            {
                if (value != "")
                {
                    if (value != _pdbcode)
                    {
                        incRefresh();
                        _interp = "LINEAR";
                        //FileDownloads fd = new FileDownloads(value);
                        //fd.downloadAll();
                        //EmCode = fd.EmCode;
                        //DensityType = fd.DensityType;
                    }
                    _pdbcode = value;
                }
                else if (_pdbcode != "6eex")
                {
                    _pdbcode = "6eex";
                    incRefresh();
                }
            }
        }
        private string _interp = "LINEAR";
        public string Interp
        {
            get { return _interp; }
            set
            {
                if (value == _interp.ToUpper())
                {
                    //do nothing
                }
                //else if (value == "" && DensityType == "cryo-em")
                //{
                //    _interp = "LINEAR";
                //    incRefresh();
                //}
                else if (value != "")
                {
                    incRefresh();
                    _interp = value.ToUpper();
                }
                else
                {// we have lost the interp so something has changed
                    _interp = "LINEAR";
                    incRefresh();
                }
            }
        }

        public int Fos = 2;
        public int Fcs = -1;
        public void setFoFc(int fos, int fcs)
        {
            if (fos != Fos)
                incRefresh();
            Fos = fos;
            if (fcs != Fcs)
                incRefresh();
            Fcs = fcs;
        }

        public string YellowDots { get; set; } = "checked";
        public string GreenDots { get; set; } = "checked";

        private double _hover_min = 0;
        public double HoverMin
        {
            get
            {
                return _hover_min;
            }
            set
            {
                if (value != _hover_min)
                {
                    _hover_min = value;
                    incRefresh();
                }
            }
        }
        private double _hover_max = 0;
        public double HoverMax
        {
            get
            {
                return _hover_max;
            }
            set
            {
                if (value != _hover_max)
                {
                    _hover_max = value;
                    incRefresh();
                }
            }
        }
        private double _nav_distance = 0.1;
        public double NavDistance
        {
            get
            {
                return _nav_distance;
            }
            set
            {
                if (value != _nav_distance)
                {
                    _nav_distance = value;
                    incRefresh();
                }
            }
        }
        public string EmCode
        {
            get; set;
        }
        public string DensityType
        {
            get; set;
        }

        public string EbiLink
        {
            get { return "https://www.ebi.ac.uk/pdbe/entry/pdb/" + PdbCode; }

        }
        private string _info = "";
        public string Info
        {
            get { return _info; }
            set
            {
                if (value != "")
                    _info = value;
            }
        }

        private string _plane = "XY";
        public string Plane
        {
            get { return _plane; }
            set
            {
                if (value != "")
                {
                    if (value != _plane)
                        incRefresh();
                    _plane = value;
                }
            }
        }
        private int _layer = 0;
        public int Layer
        {
            get { return _layer; }
            set
            {
                if (value != -1)
                {
                    if (value != _layer)
                        incRefresh();
                    _layer = value;
                }
            }
        }

        private string getNextColour(string colour)
        {
            string retcolour = "BlackWhite";

            if (colour == "RedBlueGrey")
                retcolour = "BlackWhite";
            else if (colour == "BlackWhite")
                retcolour = "RedBlueZero";
            else if (colour == "RedBlueZero")
                retcolour = "RedBlueGrey";
            //else if (_denhue == "RedBlue")
            //    retcolour = "RedBlueGrey";

            return retcolour;
        }

        private string _denplot = "contour";
        public string DenPlot
        {
            get { return _denplot; }
            set
            {
                if (value == "+1")
                {
                    if (_denplot == "contour")
                        _denplot = "heatmap";
                    else
                        _denplot = "contour";
                }
                else if (value != "")
                {
                    _denplot = value;
                }

            }
        }
        private string _denhue = "RedBlueGrey";
        public string DenHue
        {
            get { return _denhue; }
            set
            {
                if (value == "+1")
                    _denhue = getNextColour(_denhue);
                else if (value != "")
                    _denhue = value;
            }
        }
        private string _radplot = "contour";
        public string RadPlot
        {
            get { return _radplot; }
            set
            {
                if (value == "+1")
                {
                    if (_radplot == "contour")
                        _radplot = "heatmap";
                    else
                        _radplot = "contour";
                }
                else if (value != "")
                {
                    _radplot = value;
                }

            }
        }
        public bool IsSD { get { return _valsd == "st.dev."; } }
        private string _valsd = "st.dev.";
        public string ValSd
        {
            get { return _valsd; }
            set
            {
                if (value == "+1")
                {
                    if (_valsd == "st.dev.")
                        _valsd = "∝ electrons/cube";
                    else
                        _valsd = "st.dev.";
                }
                else if (value == "s.d.")
                    _valsd = "st.dev.";
                else if (value == "electrons")
                    _valsd = "∝ electrons/cube";

            }
        }
        private double _sdcap = 3.0;
        public double SdCap
        {
            get { return _sdcap; }
            set
            {
                if (value != -100)
                {
                    _sdcap = Math.Max(0, value);
                }
            }
        }
        private double _sdfloor = -1.0;
        public double SdFloor
        {
            get { return _sdfloor; }
            set
            {
                if (value != -100)
                {
                    _sdfloor = Math.Min(0, value);
                }
            }
        }

        private string _radhue = "BlackWhite";
        public string RadHue
        {
            get { return _radhue; }
            set
            {
                if (value == "+1")
                    _radhue = getNextColour(_radhue);
                else if (value != "")
                    _radhue = value;
            }
        }
        private string _lapplot = "contour";
        public string LapPlot
        {
            get { return _lapplot; }
            set
            {
                if (value == "+1")
                {
                    if (_lapplot == "contour")
                        _lapplot = "heatmap";
                    else
                        _lapplot = "contour";
                }
                else if (value != "")
                {
                    _lapplot = value;
                }
            }
        }
        private string _laphue = "RedBlueZero";
        public string LapHue
        {
            get { return _laphue; }
            set
            {
                if (value == "+1")
                    _laphue = getNextColour(_laphue);
                else if (value != "")
                    _laphue = value;
            }
        }
        private string _denbar = "N";
        public string DenBar
        {
            get { return _denbar; }
            set
            {
                if (value == "+1")
                {
                    if (_denbar == "N")
                        _denbar = "Y";
                    else
                        _denbar = "N";
                }
                else
                {
                    _denbar = value;
                }
            }

        }
        public bool IsDenBar
        {
            get
            {
                return _denbar == "Y";
            }
        }
        private string _radbar = "N";
        public string RadBar
        {
            get { return _radbar; }
            set
            {
                if (value == "+1")
                {
                    if (_radbar == "N")
                        _radbar = "Y";
                    else
                        _radbar = "N";
                }
                else
                {
                    _radbar = value;
                }
            }
        }
        public bool IsRadBar
        {
            get
            {
                return _radbar == "Y";
            }
        }
        private string _lapbar = "N";
        public string LapBar
        {
            get { return _lapbar; }
            set
            {
                if (value == "+1")
                {
                    if (_lapbar == "N")
                        _lapbar = "Y";
                    else
                        _lapbar = "N";
                }
                else
                {
                    _lapbar = value;
                }


            }
        }
        public bool IsLapBar
        {
            get
            {
                return _lapbar == "Y";
            }
        }
        private string _planeplot = "heatmap";
        public string PlanePlot
        {
            get { return _planeplot; }
            set
            {
                if (value == "+1")
                {
                    if (_planeplot == "contour")
                        _planeplot = "heatmap";
                    else
                        _planeplot = "contour";
                }
                else if (value != "")
                {
                    _planeplot = value;
                }

            }
        }

        private int _samples = 100;
        public int Samples
        {
            get { return _samples; }
            set
            {
                //don't maintain ratio when increasing the gap                
                if (value == -2) // this means increase focus
                {
                    _samples += 5;                    
                    incRefresh();
                }
                else if (value == -3)
                {
                    _samples -= 5;                    
                    incRefresh();
                }
                else if (value == -1) // this means go back to default
                {
                    _samples = 100;
                }
                else if (value != _samples)
                {
                    _samples = value;
                    incRefresh();
                }
                if (_samples > 200)
                {
                    _samples = 200;
                    incRefresh();
                }
                if (_samples < 1)
                {
                    _samples = 1;
                    incRefresh();
                }                
            }
        }
        private double _width = 6.0;
        public double Width
        {
            get { return _width; }
            set
            {
                //maintain ratio
                //double nums = _width * _gap;
                if (value == -2) // this means increase by 0.5
                {
                    _width += 0.5;
                    //_gap = nums / _width;
                    incRefresh();
                }
                else if (value == -3) //this means decrease by 0.5
                {
                    _width -= 0.5;
                    if (_width <= 0.5)
                        _width = 0.5;
                    //_gap = nums / _width;
                    incRefresh();
                }
                else if (value == -1) // this means go back to default
                {
                    _width = 6.0;
                }
                else if (Math.Round(value, 4) != Math.Round(_width, 4))
                {
                    //double aspectRatio = _width * _gap;
                    _width = value;
                    //_gap = _width / aspectRatio;
                    incRefresh();
                }
                //if (_width / _gap > 110)
                //{
                //    _gap = _width / 110;
                //    incRefresh();
                //}
                _width = Math.Round(_width, 4);
                //_gap = Math.Round(_gap, 4);
            }
        }
        // Handle setting the central-linear-planar
        public VectorThree CentralPosVector = new VectorThree(-1, -1, -1);
        private string _cxyz = "(-1,-1,-1)";
        public VectorThree CAtomStrucVector = new VectorThree(-1, -1, -1);
        public string CAA = "";
        public string CentralAtomStrucString = "A:1@C";
        public double CDistance = 0;
        public void SetCentral(string cxyz, string ca, PdbAtoms pdba, int atom_offset, bool refresh = true)
        {
            SetAtom(cxyz, ca, pdba, 0, atom_offset, ref CentralPosVector, ref CAtomStrucVector, ref CentralAtomStrucString, ref _cxyz, ref CDistance, ref CAA);
            /*
            if (cxyz == null)
                cxyz = "";
            if (ca == null)
                ca = "";
            // TODO we need to figure out the distance between the atom and the points and decide if it is the same, return it if so or blank if not
            // TODO and we need to know which has changed and which has not
            if (refresh)
            {
                CentralAtom = pdba.getFirstThreeCoords()[0];
                Central = pdba.getCoords(CentralAtom);                
                CAtom = new VectorThree(Central.A, Central.B, Central.C);
                CDistance = 0;
                incRefresh();
            }
            else if (Central.A + Central.B + Central.C == -3)
            {
                Central = pdba.getCoords(CentralAtom);
                CAtom = new VectorThree(Central.A, Central.B, Central.C);
                CDistance = 0;
                incRefresh();
            }
            // if we pass only ca in then refresh
            if (cxyz != "" && cxyz != null)
            {
                if (cxyz != _cxyz)
                {
                    incRefresh();
                    //unwrap
                    string[] xyzs = cxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    if (ca != "")
                    {
                        CentralAtom = pdba.getIncAtom(ca, atom_offset);
                        CAtom = new VectorThree(Central.A, Central.B, Central.C);
                    }
                    CDistance = Math.Round(CAtom.distance(new VectorThree(x, y, z)), 3);
                    Central = new VectorThree(x, y, z);
                    _cxyz = cxyz;
                }
                


            }
            else if (ca != "")
            {
                if (ca!= CentralAtom || cxyz == "" || atom_offset != 0)
                {
                    CentralAtom = pdba.getIncAtom(ca,atom_offset);
                    Central = pdba.getCoords(CentralAtom);
                    CAtom = new VectorThree(Central.A, Central.B, Central.C);
                    CDistance = 0;
                    incRefresh();
                }
            }                      
            else
            {

            }*/
        }
        public VectorThree LinearPosVector = new VectorThree(-1, -1, -1);
        private string _lxyz = "(-1,-1,-1)";
        public VectorThree LAtomStrucVector = new VectorThree(-1, -1, -1);
        public string LinearAtomStrucString = "A:1@O";
        public string LAA = "";
        public double LDistance = 0;
        public void SetLinear(string lxyz, string la, PdbAtoms pdba, int atom_offset, bool refresh = true)
        {
            SetAtom(lxyz, la, pdba, 1, atom_offset, ref LinearPosVector, ref LAtomStrucVector, ref LinearAtomStrucString, ref _lxyz, ref LDistance,ref LAA);
            /*if (lxyz == null)
                lxyz = "";
            if (la == null)
                la = "";
            if (refresh)
            {
                LinearAtom = pdba.getFirstThreeCoords()[1];
                Linear = pdba.getCoords(LinearAtom);
                LAtom = new VectorThree(Linear.A, Linear.B, Linear.C);
                LDistance = 0;
                incRefresh();
            }
            else if (Linear.A + Linear.B + Linear.C == -3)
            {
                Linear = pdba.getCoords(LinearAtom);
                LAtom = new VectorThree(Linear.A, Linear.B, Linear.C);
                LDistance = 0;
                incRefresh();
            }
            if (lxyz != "" && lxyz != null)
            {
                if (lxyz != _lxyz)
                {
                    //unwrap
                    string[] xyzs = lxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    LDistance = Math.Round(LAtom.distance(new VectorThree(x, y, z)), 3);
                    if (la != "")
                    {
                        LinearAtom = pdba.getIncAtom(la, atom_offset);
                        LAtom = new VectorThree(Linear.A, Linear.B, Linear.C);
                    }
                    else
                    {
                        Linear = new VectorThree(x, y, z);
                    }
                    _lxyz = lxyz;
                    incRefresh();
                }
            }
            else if (la != "")
            {
                if (la != LinearAtom || lxyz == "" || atom_offset!=0)
                {
                    LinearAtom = pdba.getIncAtom(la, atom_offset);                    
                    Linear = pdba.getCoords(LinearAtom);
                    LAtom = new VectorThree(Linear.A, Linear.B, Linear.C);
                    LDistance = 0;
                    incRefresh();
                }
            } */
        }

        public VectorThree PlanarPosVector = new VectorThree(-1, -1, -1);
        public VectorThree PAtomStrucVector = new VectorThree(-1, -1, -1);
        private string _pxyz = "(-1,-1,-1)";
        public string PlanarAtomStrucString = "A:2@N";
        public string PAA = "";
        public double PDistance = 0;

        private void SetAtom(
            string xyz, string at, PdbAtoms pdba, int pos, int atom_offset,
            ref VectorThree PosVec, ref VectorThree StrucVec, ref string atom, ref string coords, ref double distance,
            ref string AA)
        {
            if (xyz == null)
                xyz = "";
            if (at == null)
                at = "";
            if (at == "" && xyz == "")
            {
                atom = pdba.getFirstThreeCoords()[pos];
                PosVec = pdba.getCoords(atom);
                StrucVec = pdba.getCoords(atom);
                distance = 0;
                incRefresh();
            }
            if (at != "") //we want to set the atoms, we may not want to use them
            {
                if (at != atom || atom_offset != 0)
                {
                    atom = pdba.getIncAtom(at, atom_offset);
                    StrucVec = pdba.getCoords(atom);                    
                    incRefresh();
                }
            }
            if (xyz != "")
            {
                if (xyz != coords)
                {
                    //unwrap
                    string[] xyzs = xyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    PosVec = new VectorThree(x, y, z);
                    incRefresh();
                    coords = xyz;
                }
            }
            else if (at != "") // we set them up already so now move them to the positions
            {
                PosVec = new VectorThree(StrucVec.A, StrucVec.B, StrucVec.C);
                incRefresh();
            }
            AA = pdba.getAA(atom);
            distance = Math.Round(PosVec.distance(StrucVec), 3);
        }
        public void SetPlanar(string pxyz, string pa, PdbAtoms pdba, int atom_offset)
        {
            SetAtom(pxyz, pa, pdba, 2, atom_offset, ref PlanarPosVector, ref PAtomStrucVector, ref PlanarAtomStrucString, ref _pxyz, ref PDistance,ref PAA);
            /*if (pxyz == null)
                pxyz = "";
            if (pa == null)
                pa = "";
            if (pa == "" && pxyz == "")
            {
                PlanarAtomStrucString = pdba.getFirstThreeCoords()[2];
                PlanarPosVector = pdba.getCoords(PlanarAtomStrucString);
                PAtomStrucVector = pdba.getCoords(PlanarAtomStrucString);
                PDistance = 0;
                incRefresh();
            }
            if (pa != "") //we want to set the atoms, we may not want to use them
            {
                if (pa != PlanarAtomStrucString || atom_offset != 0)
                {
                    PlanarAtomStrucString = pdba.getIncAtom(pa, atom_offset);
                    PAtomStrucVector = pdba.getCoords(PlanarAtomStrucString);                                        
                    incRefresh();
                }                
            }
            if (pxyz != "")
            {
                if (pxyz != _pxyz)
                {
                    //unwrap
                    string[] xyzs = pxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    PlanarPosVector = new VectorThree(x, y, z);                    
                    incRefresh();
                }
            }
            else if (pa != "") // we set them up already so now move them to the positions
            {
                PlanarPosVector = new VectorThree(PAtomStrucVector.A, PAtomStrucVector.B, PAtomStrucVector.C);                                                
                incRefresh();                
            }
            PDistance = Math.Round(PlanarPosVector.distance(PAtomStrucVector), 3);
            _pxyz = pxyz;
            */
        }

    }
}
