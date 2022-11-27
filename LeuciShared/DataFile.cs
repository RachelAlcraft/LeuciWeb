namespace LeuciShared
{
    public class DataFiles
    {
        public List<DataFile> Files { get; set; } = new List<DataFile>();
        public List<DataFile> BbkPdbs { get; set; } = new List<DataFile>();
        public List<DataFile> SmallPdbs { get; set; } = new List<DataFile>();
        public List<DataFile> HighPdbs { get; set; } = new List<DataFile>();
        public List<DataFile> SmallEmPdbs { get; set; } = new List<DataFile>();
        public List<DataFile> HighEmPdbs { get; set; } = new List<DataFile>();
        public DataFiles(string filepath)
        {
            Files.Clear();
            string[] filePaths = Directory.GetFiles(filepath, "*.pdb");
            foreach (string pdbpath in filePaths)
            {
                DataFile df = new DataFile(pdbpath.Substring(17, 4));
                Files.Add(df);

            }
            Files = Files.OrderBy(o => o.PdbCode).ToList();

            BbkPdbs.Add(new DataFile("2bf9"));

            SmallPdbs.Add(new DataFile("6eex"));
            SmallPdbs.Add(new DataFile("6efg"));
            SmallPdbs.Add(new DataFile("6fgz"));

            HighPdbs.Add(new DataFile("1ejg"));
            HighPdbs.Add(new DataFile("3nir"));
            HighPdbs.Add(new DataFile("5d8v"));

            SmallEmPdbs.Add(new DataFile("3j9e"));

            HighEmPdbs.Add(new DataFile("7a6a"));

        }
    }


    public class DataFile
    {
        public string PdbCode { get; set; } = "";
        public double Resolution { get; set; }
        public double FileSize { get; set; }

        public DataFile(string pdbcode)
        {
            PdbCode = pdbcode;
        }
    }
}
