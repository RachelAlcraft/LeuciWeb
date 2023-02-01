using Leucippus.Models;
using LeuciShared;

namespace Leucippus.Controllers
{
    static public class MatrixHelper
    {
        public static async Task<bool> loadFDFiles(string pdbcode)
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
