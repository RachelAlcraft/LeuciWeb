using Microsoft.Extensions.Hosting.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mime;
using System.Security.Policy;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Leucippus.Models
{
    public class ElectronDensity
    {
        public string? PdbCode { get; set; }
        public string? Info { get; set; }
        public double XX { get; set; }
        public double YY { get; set; }
        public double ZZ { get; set; }
        public double[] MtxX = new double[0];
        public double[] MtxY = new double[0];
        public double[] MtxZ = new double[0];

        public ElectronDensity(string pdb)
        {
            PdbCode = pdb;
            XX = 25;
            YY = 25;
            ZZ = 25;
        }

        public async Task DownloadAsync()
        { 
            string edFilePath = "wwwroot/App_Data/" + PdbCode + ".ccp4";
            bool haveED = true;
            if (!File.Exists(edFilePath))
            {
                string edWebPath = @"https://www.ebi.ac.uk/pdbe/coordinates/files/" + PdbCode + ".ccp4";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (var s = await client.GetStreamAsync(edWebPath))
                        {
                            using (var fs = new FileStream(edFilePath, FileMode.CreateNew))
                            {
                                await s.CopyToAsync(fs);
                            }
                        }
                        
                    }
                    catch(Exception e)
                    {
                        haveED = false;
                    }                    
                }
            }
            if (haveED)
            {
                Ccp4 cp = new Ccp4();
                byte[] fileInBinary = cp.ReadBinaryFile(edFilePath);
                cp.createWords(fileInBinary);
                Info = cp.w01_NX.ToString() + "-" + cp.w02_NY.ToString() + "-" + cp.w03_NZ.ToString();

                int minLength = Math.Min(cp.MyMatrix.GetLength(0), cp.MyMatrix.GetLength(1));
                minLength = Math.Min(minLength,50);

                if (cp.Ready)
                {
                    int startX = Math.Min((int)XX,(int)(cp.MyMatrix.GetLength(0)-minLength));
                    int startY = Math.Min((int)YY, (int)(cp.MyMatrix.GetLength(1) - minLength));                    
                    int posZ = Math.Min((int)ZZ,cp.MyMatrix.GetLength(2));
                    int endX = Math.Min(startX+minLength,cp.MyMatrix.GetLength(0));
                    int endY = Math.Min(startY+minLength, cp.MyMatrix.GetLength(0));
                    
                    MtxX = new double[endX-startX];
                    for (int i = startX; i < endX; i++)
                    {
                        MtxX[i-startX] = i;
                    }
                    MtxY = new double[endY-startY];
                    for (int i = startY; i < endY; i++)
                    {
                        MtxY[i-startY] = i;
                    }                                        
                    MtxZ = new double[MtxX.Length * MtxY.Length];

                    int zIndex = posZ;

                
                    for (int yIndex = 0; yIndex < MtxY.Length; ++yIndex)
                    {
                        double y = MtxY[yIndex];
                        for (int xIndex = 0; xIndex < MtxX.Length; ++xIndex)
                        {
                            double x = MtxX[xIndex];
                            double z = cp.MyMatrix[xIndex, yIndex,zIndex];
                            MtxZ[yIndex * MtxY.Length + xIndex] = z;
                        }
                    }
                }
            }            
        }                
    }
}
