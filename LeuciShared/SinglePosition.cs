using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class SinglePosition
    {
        /*
         * This class holds the informaiton for each position object to send to the html
         * It might be an individual or a superposition
         * 
         */
        public string contourDen { get; set; }
        public string contourRad { get; set; }
        public string contourLap { get; set; }
        public string description { get; set; }  
        public double[][]? radientMatrix { get; set; }
        public double[][]? laplacianMatrix { get; set; }
        public double[]? xAxis { get; set; }
        public double[]? yAxis { get; set; }
        public double minD { get; set; }
        public double maxD { get; set; }
        public double minL { get; set; }
        public double maxL { get; set; }
        public double dcap { get; set; }
        public double dfloor { get; set; }
        public string cbar { get; set; }
        public string hue { get; set; }
        public double[][]? densityMatrix { get; set; }        
        public void copyDensity(double[][] den)
        {
            densityMatrix = new double[den.Length][];
            for (int i = 0; i < den.Length; i++)
            {
                densityMatrix[i] = new double[den.Length];
                for (int j = 0; j < den[i].Length; j++)
                {
                    densityMatrix[i][j] = den[i][j];
                }
            }
        }
        public void copyRadient(double[][] rad)
        {
            radientMatrix = new double[rad.Length][];
            for (int i = 0; i < rad.Length; i++)
            {
                radientMatrix[i] = new double[rad.Length];
                for (int j = 0; j < rad[i].Length; j++)
                {
                    radientMatrix[i][j] = rad[i][j];
                }
            }
        }
        public void copyLaplacian(double[][] lap)
        {
            laplacianMatrix = new double[lap.Length][];
            for (int i = 0; i < lap.Length; i++)
            {
                laplacianMatrix[i] = new double[lap.Length];
                for (int j = 0; j < lap[i].Length; j++)
                {
                    laplacianMatrix[i][j] = lap[i][j];
                }
            }
        }
 
        public static SinglePosition makeFromFlat(string xAxis, string yAxis, string density, string radient, string laplacian, 
            double minD, double maxD, double minL, double maxL,
            double dfloor, double dcap, string hue, string cbar)
        {
            SinglePosition sliceF = new SinglePosition();
            sliceF.xAxis = Helper.listFromString(xAxis);
            sliceF.yAxis = Helper.listFromString(yAxis);
            sliceF.densityMatrix = Helper.unwrapList(density);
            sliceF.radientMatrix = Helper.unwrapList(radient);
            sliceF.laplacianMatrix = Helper.unwrapList(laplacian);
            sliceF.minD = minD;
            sliceF.minL = minL;
            sliceF.maxD = maxD;
            sliceF.maxL = maxL;
            sliceF.dfloor = dfloor;
            sliceF.dcap = dcap;
            sliceF.hue = hue;
            sliceF.cbar = cbar;
            return sliceF;
        }




    }
}
