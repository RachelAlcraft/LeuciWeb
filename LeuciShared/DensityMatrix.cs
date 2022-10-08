using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * This pattern allows an asynchronous constructir
 * https://stackoverflow.com/questions/8145479/can-constructors-be-async/31471915#31471915
 */

namespace LeuciShared
{
    public class DensityMatrix
    {
        private const int MAXSIZE = 32;
        private string? _pdbcode;
        private int _A;
        private int _B;
        private int _C;

        private DensityBinary? _densityBinary;
        private Cubelet? _cublet;
        private int[,]? _plane;
        private int[,,]? _cube;

        public static async Task<DensityMatrix> CreateAsync(string pdbcode)
        {
            DensityMatrix x = new DensityMatrix();
            await x.InitializeAsync(pdbcode);
            return x;
        }
        private DensityMatrix() { }
        private async Task InitializeAsync(string pdbcode)
        {
            _pdbcode = pdbcode;
            string edFile = "wwwroot/App_Data/" + _pdbcode + ".ccp4";
            await DownloadAsync(edFile);            
            _densityBinary = new DensityBinary(edFile);
            _A = Convert.ToInt32(_densityBinary.Words["01_NX"]);
            _B = Convert.ToInt32(_densityBinary.Words["02_NY"]);
            _C = Convert.ToInt32(_densityBinary.Words["03_NZ"]);

            _cublet = new Cubelet(_A, _B, _C);
            _plane = new int[0,0];
            _cube =  new int[0,0,0];

        }

        public async Task<bool> DownloadAsync(string edFile)
        {
            bool haveED = false;            
            if (!File.Exists(edFile))
            {
                string edWebPath = @"https://www.ebi.ac.uk/pdbe/coordinates/files/" + _pdbcode + ".ccp4";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (var s = await client.GetStreamAsync(edWebPath))
                        {
                            using (var fs = new FileStream(edFile, FileMode.CreateNew))
                            {
                                var tasks = s.CopyToAsync(fs);
                                await Task.WhenAll(tasks);
                                haveED = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        haveED = false;
                    }
                }
            }
            else
            {
                haveED = true;
            }
            return haveED;
        }

    

        public double[,] getPlane()
        {
            return new double[0, 0];
        }

        public double[,] getSlice()
        {
            return new double[0, 0];
        }

      
    }
}
