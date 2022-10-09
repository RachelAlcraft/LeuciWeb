using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
   
    public class DensityBinary
    {
        public Dictionary<string, string> Words = new Dictionary<string, string>();
        public string Info = "";
        private string _fileName;
        private byte[] _bytes;
        private int _datalength = 0;
        private double[,,] _myMatrix;
        private double[] _myMatrixList;
        public DensityBinary(string fileName)
        {
            _fileName = fileName;
            _bytes = ReadBinaryFile(_fileName);
            createWords(_bytes);
            List<Single> sings = bytesToSingles(_bytes);
            int z = Convert.ToInt32(Words["01_NX"]);
            int y = Convert.ToInt32(Words["02_NY"]);
            int x = Convert.ToInt32(Words["03_NZ"]);            
            createMatrix(sings,x,y,z);
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
            Words["01_NX"] = Convert.ToString(bytesToInt(fileInBinary, 0)); // 1
            Words["02_NY"] = Convert.ToString(bytesToInt(fileInBinary, 4)); // 2
            Words["03_NZ"] = Convert.ToString(bytesToInt(fileInBinary, 8)); // 3
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
            _datalength = Convert.ToInt32(Words["01_NX"]) * Convert.ToInt32(Words["02_NY"]) * Convert.ToInt32(Words["03_NZ"]);
            makeInfo();
            
        }

        public double[] makePlane(List<int[]> coords)
        {
            double[] result = new double[coords.Count];            
            for (int i=0; i < coords.Count; ++i )
            {
                int[] coord = coords[i];
                result[i] = getVal(coord[0], coord[1], coord[2]);
            }
            return result;
        }

        private void makeInfo()
        {
            Info = "";
            foreach (KeyValuePair<string, string> entry in Words)
            {
                Info += entry.Key + "=" + entry.Value + "\n";
            }
        }
        private double getVal(int x, int y, int z)
        {// TODO this should be loading the binary and getting just those it wants
            return _myMatrix[x, y, z];            
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

        private List<Single> bytesToSingles(byte[] bytes)
        {            
            int start = bytes.Length - (4 * _datalength);
            List<Single> matvals = new List<Single>();
            for (int i = start; i < bytes.Length; i += 4)
            {
                Single value = BitConverter.ToSingle(bytes, i);
                matvals.Add(value);
            }
            return matvals;
        }

        private void createMatrix(List<Single> sings,int x, int y, int z)
        {            
            _myMatrix = new double[x, y, z];
            _myMatrixList = new double[_datalength];

            int count = 0;

            for (int i = 0; i < x; ++i)
            {
                for (int j = 0; j < y; ++j)
                {
                    for (int k = 0; k < z; ++k)
                    {
                        double val = sings[count];
                        _myMatrix[i, j, k] = val;
                        _myMatrixList[count] = val;
                        count += 1;
                    }
                }
            }
        }
    }


}
