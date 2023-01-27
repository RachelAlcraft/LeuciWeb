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
        private int _copies = 2;

        public bool needMatrix(string pdbcode)
        {
            bool calc = false;
            NewMatrix = false;
            if (_pdbcode == "" || _dm == null || pdbcode != _pdbcode)
            {
                calc = true;
            }
            return calc;
        }

        public async Task<DensityMatrix> getMatrix(string pdbcode, string interp, int fos, int fcs,int copies,bool symmetry=true)
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
            else if (interp != _interp && interp != "" || copies != _copies)
            {
                _interp = interp;
                _copies = copies;
                NewMatrix = true;
                if (_dm != null)
                    _dm.changeInterp(_interp,_copies);
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
                    _dm = await DensityMatrix.CreateAsync(pdbcode, FD.EmFilePath, FD.DiffFilePath, interp, fos, fcs,symmetry,copies);
                }
                catch (Exception e)
                {
                    try
                    {
                        _dm = await DensityMatrix.CreateAsync(pdbcode, FD.EmFilePath, FD.DiffFilePath, interp, fos, fcs, symmetry,copies);
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
        public async Task<bool> loadFDFiles(string pdbcode)
        {
            //Load the density matrix
            DensitySingleton.Instance.FD = new FileDownloads(pdbcode);
            string pdbStatus = await DensitySingleton.Instance.FD.existsPdbMatrixAsync();
            if (pdbStatus == "Y")
            {
                string ccp4Status = await DensitySingleton.Instance.FD.existsCcp4Matrix();
                if (ccp4Status == "Y")
                {
                    DensityMatrix dm = await DensitySingleton.Instance.getMatrix(pdbcode, "LINEAR", 2, -1, 2);                    
                    return true;
                }                
            }            
            return false;
        }

    }
}
