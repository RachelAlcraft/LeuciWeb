

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

        private string _pdbcode = "6eex";
        public string PdbCode
        {
            get { return _pdbcode; }
            set 
            { 
                if (value != "")
                    _pdbcode = value;                
            }
        }
        private string _emcode = "6eex";
        public string EmCode
        {
            get { return _emcode; }
            set
            {
                if (value != "")
                    _emcode = value;                
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
                    _plane = value;
            }
        }
        private int _layer = 0;
        public int Layer
        {
            get { return _layer; }
            set
            {
                if (value != -1)
                    _layer = value;
            }
        }


    }
}
