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
        private string _interp = "";
        private DensityMatrix _dm;
        public FileDownloads FD;
        public PdbAtoms PA;
        public bool NewMatrix = false;

        public async Task<DensityMatrix> getMatrix(string pdbcode, string interp)
        {
            bool calc = false;
            NewMatrix = false;
            if (_pdbcode == "" || _dm == null || pdbcode != _pdbcode)
                calc = true;

            if (interp != _interp)
            {
                calc = true;
                _interp = interp;
            }

            if (!calc)
            {
                NewMatrix = false;
                return _dm;
            }
            else
            {
                FD = new FileDownloads(pdbcode);
                bool ok = await FD.downloadAll();
                PA = new PdbAtoms(FD.PdbLines);
                
                _pdbcode = pdbcode;
                _dm = await DensityMatrix.CreateAsync(pdbcode,FD.EmFilePath,interp);
                NewMatrix = true;
                return _dm;
            }
        }

    }
}
