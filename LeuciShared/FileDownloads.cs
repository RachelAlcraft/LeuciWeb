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
        public string EmNum = "";
        public string EmViewLink = "";
        public string EmDownloadLink = "";
        public string EmFilePath = "";
        public string EmFilePathGz = "";
        public string DiffViewLink = "";
        public string DiffDownloadLink = "";
        public string DiffFilePath = "";
        public string[] PdbLines = new string[0];
        public string Resolution = "";        
        public PdbAtoms PA;


        public string DensityType = "none";
        // TODO and we would deal with mmCif format here

        public FileDownloads(string pdbcode)
        {            
            PdbCode = pdbcode.ToLower();
            EmCode = pdbcode.ToLower();
            PdbFilePath = "wwwroot/App_Data/" + PdbCode + ".pdb";            
            PdbDownloadLink = "https://files.rcsb.org/download/" + PdbCode + ".pdb";
            PdbFilePath = "wwwroot/App_Data/" + PdbCode + ".pdb";
            EmFilePath = "wwwroot/App_Data/" + PdbCode + ".ccp4";
            EmFilePathGz = "wwwroot/App_Data/" + PdbCode + ".ccp4.gz";
            DiffFilePath = "wwwroot/App_Data/" + PdbCode + "_diff.ccp4";
            PdbViewLink = "https://www.ebi.ac.uk/pdbe/entry-files/pdb" + PdbCode + ".ent";
        }

        public async Task<string> existsPdbMatrixAsync()
        {
            bool ok = true;
            if (!System.IO.File.Exists(PdbFilePath))            
                ok = await downloadPdbFile();
            if (ok)
            {
                processPdbFile();
                return "Y";
            }
            else
            {
                return "Error";
            }
        }
        public string existsCcp4Matrix()
        {
            string exists_matrix = "N";
            if (System.IO.File.Exists(EmFilePath))
                exists_matrix = "Y";
            
            if (DiffFilePath != "")
            {
                if (!System.IO.File.Exists(DiffFilePath))
                    exists_matrix = "N";
            }

            string downloading = "N";
            if (System.IO.File.Exists(EmFilePath + ".downloading"))
                downloading = "Y";

            if (System.IO.File.Exists(DiffFilePath + ".downloading"))
                downloading = "Y";

            if (downloading == "Y")
                return "still downloading";
            else if (exists_matrix == "Y")
            {
                processCcp4File();
                return "Y";
            }
            else
            {
                _ = downloadCcp4File();
                return "starting download";
            }
        }
        public async Task<bool> downloadPdbFile()
        {            
            bool ok = await downloadAsync(PdbFilePath, PdbDownloadLink);
            return ok;
        }
        public void processPdbFile()
        { 
            PdbLines = System.IO.File.ReadAllLines(PdbFilePath);
            DensityType = "x-ray";
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
                if (line.Substring(0, 4) == "ATOM")
                    break;
            }

            if (DensityType == "cryo-em")
            {
                string[] ems = EmCode.Split("-");
                EmNum = ems[1];                                
                EmFilePath = "wwwroot/App_Data/emd_" + EmNum + ".ccp4";
                EmFilePathGz = "wwwroot/App_Data/emd_" + EmNum + ".ccp4.gz";
                EmDownloadLink = "https://ftp.ebi.ac.uk/pub/databases/emdb/structures/EMD-" + EmNum + "/map/emd_" + EmNum + ".map.gz";                                                            
            }
            else
            {
                EmDownloadLink = "https://www.ebi.ac.uk/pdbe/entry-files/" + EmCode + ".ccp4";                    
                DensityType = "x-ray";
                DiffDownloadLink = "https://www.ebi.ac.uk/pdbe/entry-files/" + EmCode + "_diff.ccp4";
            }

            PA = new PdbAtoms(PdbLines);
        }

        public async Task<bool> downloadCcp4File()
        {            
            bool ok = false;
            if (DensityType == "cryo-em")
            {
                if (!System.IO.File.Exists(EmFilePath))
                {
                    ok = await downloadAsync(EmFilePathGz, EmDownloadLink);
                    // now the em data is going to be zipped so we neeed to unzip                    
                    FileInfo fi = new FileInfo(EmFilePathGz);
                    Decompress(fi, EmFilePath);                    
                    if (System.IO.File.Exists(EmFilePath))
                        if (System.IO.File.Exists(EmFilePathGz))
                            System.IO.File.Delete(EmFilePathGz);                    
                    DiffFilePath = ""; //there is no difference density for cryo-em
                                       // we can delete the zipped version now

                }
            }
            else
            {
                ok = await downloadAsync(EmFilePath, EmDownloadLink);
                if (ok)
                    ok = await downloadAsync(DiffFilePath, DiffDownloadLink);
                if (ok)
                    DensityType = "x-ray";
            }
                                    
            return true;
        }
        public void processCcp4File()
        {
            if (DensityType == "cryo-em")
                DiffFilePath = ""; //there is no difference density for cryo-em
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
                        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));                        
                        if (response != null)
                        {
                            string ctn_len = Convert.ToString(response.Content.Headers.ContentLength);

                            if (!System.IO.File.Exists(filepath + ".downloading"))
                                using (StreamWriter sw = new StreamWriter(filepath + ".downloading"))
                                    sw.WriteLine(ctn_len);

                            using (var s = await client.GetStreamAsync(url))
                            {
                                using (var fs = new FileStream(filepath, FileMode.CreateNew))
                                {
                                    var tasks = s.CopyToAsync(fs);
                                    await Task.WhenAll(tasks);
                                    success = true;
                                }
                            }
                            
                            if (System.IO.File.Exists(filepath + ".downloading"))
                                System.IO.File.Delete(filepath + ".downloading");

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
