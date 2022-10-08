using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
   
    public class DensityBinary
    {
        public Dictionary<string, string> Words = new Dictionary<string, string>();
        private string _fileName;
        byte[] _bytes;
        public DensityBinary(string fileName)
        {
            _fileName = fileName;
            _bytes = ReadBinaryFile(_fileName);
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
    }


}
