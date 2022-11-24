

using LeuciShared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Permissions;

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
                        _interp = "BSPLINE3";
                        //FileDownloads fd = new FileDownloads(value);
                        //fd.downloadAll();
                        //EmCode = fd.EmCode;
                        //DensityType = fd.DensityType;
                    }
                    _pdbcode = value;
                }
                else if(_pdbcode != "6eex")
                {
                    _pdbcode = "6eex";
                    incRefresh();
                }
            }
        }
        private string _interp = "BSPLINE3";
        public string Interp
        {
            get { return _interp; }
            set
            {
                if (value == _interp)
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
                    _interp = value;                    
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
            get;set;
        }
        public string DensityType
        {
            get; set;
        }

        public string EbiLink
        {
            get { return "https://www.ebi.ac.uk/pdbe/entry/pdb/" + PdbCode;  }
            
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
                    _sdcap = Math.Max(0,value);
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

        private double _gap = 0.1;
        public double Gap
        {
            get { return _gap; }
            set
            {
                //don't maintain ratio when increasing the gap                
                if (value == -2) // this means increase by 0.05
                {
                    _gap += 0.01;
                    incRefresh();
                }
                else if (value == -3)
                {
                    _gap -= 0.01;
                    if (_gap <= 0.01)
                        _gap = 0.01;
                    incRefresh();
                }
                else if (value == -1) // this means go back to default
                {
                    _gap = 0.1;
                }
                else if (Math.Round(value, 4) != Math.Round(_gap, 4))
                {                    
                    _gap = value;
                    incRefresh();
                }
                if (_width / _gap > 110)
                {
                    _gap = _width / 110;
                    incRefresh();
                }
                _width = Math.Round(_width, 4);
                _gap = Math.Round(_gap, 4);
            }
        }
        private double _width = 6.0;
        public double Width
        {
            get { return _width; }
            set
            {
                //maintain ratio
                double nums = _width * _gap;
                if (value == -2) // this means increase by 0.5
                {
                    _width += 0.5;
                    _gap = nums / _width;
                    incRefresh();
                }
                else if (value == -3) //this means decrease by 0.5
                {
                    _width -= 0.5;
                    if (_width <= 0.5)
                        _width = 0.5;
                    _gap = nums / _width;
                    incRefresh();
                }
                else if (value == -1) // this means go back to default
                {
                    _width = 6.0;
                }
                else if (Math.Round(value,4) != Math.Round(_width,4)) 
                {
                    double aspectRatio = _width * _gap;
                    _width = value;
                    _gap = _width / aspectRatio;
                    incRefresh();
                }
                if (_width / _gap > 110)
                {                    
                    _gap = _width / 110;
                    incRefresh();
                }
                _width = Math.Round(_width, 4);
                _gap = Math.Round(_gap, 4);
            }
        }                  
        // Handle setting the central-linear-planar
        public VectorThree Central = new VectorThree(-1, -1, -1);        
        private string _cxyz = "(-1,-1,-1)";
        public VectorThree CAtom = new VectorThree(-1, -1, -1);
        public string CentralAtom = "A:1@C";
        public double CDistance = 0;
        public void SetCentral(string cxyz, string ca,PdbAtoms pdba,int atom_offset,bool refresh=true)
        {
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
            if (ca != "")
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
            else if (cxyz != "" && cxyz != null)
            {
                if (cxyz != _cxyz)
                {
                    incRefresh();
                    //unwrap
                    string[] xyzs = cxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    //CentralAtom = "";
                    CDistance = Math.Round(CAtom.distance(new VectorThree(x, y, z)), 3);
                    Central = new VectorThree(x, y, z);
                    _cxyz = cxyz;                    
                }
            }            
            else
            {

            }
        }
        public VectorThree Linear = new VectorThree(-1,-1,-1);        
        private string _lxyz = "(-1,-1,-1)";
        public VectorThree LAtom = new VectorThree(-1, -1, -1);
        public string LinearAtom = "A:1@O";
        public double LDistance = 0;
        public void SetLinear(string lxyz, string la, PdbAtoms pdba, int atom_offset, bool refresh = true)
        {
            if (lxyz == null)
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
            if (la != "")
            {
                if (la != LinearAtom || lxyz == "" || atom_offset!=0)
                {
                    LinearAtom = pdba.getIncAtom(la, atom_offset);                    
                    Linear = pdba.getCoords(LinearAtom);
                    LAtom = new VectorThree(Linear.A, Linear.B, Linear.C);
                    LDistance = 0;
                    incRefresh();
                }
            }
            else if (lxyz != "" && lxyz != null)
            {
                if (lxyz != _lxyz)
                {
                    //unwrap
                    string[] xyzs = lxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    LDistance = Math.Round(LAtom.distance(new VectorThree(x, y, z)), 3);
                    //LinearAtom = "";
                    Linear = new VectorThree(x, y, z);
                    _lxyz = lxyz;
                    incRefresh();
                }
            }
            
        }
        
        public VectorThree Planar = new VectorThree(-1,-1,-1);
        public VectorThree PAtom = new VectorThree(-1, -1, -1);
        private string _pxyz = "(-1,-1,-1)";
        public string PlanarAtom = "A:2@N";
        public double PDistance = 0;
        public void SetPlanar(string pxyz, string pa, PdbAtoms pdba, int atom_offset, bool refresh=true)
        {
            if (pxyz == null)
                pxyz = "";
            if (pa == null)
                pa = "";
            if (refresh)
            {
                PlanarAtom = pdba.getFirstThreeCoords()[2];                
                Planar = pdba.getCoords(PlanarAtom);
                PAtom = new VectorThree(Planar.A, Planar.B, Planar.C);
                PDistance = 0;
                incRefresh();
            }
            else if (Planar.A + Planar.B + Planar.C == -3)
            {
                Planar = pdba.getCoords(PlanarAtom);
                PAtom = new VectorThree(Planar.A, Planar.B, Planar.C);
                PDistance = 0;
                incRefresh();
            }
            if (pa != "")
            {
                if (pa != PlanarAtom || pxyz == "" || atom_offset != 0)
                {
                    PlanarAtom = pdba.getIncAtom(pa, atom_offset);                    
                    Planar = pdba.getCoords(PlanarAtom);
                    PAtom = new VectorThree(Planar.A, Planar.B, Planar.C);
                    PDistance = 0;
                    incRefresh();
                }
            }
            else if (pxyz != "" && pxyz != null)
            {
                if (pxyz != _pxyz)
                {
                    //unwrap
                    string[] xyzs = pxyz.Split(",");
                    double x = Convert.ToDouble(xyzs[0].Substring(1));
                    double y = Convert.ToDouble(xyzs[1]);
                    double z = Convert.ToDouble(xyzs[2].Substring(0, xyzs[2].Length - 1));
                    PDistance = Math.Round(PAtom.distance(new VectorThree(x, y, z)), 3);
                    //PlanarAtom = "";
                    Planar = new VectorThree(x, y, z);
                    _pxyz = pxyz;
                    incRefresh();
                }
            }            
        }

    }
}
