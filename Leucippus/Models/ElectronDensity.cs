using Microsoft.Extensions.Hosting.Internal;
using NuGet.Packaging;
using System;
using System.Collections;
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
        public string? EbiLink { get; set; }
        public string? Ccp4Link { get; set; }
        public Ccp4? ccp4 { get; set; }
        public string? Info { get; set; }
        public double XX { get; set; }
        public double YY { get; set; }
        public double ZZ { get; set; }
        public double[] MtxA = new double[0];
        public double[] MtxB = new double[0];
        public double[] MtxC = new double[0];
        public double[][] MtxD;
        //Variables to view the plane with no interpolation
        public int Layer;
        public int LayerMax;
        public string Plane;
        public double MinV;
        public double MaxV;

        DllInterface dllI = new DllInterface();

        public ElectronDensity(string pdb)
        {
            PdbCode = pdb;
            EbiLink = "https://www.ebi.ac.uk/pdbe/entry/pdb/" + PdbCode;            
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
                ccp4 = new Ccp4();
                byte[] fileInBinary = ccp4.ReadBinaryFile(edFilePath);
                ccp4.createWords(fileInBinary);
                Info = ccp4.w01_NX.ToString() + "-" + ccp4.w02_NY.ToString() + "-" + ccp4.w03_NZ.ToString();

                int minLength = Math.Min(ccp4.MyMatrix.GetLength(0), ccp4.MyMatrix.GetLength(1));
                minLength = Math.Min(minLength,50);

                
            }            
        }
        public void calculateWholeLayer(string plane, int layer)
        {
            if (ccp4 != null)
            {
                if (ccp4.Ready)
                {                    
                    int eX = ccp4.MyMatrix.GetLength(0);
                    int eY = ccp4.MyMatrix.GetLength(1);
                    int eZ = ccp4.MyMatrix.GetLength(2);
                    MinV = 0;
                    MaxV = 0;

                    
                    int endX = 0;                    
                    int endY = 0;
                    if (layer < 0)                    
                        layer = 0;

                    if (plane == "XY")
                    {
                        endX = eX;
                        endY = eY;
                        LayerMax = eZ;
                        if (layer >= eZ)                        
                            layer = eZ - 1;                        
                    }
                    else if (plane == "YZ")
                    {
                        endX = eY;
                        endY = eZ;
                        LayerMax = eX;
                        if (layer >= eX)                        
                            layer = eX - 1;                        
                    }
                    else if (plane == "ZX")
                    {
                        endX = eZ;
                        endY = eX;
                        LayerMax = eY;
                        if (layer >= eY)                        
                            layer = eY - 1;                        
                    }

                    MtxA = new double[endX];
                    for (int i = 0; i < endX; i++)
                    {
                        MtxA[i] = i;
                    }
                    MtxB = new double[endY];
                    for (int i = 0; i < endY; i++)
                    {
                        MtxB[i] = i;
                    }
                    MtxC = new double[MtxA.Length * MtxB.Length];
                    MtxD = new double[MtxB.Length][];

                    for (int y = 0; y < MtxB.Length; ++y)
                    {
                        MtxD[y] = new double[MtxA.Length];
                        for (int x = 0; x < MtxA.Length; ++x)
                        {                            
                            double val = 0;
                            if (plane == "XY")                            
                                val = ccp4.MyMatrix[x,y,layer];                            
                            else if (plane == "YZ")
                                val = ccp4.MyMatrix[layer,x,y];                            
                            else if (plane == "ZX")
                                val = ccp4.MyMatrix[y,layer,x];                            
                            MtxC[y*MtxA.Length+x] = val;
                            MtxD[y][x] = val;
                            MinV = Math.Min(MinV, val);
                            MaxV = Math.Max(MaxV, val);
                            //MaxV += val;
                        }
                    }
                    //int pos = (int)(MtxC.Length - MtxC.Length / 100);
                    //Array.Sort(MtxC);
                    //MaxV = MtxC[pos]; //percential the cap on density
                    Layer = layer;
                    Plane = plane;
                    
                }
            }
        }
    }
}
