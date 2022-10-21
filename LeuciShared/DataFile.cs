using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class DataFiles
    {
        public List<DataFile> Files { get; set; } = new List<DataFile>();
        public DataFiles(string filepath)
        {
            Files.Clear();
            string[] filePaths = Directory.GetFiles(filepath, "*.pdb");
            foreach (string pdbpath in filePaths)
            {
                DataFile df = new DataFile();
                df.PdbCode = pdbpath.Substring(17,4);
                Files.Add(df);
                  
            }
            Files = Files.OrderBy(o => o.PdbCode).ToList();
        }
    }
    
    
    public class DataFile
    {
        public string PdbCode { get; set; } = "";
        public double Resolution { get; set; }
        public double FileSize { get; set; }
    }
}
