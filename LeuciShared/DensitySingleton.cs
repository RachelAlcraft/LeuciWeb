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
        private DensityMatrix _dm;

        public async Task<DensityMatrix> getMatrix(string pdbcode)
        {
            bool calc = false;
            if (_pdbcode == "" || _dm == null || pdbcode != _pdbcode)
                calc = true;

            if (!calc)
            {
                return _dm;
            }
            else
            {
                _pdbcode = pdbcode;
                _dm = await DensityMatrix.CreateAsync(pdbcode);
                return _dm;
            }
        }

    }
}
