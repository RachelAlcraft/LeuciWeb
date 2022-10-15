using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace LeuciShared
{
    public class FileDownloads
    {
        public string PdbCode = "";        
        public string PdbViewLink = "";
        public string PdbDownloadLink = "";
        private string PdbFilePath = "";
        public string EmCode = "";
        public string EmViewLink = "";
        public string EmDownloadLink = "";
        public string EmFilePath = "";
        public string DiffViewLink = "";
        public string DiffDownloadLink = "";
        private string DiffFilePath = "";
        public string[] PdbLines = new string[0];
        public string Resolution = "";

        
        public string DensityType = "none";
        // TODO and we would deal with mmCif format here

        public FileDownloads(string pdbcode)
        {
            PdbCode = pdbcode.ToLower();
            EmCode = pdbcode.ToLower();
        }

        public async Task<bool> downloadAll()
        {
            PdbDownloadLink = "https://files.rcsb.org/download/" + PdbCode + ".pdb";
            PdbFilePath = "wwwroot/App_Data/" + PdbCode + ".pdb";
            EmFilePath = "wwwroot/App_Data/" + PdbCode + ".ccp4";
            PdbViewLink = "https://www.ebi.ac.uk/pdbe/entry-files/pdb" + PdbCode + ".ent";
            bool ok = await downloadAsync(PdbFilePath, PdbDownloadLink);
            if (ok)
            {                
                PdbLines = System.IO.File.ReadAllLines(PdbFilePath);
                foreach (string line in PdbLines)
                {//REMARK 900 RELATED ID: EMD-11668   RELATED DB: EMDB
                    if (line.Contains("REMARK 900 RELATED ID:") && line.Contains("EMD-"))
                    {
                        string[] line_split = line.Split(" ");
                        EmCode = line_split[4];
                        DensityType = "cryo-em";
                        break;
                    }
                    else if (line.Contains("2 RESOLUTION."))
                    {//REMARK   2 RESOLUTION.    0.48 ANGSTROMS.    
                        Resolution = line.Substring(26, 5).Trim();
                    }
                    if (line.Substring(0,4) == "ATOM")
                        break;
                }

                if (DensityType == "cryo-em")
                {
                    string[] ems = EmCode.Split("-");
                    EmFilePath = "wwwroot/App_Data/emd_" + ems[1] + ".ccp4";
                    if (!System.IO.File.Exists(EmFilePath))
                    {
                        EmFilePath = "wwwroot/App_Data/emd_" + ems[1] + ".ccp4.gz";
                        EmDownloadLink = "https://ftp.ebi.ac.uk/pub/databases/emdb/structures/EMD-" + ems[1] + "/map/emd_" + ems[1] + ".map.gz";
                        ok = await downloadAsync(EmFilePath, EmDownloadLink);
                        // now the em data is going to be zipped so we neeed to unzip                    
                        FileInfo fi = new FileInfo(EmFilePath);
                        Decompress(fi, "wwwroot/App_Data/emd_" + ems[1] + ".ccp4");
                        EmFilePath = "wwwroot/App_Data/emd_" + ems[1] + ".ccp4";
                        if (System.IO.File.Exists(EmFilePath))
                            if (System.IO.File.Exists("wwwroot/App_Data/emd_" + ems[1] + ".ccp4.gz"))
                                System.IO.File.Delete("wwwroot/App_Data/emd_" + ems[1] + ".ccp4.gz");

                        EmFilePath = "wwwroot/App_Data/emd_" + ems[1] + ".ccp4";
                        // we can delete the zipped version now
                        
                    }
                }
                else
                {
                    EmDownloadLink = "https://www.ebi.ac.uk/pdbe/entry-files/" + EmCode + ".ccp4";
                    ok = await downloadAsync(EmFilePath, EmDownloadLink);
                    if (ok)
                        DensityType = "x-ray";

                }


            }
            return true;

        }



        public async Task<bool> downloadAsync(string filepath, string url)
        {
            bool success = false;
            if (!System.IO.File.Exists(filepath))
            {                
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (var s = await client.GetStreamAsync(url))
                        {
                            using (var fs = new FileStream(filepath, FileMode.CreateNew))
                            {
                                var tasks = s.CopyToAsync(fs);
                                await Task.WhenAll(tasks);
                                success = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        success = false;
                    }
                }
            }
            else
            {
                success = true;
            }
            return success;
        }

        public static void Decompress(FileInfo fileToDecompress,string filetarget)
        {
            if (!System.IO.File.Exists(filetarget))
            { 
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = System.IO.File.Create(newFileName))
                    {
                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                            Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                        }
                    }
                }
            }
        }
    }
}
