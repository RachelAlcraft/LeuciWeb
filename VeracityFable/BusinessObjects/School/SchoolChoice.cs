using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.School
{
    public class SchoolChoice
    {
        public string SchoolName
        {
            get;
            set;
        }
        public bool IsSpecial 
        {
            get;
            set;
        }
        public bool IsSibling
        {
            get;
            set;
        }
        public bool IsWithinSiblingMax(double sibMax)
        {                        
            return (Distance <= sibMax);                           
        }
        public int ChoiceNumber
        {
            get;
            set;
        }

        public double Distance
        {
            get;
            set;
        }
        public bool IsWithinRandomRadius(double ranRadius)
        {            
            return (Distance <= ranRadius);            
        }
    }
}