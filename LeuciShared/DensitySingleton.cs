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

        private string _pdbcode = "6eex";
        private DensityMatrix _dm;

        public async Task<DensityMatrix> getMatrix(string pdbcode)
        {
            if (pdbcode == _pdbcode || pdbcode == "")
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
