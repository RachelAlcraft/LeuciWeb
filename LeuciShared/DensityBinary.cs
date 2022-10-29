using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace LeuciShared
{
   
    public class DensityBinary
    {
        public Dictionary<string, string> Words = new Dictionary<string, string>();
        public string Info = "";
        private string _fileName;
        public byte[] Bytes = new byte[0];
        public int Blength = 0;
        public int Bstart = 0;
        //private double[,,] _myMatrix;
        //private double[] _myMatrixListX;
        //private Dictionary<int,double> _myMatrixList;


        // conversion between orthogonal
        private MatrixThreeThree _orthoMat = new MatrixThreeThree();
        private MatrixThreeThree _deOrthoMat = new MatrixThreeThree();
        private VectorThree _origin = new VectorThree();
        private int[] _map2xyz = new int[3];
        private int[] _map2crs = new int[3];
        private double[] _cellDims = new double[3];
        private int[] _axisSampling = new int[3];
        private int[] _crsStart = new int[3];
        private int[] _dimOrder = new int[3];

        private const double M_PI = 3.14159265358979323846;   // pi

        public int X1_cap = 0;
        public int Y2_cap = 0;
        public int Z3_cap = 0;

        public double Sd = 0;
        public double Mean = 0;

        const int TEMP_CAP = 1000;
        public bool INIT = false;

        public DensityBinary(string fileName)
        {
            //I am going to try to read this twice and if it fails throw an error and delete the potentially corrupt file
            _fileName = fileName;
            try
            {
                Bytes = ReadBinaryFile(_fileName);
            }
            catch(Exception e)
            {
                try
                {
                    Bytes = ReadBinaryFile(_fileName);
                }
                catch(Exception ee)
                {
                    throw new Exception("Error creating binary so deleted " + _fileName + " - " + ee.Message);
                }
            }
            createWords(Bytes);
            //List<Single> sings = bytesToSingles(_bytes);                        
        }

        public void Init()
        {
            //calcStats(Bytes);
            //createMatrix(0, Z3_cap, 0, Y2_cap, 0, X1_cap);
            // Some testing
            //VectorThree test1 = getCRSFromXYZ(new VectorThree(0, 0, 0));
            //VectorThree test2 = getXYZFromCRS(test1);
            //VectorThree test3 = getCRSFromXYZ(new VectorThree(1, 1, 10));
            //VectorThree test4 = getXYZFromCRS(test3);
            INIT = true;
        }

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
            catch (Exception e)
            {
                return null;
            }
        }

        public void createWords(byte[] fileInBinary)
        {
            X1_cap = bytesToInt(fileInBinary, 0); // 1
            Y2_cap = bytesToInt(fileInBinary, 4); // 1
            Z3_cap = bytesToInt(fileInBinary, 8); // 1            
            Words["01_NX"] = Convert.ToString(X1_cap); // 1
            Words["02_NY"] = Convert.ToString(Y2_cap); // 2
            Words["03_NZ"] = Convert.ToString(Z3_cap); // 3
            // WARNING !!!! OPTIMISATION DECISION cap any of these to 200
            X1_cap = Math.Min(TEMP_CAP, X1_cap);
            Y2_cap = Math.Min(TEMP_CAP, Y2_cap);
            Z3_cap = Math.Min(TEMP_CAP, Z3_cap);
            /////////////////////////////////////////////////////////////
            Words["04_MODE"] = Convert.ToString(bytesToInt(fileInBinary, 12)); // 4
            Words["05_NXSTART"] = Convert.ToString(bytesToInt(fileInBinary, 16)); // 5
            Words["06_NYSTART"] = Convert.ToString(bytesToInt(fileInBinary, 20)); // 6
            Words["07_NZSTART"] = Convert.ToString(bytesToInt(fileInBinary, 24)); // 7
            Words["08_MX"] = Convert.ToString(bytesToInt(fileInBinary, 28)); // 8
            Words["09_MY"] = Convert.ToString(bytesToInt(fileInBinary, 32)); // 9 
            Words["10_MZ"] = Convert.ToString(bytesToInt(fileInBinary, 36)); // 10
            Words["11_CELLA_X"] = Convert.ToString(bytesToSingle(fileInBinary, 40)); // 11
            Words["12_CELLA_Y"] = Convert.ToString(bytesToSingle(fileInBinary, 44)); // 12
            Words["13_CELLA_Z"] = Convert.ToString(bytesToSingle(fileInBinary, 48)); // 13
            Words["14_CELLB_X"] = Convert.ToString(Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 52)), 2))); // 14
            Words["15_CELLB_Y"] = Convert.ToString(Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 56)), 2))); // 15
            Words["16_CELLB_Z"] = Convert.ToString(Convert.ToSingle(Math.Round(Convert.ToDouble(bytesToSingle(fileInBinary, 60)), 2))); // 16
            Words["17_MAPC"] = Convert.ToString(bytesToInt(fileInBinary, 64) - 1); // 17
            Words["18_MAPR"] = Convert.ToString(bytesToInt(fileInBinary, 68) - 1); // 18
            Words["19_MAPS"] = Convert.ToString(bytesToInt(fileInBinary, 72) - 1); // 19
            Words["20_DMIN"] = Convert.ToString(bytesToSingle(fileInBinary, 76)); // 20
            Words["21_DMAX"] = Convert.ToString(bytesToSingle(fileInBinary, 80)); // 21
            Words["22_DMEAN"] = Convert.ToString(bytesToSingle(fileInBinary, 84)); // 22

            Words["ISPG"] = Convert.ToString(bytesToInt(fileInBinary, 88)); // 23
            Words["NYSYMBT"] = Convert.ToString(bytesToInt(fileInBinary, 92)); // 24
                                                                               //EXTRA
            Words["EXTTYP"] = Convert.ToString(bytesToInt(fileInBinary, 104)); // 27
            Words["NVERSION"] = Convert.ToString(bytesToInt(fileInBinary, 108)); // 28
                                                                                 //EXTRA
            Words["ORIGIN_X"] = Convert.ToString(bytesToSingle(fileInBinary, 196)); // 50
            Words["ORIGIN_Y"] = Convert.ToString(bytesToSingle(fileInBinary, 200)); // 51
            Words["ORIGIN_Z"] = Convert.ToString(bytesToSingle(fileInBinary, 204)); // 52
            Words["MAP"] = Convert.ToString(bytesToString(fileInBinary, 208, 4)); // 53
            Words["RMS"] = Convert.ToString(bytesToSingle(fileInBinary, 216)); // 55
            Words["NLABL"] = Convert.ToString(bytesToInt(fileInBinary, 220)); // 56
            Words["LABEL1"] = Convert.ToString(bytesToString(fileInBinary, 224, 80)); // 57
            Words["LABEL2"] = Convert.ToString(bytesToString(fileInBinary, 304, 80)); // 58
            Words["LABEL3"] = Convert.ToString(bytesToString(fileInBinary, 384, 80)); // 59
            Words["LABEL4"] = Convert.ToString(bytesToString(fileInBinary, 464, 80)); // 60
            Words["LABEL5"] = Convert.ToString(bytesToString(fileInBinary, 544, 80)); // 61
            Words["LABEL6"] = Convert.ToString(bytesToString(fileInBinary, 624, 80)); // 62
            Words["LABEL7"] = Convert.ToString(bytesToString(fileInBinary, 704, 80)); // 63
            Words["LABEL8"] = Convert.ToString(bytesToString(fileInBinary, 784, 80)); // 64
            Words["LABEL9"] = Convert.ToString(bytesToString(fileInBinary, 864, 80)); // 65
            Words["LABEL10"] = Convert.ToString(bytesToString(fileInBinary, 944, 80)); // 66
            Blength = Convert.ToInt32(Words["01_NX"]) * Convert.ToInt32(Words["02_NY"]) * Convert.ToInt32(Words["03_NZ"]);
            Bstart = Bytes.Length - (4 * Blength);
            makeInfo();
            loadInfo();
            calcStats();
            
        }

        /*public double[] makePlane(List<int[]> coords)
        {
            double[] result = new double[coords.Count];            
            for (int i=0; i < coords.Count; ++i )
            {
                int[] coord = coords[i];
                result[i] = getVal(coord[0], coord[1], coord[2]);
            }
            return result;
        }*/

        private void makeInfo()
        {
            Info = "";
            foreach (KeyValuePair<string, string> entry in Words)
            {
                Info += entry.Key + "=" + entry.Value + "\n";
            }
        }
        //public double[] getShortListX()
        //{
        //    return _myMatrixListX;       
        //}        
        public Dictionary<int, double> getShorterList(List<int> xyz)
        {
            return bytesToDoubles(Bytes, xyz);
        }
        public Dictionary<int,double> getFullList(int[] xyz)
        {
            int count = 0;            
            Dictionary<int,double> matvals=new Dictionary<int,double>();            
            for (int i = Bstart; i < Bytes.Length; i += 4)
            {                
                Single value = BitConverter.ToSingle(Bytes, i);
                matvals[count] = Convert.ToDouble(value);                                
                count++;
                
            }
            return matvals;
        }
        //private double getVal(int x, int y, int z)
        //{// TODO this should be loading the binary and getting just those it wants
        //    return _myMatrix[x, y, z];            
        //}
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

        private void calcStats()
        {            
            Mean = Convert.ToDouble(Words["22_DMEAN"]);
            Sd = Convert.ToDouble(Words["RMS"]);
            /*
            double sum = 0;
            int count = 0;
            int start = bytes.Length - (4 * _datalength);
            double sdSum = 0;            
            for (int i = start; i < bytes.Length; i += 4)
            {
                Single value = BitConverter.ToSingle(bytes, i);
                sdSum += Math.Pow(value - Mean, 2);                
                count++;
                sum += value;
            }
            // calculate the stats                                    
            sdSum /= count;
            Sd = Math.Sqrt(sdSum);*/
        }
        private Dictionary<int,double> bytesToDoubles(byte[] bytes, List<int> poses)
        {            
            int count = 0;
            int start = bytes.Length - (4 * Blength);                     
            // add the reduced numbre of required values to the list
            //Dictionary<int,double> matvals=new Dictionary<int,double>();
            List<Single> vals = new List<Single>();
            /*for (int i = start; i < bytes.Length; i += 4)
            {
                //if (poses.Contains(count))
                {
                    Single value = BitConverter.ToSingle(bytes, i);
                    //matvals[count] = Convert.ToDouble(value);
                    vals.Add(value);
                }
                count++;
                //if (poses.Count == matvals.Count)
                //    return matvals;
            }*/

            foreach (int pos in poses)
            {
                try
                {
                    Single value = BitConverter.ToSingle(bytes, start + pos * 4);
                    vals.Add(value);
                }
                catch(Exception e)
                {
                    string msg = e.Message;
                }
            }

            Dictionary<int,double> matvals=new Dictionary<int,double>();
            for (int m = 0; m < poses.Count; ++m)
            {
                if (matvals.ContainsKey(poses[m]))
                {
                    int breakpoint = 0;
                    breakpoint++;
                }
                else
                {
                    matvals.Add(poses[m], vals[m]);
                }
            }


            return matvals;
        }
        
        public void createMatrix(int xl, int xu, int yl, int yu, int zl, int zu)
        {
            int z = Convert.ToInt32(Words["01_NX"]);
            int y = Convert.ToInt32(Words["02_NY"]);
            int x = Convert.ToInt32(Words["03_NZ"]);

            // first generate list of positions we want given the caps
            List<int> poses = new List<int>();
            int pos = 0;
            for (int i = 0; i < x; ++i)
            {                
                for (int j = 0; j < y; ++j)
                {                        
                    for (int k = 0; k < z; ++k)
                    {
                        if ((i >= xl && i < xu)  && (j >= yl && j < yu) && (k >= zl && k < zu) || xl+xu+yl+yu+zl+zu==-6)
                        {
                            poses.Add(pos);
                        }
                        pos++;
                    }                        
                }                
            }
            //_myMatrixList = bytesToDoubles(_bytes,poses);            
        }

        ///////////////////////////////////////////////////
        private void loadInfo()
        {
        
            int len = Convert.ToInt32(Words["01_NX"]) * Convert.ToInt32(Words["02_NY"]) * Convert.ToInt32(Words["03_NZ"]);
            
            calculateOrthoMat(      Convert.ToDouble(Words["11_CELLA_X"]), Convert.ToDouble(Words["12_CELLA_Y"]), Convert.ToDouble(Words["13_CELLA_Z"]),
                                    Convert.ToDouble(Words["14_CELLB_X"]), Convert.ToDouble(Words["15_CELLB_Y"]), Convert.ToDouble(Words["16_CELLB_Z"]));
            
            calculateOrigin(Convert.ToInt32(Words["05_NXSTART"]), Convert.ToInt32(Words["06_NYSTART"]), Convert.ToInt32(Words["07_NZSTART"]), 
                            Convert.ToInt32(Words["17_MAPC"]), Convert.ToInt32(Words["18_MAPR"]), Convert.ToInt32(Words["08_MX"]), Convert.ToInt32(Words["09_MY"]), Convert.ToInt32(Words["10_MZ"]));

            _map2xyz[Convert.ToInt32(Words["17_MAPC"])] = 0;
            _map2xyz[Convert.ToInt32(Words["18_MAPR"])] = 1;
            _map2xyz[Convert.ToInt32(Words["19_MAPS"])] = 2;
            
            _map2crs[0] = Convert.ToInt32(Words["17_MAPC"]);
            _map2crs[1] = Convert.ToInt32(Words["18_MAPR"]);
            _map2crs[2] = Convert.ToInt32(Words["19_MAPS"]);
            
            _cellDims[0] = Convert.ToDouble(Words["11_CELLA_X"]);
            _cellDims[1] = Convert.ToDouble(Words["12_CELLA_Y"]);
            _cellDims[2] = Convert.ToDouble(Words["13_CELLA_Z"]);
            
            _axisSampling[0] = Convert.ToInt32(Words["08_MX"]);
            _axisSampling[1] = Convert.ToInt32(Words["09_MY"]);
            _axisSampling[2] = Convert.ToInt32(Words["10_MZ"]);
            
            _crsStart[0] = Convert.ToInt32(Words["05_NXSTART"]);
            _crsStart[1] = Convert.ToInt32(Words["06_NYSTART"]);
            _crsStart[2] = Convert.ToInt32(Words["07_NZSTART"]);
            
            _dimOrder[0] = Convert.ToInt32(Words["01_NX"]);
            _dimOrder[1] = Convert.ToInt32(Words["02_NY"]);
            _dimOrder[2] = Convert.ToInt32(Words["03_NZ"]);


        }

        private void calculateOrthoMat(double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z, double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z)
        {
            // Cell angles is w14_CELLB_X, w15_CELLB_Y, w16_CELLB_Z
            // Cell lengths is w11_CELLA_X , w12_CELLA_Y , w13_CELLA_Z 
            double alpha = M_PI / 180 * w14_CELLB_X;
            double beta = M_PI / 180 * w15_CELLB_Y;
            double gamma = M_PI / 180 * w16_CELLB_Z;
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

            _orthoMat.putValue(w11_CELLA_X, 0, 0);
            _orthoMat.putValue(w12_CELLA_Y * Math.Cos(gamma), 0, 1);
            _orthoMat.putValue(w13_CELLA_Z * Math.Cos(beta), 0, 2);
            _orthoMat.putValue(0, 1, 0);
            _orthoMat.putValue(w12_CELLA_Y * Math.Sin(gamma), 1, 1);
            _orthoMat.putValue(w13_CELLA_Z * (Math.Cos(alpha) - Math.Cos(beta) * Math.Cos(gamma)) / Math.Sin(gamma), 1, 2);
            _orthoMat.putValue(0, 2, 0);
            _orthoMat.putValue(0, 2, 1);
            _orthoMat.putValue(w13_CELLA_Z * temp / Math.Sin(gamma), 2, 2);
            _deOrthoMat = _orthoMat.getInverse();
        }

        private void calculateOrigin(int w05_NXSTART, int w06_NYSTART, int w07_NZSTART, int w17_MAPC, int w18_MAPR, int W08_NX, int W09_NY, int W10_NZ)
        {
            /****************************
            * These comments are from my C# version and I have no idea currently what they mean (RSA 6/9/21)
            * ******************************
             *TODO I am ignoring the possibility of passing in the origin for nowand using the dot product calc for non orthoganality.
             *The origin is perhaps used for cryoEM only and requires orthoganility
             *CRSSTART is w05_NXSTART, w06_NYSTART, w07_NZSTART
             *Cell dims w08_MX, w09_MY, w10_MZ;
             *Map of indices from crs to xyz is w17_MAPC, w18_MAPR, w19_MAPS
             */

            VectorThree oro = new VectorThree();

            for (int i = 0; i < 3; ++i)
            {
                int startVal = 0;
                if (w17_MAPC == i)
                    startVal = w05_NXSTART;
                else if (w18_MAPR == i)
                    startVal = w06_NYSTART;
                else
                    startVal = w07_NZSTART;

                oro.putByIndex(i, startVal);
            }
            oro.putByIndex(0, oro.getByIndex(0) / W08_NX);
            oro.putByIndex(1, oro.getByIndex(1) / W09_NY);
            oro.putByIndex(2, oro.getByIndex(2) / W10_NZ);
            _origin = _orthoMat.multiply(oro, true);
        }

        public VectorThree getCRSFromXYZ(VectorThree XYZ)
        {
            VectorThree vCRS = new VectorThree();
            //If the axes are all orthogonal            
            if (Words["14_CELLB_X"] == "90" && Words["15_CELLB_Y"] == "90" && Words["16_CELLB_Z"] == "90")
            {
                for (int i = 0; i < 3; ++i)
                {
                    double startVal = XYZ.getByIndex(i) - _origin.getByIndex(i);
                    startVal /= _cellDims[i] / _axisSampling[i];
                    //vCRS[i] = startVal;
                    vCRS.putByIndex(i, startVal);
                }
            }
            else // they are not orthogonal
            {
                VectorThree vFraction = _deOrthoMat.multiply(XYZ, true);
                for (int i = 0; i < 3; ++i)
                {
                    double val = vFraction.getByIndex(i) * _axisSampling[i] - _crsStart[_map2xyz[i]];
                    vCRS.putByIndex(i, val);
                }
            }
            double c = vCRS.getByIndex(_map2crs[0]);
            double r = vCRS.getByIndex(_map2crs[1]);
            double s = vCRS.getByIndex(_map2crs[2]);

            VectorThree CRS = new VectorThree();
            CRS.A = c;
            CRS.B = r;
            CRS.C = s;
            return CRS;

        }
        public VectorThree getXYZFromCRS(VectorThree vCRSIn)
        {
            VectorThree vXYZ = new VectorThree();

            //If the axes are all orthogonal            
            if (Words["14_CELLB_X"] == "90" && Words["15_CELLB_Y"] == "90" && Words["16_CELLB_Z"] == "90")
            {
                for (int i = 0; i < 3; ++i)
                {
                    double startVal = vCRSIn.getByIndex(_map2xyz[i]);
                    startVal *= _cellDims[i] / _axisSampling[i];
                    startVal += _origin.getByIndex(i);
                    vXYZ.putByIndex(i, startVal);
                }
            }
            else // they are not orthogonal
            {
                VectorThree vCRS = new VectorThree();
                for (int i = 0; i < 3; ++i)
                {
                    double startVal = 0;
                    if (Convert.ToInt32(Words["17_MAPC"]) == i)
                        startVal = Convert.ToInt32(Words["05_NXSTART"]) + vCRSIn.A;
                    else if (Convert.ToInt32(Words["18_MAPR"]) == i)
                        startVal = Convert.ToInt32(Words["06_NYSTART"]) + vCRSIn.B;
                    else
                        startVal = Convert.ToInt32(Words["07_NZSTART"]) + vCRSIn.C;
                    vCRS.putByIndex(i, startVal);
                }
                vCRS.putByIndex(0, vCRS.getByIndex(0) / Convert.ToInt32(Words["08_MX"]));
                vCRS.putByIndex(1, vCRS.getByIndex(1) / Convert.ToInt32(Words["09_MY"]));
                vCRS.putByIndex(2, vCRS.getByIndex(2) / Convert.ToInt32(Words["10_MZ"]));
                vXYZ = _orthoMat.multiply(vCRS, false);
            }
            return vXYZ;
        }
        public bool AllValid(VectorThree crs)
        {
            if (crs.A < TEMP_CAP && crs.B < TEMP_CAP && crs.C < TEMP_CAP)
                return true;
            else
                return false;
        }
    }


}
