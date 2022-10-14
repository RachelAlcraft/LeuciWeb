using System.Numerics;

namespace Leucippus.Models
{
    /*public class Ccp4XXX
    {
        public bool Ready = false;
        public int w01_NX = 0; //Column dim, the fastest changing axis
        public int w02_NY = 0; //Row dim, the fastest changing axis
        public int w03_NZ = 0; //Sector dim, the slowest changing axis        
        public int w04_MODE = 0;
        public int w05_NXSTART = 0;
        public int w06_NYSTART = 0;
        public int w07_NZSTART = 0;
        public int w08_MX = 0;
        public int w09_MY = 0;
        public int w10_MZ = 0;
        public double w11_CELLA_X = 0;
        public double w12_CELLA_Y = 0;
        public double w13_CELLA_Z = 0;
        public double w14_CELLB_X = 0;
        public double w15_CELLB_Y = 0;
        public double w16_CELLB_Z = 0;
        public int w17_MAPC = 0;
        public int w18_MAPR = 0;
        public int w19_MAPS = 0;
        public double w20_DMIN = 0;
        public double w21_DMAX = 0;
        public double w22_DMEAN = 0;
        
        private CopyMatrix _orthoMat;        
        private CopyMatrix _deOrthoMat;        
        EdVector _origin = new EdVector();        
        private int[] _map2xyz;        
        private int[] _map2crs;        
        private double[] _cellDims;        
        private int[] _axisSampling;        
        private int[] _crsStart;        
        private int[] _dimOrder;
        public string InfoDim { get; set; } = "";
        public string InfoCellLengths { get; set; } = "";
        public string InfoCellAngles { get; set; } = "";
        public int Degree = 0;        
        private string _id;

        public double[,,] MyMatrix = new double[0, 0, 0];
        public double[] MyVector = new double[0];




        public byte[] ReadBinaryFile(string filePath)
        {
            try
            {
                byte[] buffer;
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                try
                {
                    int length = (int)fileStream.Length;  // get file length
                    buffer = new byte[length];            // create buffer
                    int count;                            // actual number of bytes read
                    int sum = 0;                          // total number of bytes read

                    // read until Read method returns 0 (end of the stream has been reached)
                    while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                        sum += count;  // sum is a buffer offset for next reading
                }
                finally
                {
                    fileStream.Close();
                }

                //if (BitConverter.IsLittleEndian)
                //    Array.Reverse(buffer);

                return buffer;
            }
            catch(Exception e)
            {
                return null;
            }
        }


        private int bytesToInt(byte[] bytes, int start)
        {
            int i = BitConverter.ToInt32(bytes, start);
            return i;
        }
        private Single bytesToSingle(byte[] bytes, int start)
        {
            Single value = BitConverter.ToSingle(bytes, start);
            return value;
        }
        private List<Single> bytesToSingles(byte[] bytes, int start)
        {
            int len = w01_NX * w02_NY * w03_NZ;
            start = bytes.Length - (4 * len);

            List<Single> matvals = new List<Single>();
            for (int i = start; i < bytes.Length; i += 4)
            {
                Single value = BitConverter.ToSingle(bytes, i);
                matvals.Add(value);
            }
            return matvals;
        }
        private string bytesToString(byte[] bytes, int start, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(bytes, start, result, 0, length);
            using (var stream = new MemoryStream(result))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public void createWords(byte[] fileInBinary)
        {            
            w01_NX = bytesToInt(fileInBinary, 0); // 1
            w02_NY = bytesToInt(fileInBinary, 4); // 2
            w03_NZ = bytesToInt(fileInBinary, 8); // 3
            w04_MODE = bytesToInt(fileInBinary, 12); // 4
            w05_NXSTART = bytesToInt(fileInBinary, 16); // 5
            w06_NYSTART = bytesToInt(fileInBinary, 20); // 6
            w07_NZSTART = bytesToInt(fileInBinary, 24); // 7
            w08_MX = bytesToInt(fileInBinary, 28); // 8
            w09_MY = bytesToInt(fileInBinary, 32); // 9 
            w10_MZ = bytesToInt(fileInBinary, 36); // 10
            w11_CELLA_X = bytesToSingle(fileInBinary, 40); // 11
            w12_CELLA_Y = bytesToSingle(fileInBinary, 44); // 12
            w13_CELLA_Z = bytesToSingle(fileInBinary, 48); // 13
            w14_CELLB_X = Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 52)), 2)); // 14
            w15_CELLB_Y = Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 56)), 2)); // 15
            w16_CELLB_Z = Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 60)), 2)); // 16
            w17_MAPC = bytesToInt(fileInBinary, 64) - 1; // 17
            w18_MAPR = bytesToInt(fileInBinary, 68) - 1; // 18
            w19_MAPS = bytesToInt(fileInBinary, 72) - 1; // 19
            w20_DMIN = bytesToSingle(fileInBinary, 76); // 20
            w21_DMAX = bytesToSingle(fileInBinary, 80); // 21
            w22_DMEAN = bytesToSingle(fileInBinary, 84); // 22
            int ISPG = bytesToInt(fileInBinary, 88); // 23
            int NYSYMBT = bytesToInt(fileInBinary, 92); // 24
                                                        //EXTRA
            int EXTTYP = bytesToInt(fileInBinary, 104); // 27
            int NVERSION = bytesToInt(fileInBinary, 108); // 28
                                                          //EXTRA
            Single ORIGIN_X = bytesToSingle(fileInBinary, 196); // 50
            Single ORIGIN_Y = bytesToSingle(fileInBinary, 200); // 51
            Single ORIGIN_Z = bytesToSingle(fileInBinary, 204); // 52
            string MAP = bytesToString(fileInBinary, 208, 4); // 53
            Single RMS = bytesToSingle(fileInBinary, 216); // 55
            int NLABL = bytesToInt(fileInBinary, 220); // 56
            string LABEL1 = bytesToString(fileInBinary, 224, 80); // 57
            string LABEL2 = bytesToString(fileInBinary, 304, 80); // 58
            string LABEL3 = bytesToString(fileInBinary, 384, 80); // 59
            string LABEL4 = bytesToString(fileInBinary, 464, 80); // 60
            string LABEL5 = bytesToString(fileInBinary, 544, 80); // 61
            string LABEL6 = bytesToString(fileInBinary, 624, 80); // 62
            string LABEL7 = bytesToString(fileInBinary, 704, 80); // 63
            string LABEL8 = bytesToString(fileInBinary, 784, 80); // 64
            string LABEL9 = bytesToString(fileInBinary, 864, 80); // 65
            string LABEL10 = bytesToString(fileInBinary, 944, 80); // 66

            MyMatrix = new double[w03_NZ, w02_NY, w01_NX];
            MyVector = new double[w03_NZ * w02_NY * w01_NX];
            int len = w01_NX * w02_NY * w03_NZ;
            if (fileInBinary.Length - (4 * len) > 0)
            {
                Ready = true;
                //And now get the actual matrx data as a list
                List<Single> theMatrixData = bytesToSingles(fileInBinary, NYSYMBT);

                int count = 0;
                
                for (int i = 0; i < w03_NZ; ++i)
                {
                    for (int j = 0; j < w02_NY; ++j)
                    {
                        for (int k = 0; k < w01_NX; ++k)
                        {
                            double val = theMatrixData[count];
                            MyMatrix[i, j, k] = val;
                            MyVector[count] = val;

                            w20_DMIN = Math.Min(w20_DMIN, val);
                            w21_DMAX = Math.Max(w21_DMAX, val);
                            if (val < 0)
                            {
                                int ibp = 0;
                                ++ibp;//simply for a breakpoint to check less than 0 values in the inputs
                            }
                            if (count > 10000)
                            {
                                int abp = 0;
                                ++abp;//simply for a breakpoint to check less than 0 values in the inputs
                            }
                            count += 1;

                        }
                    }
                }
                //And now make some calculations
                calculateOrthoMat();
                calculateOrigin();
                //_map2xyz = Vector.Create<int>(3);
                _map2xyz = new int[3];
                _map2xyz[w17_MAPC] = 0;
                _map2xyz[w18_MAPR] = 1;
                _map2xyz[w19_MAPS] = 2;

                //_map2crs = Vector.Create<int>(3);
                _map2crs = new int[3];
                _map2crs[0] = w17_MAPC;
                _map2crs[1] = w18_MAPR;
                _map2crs[2] = w19_MAPS;

                //_cellDims = Vector.Create<Single>(3);
                _cellDims = new double[3];
                _cellDims[0] = w11_CELLA_X;
                _cellDims[1] = w12_CELLA_Y;
                _cellDims[2] = w13_CELLA_Z;

                //_axisSampling = Vector.Create<int>(3);
                _axisSampling = new int[3];
                _axisSampling[0] = w08_MX;
                _axisSampling[1] = w09_MY;
                _axisSampling[2] = w10_MZ;

                //_crsStart = Vector.Create<int>(3);
                _crsStart = new int[3];
                _crsStart[0] = w05_NXSTART;
                _crsStart[1] = w06_NYSTART;
                _crsStart[2] = w07_NZSTART;

                _dimOrder = new int[3];
                _dimOrder[0] = w01_NX;
                _dimOrder[1] = w02_NY;
                _dimOrder[2] = w03_NZ;

                InfoDim = Convert.ToString(w01_NX) + ":" + Convert.ToString(w02_NY) + ":" + Convert.ToString(w03_NZ);
                InfoCellLengths = Convert.ToString(Math.Round(w11_CELLA_X / w08_MX, 2)) + ":" + Convert.ToString(Math.Round(w12_CELLA_Y / w09_MY, 2)) + ":" + Convert.ToString(Math.Round(w13_CELLA_Z / w10_MZ, 2));
                InfoCellAngles = Convert.ToString(Math.Round(w14_CELLB_X, 0)) + ":" + Convert.ToString(Math.Round(w15_CELLB_Y, 0)) + ":" + Convert.ToString(Math.Round(w16_CELLB_Z, 0));
            }            

        }

        private void calculateOrthoMat()
        {            
            _orthoMat = new CopyMatrix();
            double alpha = Math.PI / 180 * w14_CELLB_X;
            double beta = Math.PI / 180 * w15_CELLB_Y;
            double gamma = Math.PI / 180 * w16_CELLB_Z;
            double temp = Math.Sqrt(1 - Math.Pow(Math.Cos(alpha), 2) - Math.Pow(Math.Cos(beta), 2) - Math.Pow(Math.Cos(gamma), 2) + 2 * Math.Cos(alpha) * Math.Cos(beta) * Math.Cos(gamma));
            double v00 = w11_CELLA_X;
            double v01 = w12_CELLA_Y * Math.Cos(gamma);
            double v02 = w13_CELLA_Z * Math.Cos(beta);
            double v10 = 0;
            double v11 = w12_CELLA_Y * Math.Sin(gamma);
            double v12 = w13_CELLA_Z * (Math.Cos(alpha) - Math.Cos(beta) * Math.Cos(gamma)) / Math.Sin(gamma);
            double v20 = 0;
            double v21 = 0;
            double v22 = w13_CELLA_Z * temp / Math.Sin(gamma);
            _orthoMat.set(0, 0, w11_CELLA_X);
            _orthoMat.set(0, 1, w12_CELLA_Y * Math.Cos(gamma));
            _orthoMat.set(0, 2, w13_CELLA_Z * Math.Cos(beta));
            _orthoMat.set(1, 0, 0);
            _orthoMat.set(1, 1, w12_CELLA_Y * Math.Sin(gamma));
            _orthoMat.set(1, 2, w13_CELLA_Z * (Math.Cos(alpha) - Math.Cos(beta) * Math.Cos(gamma)) / Math.Sin(gamma));
            _orthoMat.set(2, 0, 0);
            _orthoMat.set(2, 1, 0);
            _orthoMat.set(2, 2, w13_CELLA_Z * temp / Math.Sin(gamma));            
            _deOrthoMat = _orthoMat.getInverse();            
        }

        private void calculateOrigin()
        {
            // TODO I am ignoring the possibility of passing in the origin for now and using the dot product calc for non orthoganality.
            // The origin is perhaps used for cryoEM only and requires orthoganility            
            EdVector vCRS = new EdVector();
            for (int i = 0; i < 3; ++i)
            {
                int startVal = 0;
                if (w17_MAPC == i)
                    startVal = w05_NXSTART;
                else if (w18_MAPR == i)
                    startVal = w06_NYSTART;
                else
                    startVal = w07_NZSTART;
                
                vCRS.vector[i] = startVal;
            }            
            vCRS.vector[0] /= w08_MX;
            vCRS.vector[1] /= w09_MY;
            vCRS.vector[2] /= w10_MZ;
            _origin = _orthoMat.multiply(vCRS);            
        }
    }*/


}
