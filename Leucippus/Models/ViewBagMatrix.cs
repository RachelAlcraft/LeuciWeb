

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
                        ++_refresh;
                    _pdbcode = value;
                }
            }
        }
        private string _emcode = "6eex";
        public string EmCode
        {
            get { return _emcode; }
            set
            {
                if (value != "")
                {
                    if (value != _emcode)
                        ++_refresh;
                    _emcode = value;
                }
            }
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

    }
}
