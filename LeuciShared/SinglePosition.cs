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
        public string name { get; set; }
        public double[][]? densityMatrix { get; set; }
        public double[][]? radientMatrix { get; set; }
        public double[][]? laplacianMatrix { get; set; }
        public double[]? xAxis { get; set; }
        public double[]? yAxis { get; set; }
        public double minD { get; set; }
        public double maxD { get; set; }
        public double minL { get; set; }
        public double maxL { get; set; }

    }
}
