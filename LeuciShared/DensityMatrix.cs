using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
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
        private string? _emcode;
        private int _A;
        private int _B;
        private int _C;
        public string Info = "";


        private DensityBinary? _densityBinary;
        private Cubelet? _cublet;        
        public double[] MatA;
        public double[] MatB;
        public double[] MatC;
        public double[][] MatD;        
        public static async Task<DensityMatrix> CreateAsync(string pdbcode)
        {
            DensityMatrix x = new DensityMatrix();
            await x.InitializeAsync(pdbcode);
            return x;
        }
        private DensityMatrix() { }
        private async Task InitializeAsync(string emcode)
        {
            _emcode = emcode;
            string edFile = "wwwroot/App_Data/" + _emcode + ".ccp4";
            await DownloadAsync(edFile);
            _densityBinary = new DensityBinary(edFile);
            //_A = Convert.ToInt32(_densityBinary.Words["08_MX"]);
            //_B = Convert.ToInt32(_densityBinary.Words["09_MY"]);
            //_C = Convert.ToInt32(_densityBinary.Words["10_MZ"]);
            _A = Convert.ToInt32(_densityBinary.Words["03_NZ"]);
            _B = Convert.ToInt32(_densityBinary.Words["02_NY"]);
            _C = Convert.ToInt32(_densityBinary.Words["01_NX"]);
            Info = _densityBinary.Info;            
            _cublet = new Cubelet(_A, _B, _C);            
        }

        public async Task<bool> DownloadAsync(string edFile)
        {
            bool haveED = false;            
            if (!File.Exists(edFile))
            {
                string edWebPath = @"https://www.ebi.ac.uk/pdbe/coordinates/files/" + _emcode + ".ccp4";
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

    

        public void calculatePlane(string plane, int layer)
        {
            //TODO check if it needs to be recalced
            List<int[]> coords = _cublet.getPlaneCoords(plane, layer);
            int[] XY = _cublet.getPlaneDims(plane, layer);
            double[] doubles = _densityBinary.makePlane(coords);
            MatD = _cublet.makeSquare(doubles, XY);
            MatA = new double[XY[1]];
            MatB = new double[XY[0]];
            MatC = new double[XY[0]*XY[1]];
            int count = 0;
            for (int a=0; a < XY[0]; ++a)
            {
                MatB[a] = a;
                for (int b = 0; b < XY[1]; ++b)
                {
                    MatA[b] = b;
                    MatC[count] = MatD[a][b];
                    count++;

                }
            }            
        }

        

      
    }
}
