using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class SingleMatches
    {
        public List<SingleMatch> singleMatches { get; set; } = new List<SingleMatch>();
    }
    public class SingleMatch
    {
        /*
         * This class holds the informaiton for each position object to send to the html
         * It might be an individual or a superposition
         * 
         */        
        public string pdbcode { get; set; }
        public string line { get; set; }
        public string distance { get; set; }
        public string key { get; set; }  

        public SingleMatch(string pdb,string lne, string dis,string ky)
        {
            pdbcode = pdb;
            line = lne;
            distance = dis;
            key = ky;
        }
        
    }
}
