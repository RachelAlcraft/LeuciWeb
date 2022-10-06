namespace Leucippus.Models
{
    using System;
    public sealed class MatrixServer
    {
        // https://csharpindepth.com/Articles/Singleton 4th version implementation of Singleton

        private static readonly MatrixServer instance = new MatrixServer();
        private MatrixServer() 
        { 
        }

        public static MatrixServer Instance
        {
            get
            {                
                return instance;
            }
        }

        public string PdbCode = "";
        public ElectronDensity? ed;
        public bool init = false;

        public async Task setPdbCode(string pdbcode)
        {
            init = true;
            if (pdbcode == "")
                pdbcode = "6eex";            
            
            PdbCode = pdbcode;
            ed = new ElectronDensity(PdbCode);
            var tasks = ed.DownloadAsync();
            await Task.WhenAll(tasks);
            ed.sendToDll();

            if (PdbCode.ToLower() == "1ejg")
            {
                ed.CX = 9.373;
                ed.CY = 7.668;
                ed.CZ = 15.546;
                ed.LX = 9.5;
                ed.LY = 9.079;
                ed.LZ = 14.937;
                ed.PX = 9.64;
                ed.PY = 7.542;
                ed.PZ = 16.748;
            }
            else
            {                    
                ed.CX = 2.939;
                ed.CY = 9.67;
                ed.CZ = 18.422;
                ed.LX = 3.567;
                ed.LY = 9.168;
                ed.LZ = 19.706;
                ed.PX = 1.823;
                ed.PY = 10.185;
                ed.PZ = 18.428;
            }
            ed.Layer = 0;
            ed.Plane = "XY";
            ed.getSlice();            
                    
        }
        
    }
}
