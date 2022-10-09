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
        public string PdbCode { get; set; }
        public string? EbiLink { get; set; }
        public string? Ccp4Link { get; set; }
        public string? EdFile { get; set; }
        public Ccp4? ccp4 { get; set; }
        public string? Info { get; set; }
        public double CX { get; set; }
        public double CY { get; set; }
        public double CZ { get; set; }
        public double LX { get; set; }
        public double LY { get; set; }
        public double LZ { get; set; }
        public double PX { get; set; }
        public double PY { get; set; }
        public double PZ { get; set; }
        public double[] MtxA = new double[0];
        public double[] MtxB = new double[0];
        public double[] MtxC = new double[0];
        public double[][]? MtxD;
        public double[][]? SliceDensity;
        public double[][]? SliceRadiant;
        public double[][]? SliceLaplacian;
        public double[] SliceAxis = new double[0];
        //Variables to view the plane with no interpolation
        public int Layer;
        public int LayerMax;
        public string? Plane;
        public double MinV;
        public double MaxV;
        private bool haveED = false;

        DllInterface? dllI = null;// new DllInterface("","");        

        public ElectronDensity(string pdb)
        {
            PdbCode = pdb;
            EbiLink = "https://www.ebi.ac.uk/pdbe/entry/pdb/" + PdbCode;
            Layer = 0;
            Plane = "XY";            
        }        
        public async Task DownloadAsync()
        {
            EdFile = "wwwroot/App_Data/" + PdbCode + ".ccp4";            
            if (!File.Exists(EdFile))
            {
                string edWebPath = @"https://www.ebi.ac.uk/pdbe/coordinates/files/" + PdbCode + ".ccp4";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        using (var s = await client.GetStreamAsync(edWebPath))
                        {
                            using (var fs = new FileStream(EdFile, FileMode.CreateNew))
                            {
                                var tasks = s.CopyToAsync(fs);
                                await Task.WhenAll(tasks);
                                haveED = true;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        haveED = false;
                    }
                }                
            }
            else
            {
                haveED = true;
            }            
        }
        public void sendToDll()
        { 
            if (haveED)
            {
                //finish the creation
                ccp4 = new Ccp4();
                byte[] fileInBinary = ccp4.ReadBinaryFile(EdFile);
                ccp4.createWords(fileInBinary);
                Info = "Dimensions= " + ccp4.InfoDim + "<br/>Cell lengths= " + ccp4.InfoCellLengths + "<br/>Cell angles= " + ccp4.InfoCellAngles;

                // call out to the c++ dll
                dllI = new DllInterface(ccp4.MyVector,PdbCode, "");
                dllI.setWords(
                    ccp4.w01_NX, ccp4.w02_NY, ccp4.w03_NZ, ccp4.w04_MODE,
                    ccp4.w05_NXSTART, ccp4.w06_NYSTART, ccp4.w07_NZSTART,
                    ccp4.w08_MX, ccp4.w09_MY, ccp4.w10_MZ,
                    ccp4.w11_CELLA_X, ccp4.w12_CELLA_Y, ccp4.w13_CELLA_Z,
                    ccp4.w14_CELLB_X, ccp4.w15_CELLB_Y, ccp4.w16_CELLB_Z,
                    ccp4.w17_MAPC, ccp4.w18_MAPR, ccp4.w19_MAPS);

                int minLength = Math.Min(ccp4.MyMatrix.GetLength(0), ccp4.MyMatrix.GetLength(1));
                minLength = Math.Min(minLength,50);                  
            }            
        }
        public void getSlice()
        {
            double width = 5;
            double gap = 0.1;
            int nums = (int)(width / gap) + 1;

            if (dllI == null)
                sendToDll();


            if (dllI != null)
            {
                dllI.createSlice(CX, CY, CZ, LX, LY, LZ, PX, PY, PZ, width, gap);
                SliceDensity = dllI.getSlice(width, gap);
                SliceRadiant = dllI.getRadiantSlice(width, gap);
                SliceLaplacian = dllI.getLaplacianSlice(width, gap);
                SliceAxis = dllI.getAxis(width, gap);

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

                    MtxA = new double[endY];
                    for (int i = 0; i < endY; i++)
                    {
                        MtxA[i] = i;
                    }
                    MtxB = new double[endX];
                    for (int i = 0; i < endX; i++)
                    {
                        MtxB[i] = i;
                    }
                    MtxC = new double[MtxA.Length * MtxB.Length];
                    MtxD = new double[MtxB.Length][];

                    for (int x = 0; x < MtxB.Length; ++x)
                    {
                        MtxD[x] = new double[MtxA.Length];
                        for (int y = 0; y < MtxA.Length; ++y)
                        {                            
                            double val = 0;
                            if (plane == "XY")                            
                                val = ccp4.MyMatrix[x,y,layer];                            
                            else if (plane == "YZ")
                                val = ccp4.MyMatrix[layer,x,y];                            
                            else if (plane == "ZX")
                                val = ccp4.MyMatrix[y,layer,x];                            
                            MtxC[x*MtxA.Length+y] = val;
                            MtxD[x][y] = val;
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
