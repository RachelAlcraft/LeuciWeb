

using LeuciShared;
using Microsoft.AspNetCore.Mvc;


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
                        ++_refresh;
                        //FileDownloads fd = new FileDownloads(value);
                        //fd.downloadAll();
                        //EmCode = fd.EmCode;
                        //DensityType = fd.DensityType;
                    }
                    _pdbcode = value;
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
                        ++_refresh;
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
                        ++_refresh;
                    _layer = value;
                }
            }
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
        private string _radplot = "heatmap";
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
        private string _planeplot = "contour";
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
                if (value == -2) // this means increase by 0.05
                {
                    _gap += 0.01;
                    ++_refresh;
                }
                else if (value == -3)
                {
                    _gap -= 0.01;
                    if (_gap <= 0.01)
                        _gap = 0.01;
                    ++_refresh;
                }
                else if (value == -1) // this means don;t change
                {
                    
                }
                else
                {
                    _gap = value;
                    ++_refresh;
                }
                if (_width / _gap > 100)
                {
                    _width = 100 * _gap;
                    ++_refresh;
                }
                _width = Math.Round(_width, 4);
                _gap = Math.Round(_gap, 4);
            }
        }
        private double _width = 5.0;
        public double Width
        {
            get { return _width; }
            set
            {
                if (value == -2) // this means increase by 0.05
                {
                    _width += 0.5;
                    ++_refresh;
                }
                else if (value == -3)
                {
                    _width -= 0.5;
                    if (_width <= 0.5)
                        _width = 0.5;
                    ++_refresh;
                }
                else if (value == -1) // this means don;t change
                {

                }
                else 
                { 
                    _width = value;
                    ++_refresh;
                }
                if (_width / _gap > 100)
                {
                    _gap = _width / 100;
                    ++_refresh;
                }
                _width = Math.Round(_width, 4);
                _gap = Math.Round(_gap, 4);
            }
        }
                  
        // Handle setting the central-linear-planar
        public VectorThree Central = new VectorThree(2.939, 9.67, 18.422);
        private string _centralatom = "";
        public void SetCentral(double cx, double cy, double cz, string ca)
        {
            // TODO we need to figure out the distance between the atom and the points and decide if it is the same, return it if so or blank if not
            // TODO and we need to know which has changed and which has not

            // meanwhile:
            if (cx != -1)
                Central.A = cx;
            if (cy != -1)
                Central.B = cy;
            if (cz != -1)
                Central.C = cz;
        }
        public VectorThree Linear = new VectorThree(3.567, 9.168, 19.706);
        private string _linearatom = "";
        public void SetLinear(double lx, double ly, double lz, string la)
        {
            // TODO we need to figure out the distance between the atom and the points and decide if it is the same, return it if so or blank if not
            // TODO and we need to know which has changed and which has not

            // meanwhile:
            if (lx != -1)
                Linear.A = lx;
            if (ly != -1)
                Linear.B = ly;
            if (lz != -1)
                Linear.C = lz;
        }
        
        public VectorThree Planar = new VectorThree(1.823, 10.185, 18.428);
        private string _planaratom = "";
        public void SetPlanar(double px, double py, double pz, string pa)
        {
            // TODO we need to figure out the distance between the atom and the points and decide if it is the same, return it if so or blank if not
            // TODO and we need to know which has changed and which has not

            // meanwhile:
            if (px != -1)
                Planar.A = px;
            if (py != -1)
                Planar.B = py;
            if (pz != -1)
                Planar.C = pz;
        }

    }
}
