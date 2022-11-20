using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace LeuciShared
{
    public sealed class DensitySingleton
    {
        // https://csharpindepth.com/Articles/Singleton 4th version implementation of Singleton

        private static readonly DensitySingleton instance = new DensitySingleton();
        static DensitySingleton()
        {
        }
        private DensitySingleton()
        {
        }

        public static DensitySingleton Instance
        {
            get
            {
                return instance;
            }
        }

        private string _pdbcode = "";
        private string _interp = "BSPLINE3";
        private DensityMatrix _dm;
        public FileDownloads FD;        
        public bool NewMatrix = false;
        private int _fos = 2;
        private int _fcs = -1;

        public async Task<DensityMatrix> getMatrix(string pdbcode, string interp,int fos,int fcs)
        {
            bool calc = false;
            NewMatrix = false;
            if (_pdbcode == "" || _dm == null || pdbcode != _pdbcode)
            { 
                calc = true;
            }
            if (fos != _fos || fcs != _fcs)
            {
                calc = true;
                _fos = fos;
                _fcs = fcs;
            }
            else if (interp != _interp && interp != "")
            {                
                _interp = interp;
                if (_dm != null)
                    _dm.changeInterp(_interp);
            }
            
            if (!calc)
            {
                NewMatrix = false;
                return _dm;
            }
            else
            {                                                
                _pdbcode = pdbcode;
                try
                {
                    _dm = await DensityMatrix.CreateAsync(pdbcode, FD.EmFilePath, FD.DiffFilePath, interp, fos, fcs);
                }
                catch(Exception e)
                {
                    try
                    {
                        _dm = await DensityMatrix.CreateAsync(pdbcode, FD.EmFilePath, FD.DiffFilePath, interp, fos, fcs);
                    }
                    catch (Exception ee)
                    {
                        _pdbcode = "";
                        File.Delete(FD.EmFilePath);
                        throw new Exception("Error creating binary so deleted " + FD.EmFilePath + " - " + ee.Message);                        
                    }

                }
                NewMatrix = true;
                return _dm;
            }
        }

    }
}
