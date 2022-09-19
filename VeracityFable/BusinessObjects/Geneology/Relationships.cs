using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.Geneology
{//this will be a singleton object
    public class Relationships
    {
         private List<string> _relationships = new List<string>();
        public Relationships()
        {//Here we add the possible relationships in the format type:age degree, relation degree, marriage plane (3d on generation, relation and marriage)
            //We have the option of 3d, where 2d we need to use the 3rd dim in the 2nd - ie a spouse is marked next to them like a sibling.
            //it doesn;lt fully make sense, using 3d daughters would have a half offset? Stick with 2d for now!
            _relationships.Add("Father:1:0:0");                 
            _relationships.Add("Mother:1:0:0");
            _relationships.Add("Sister:0:0:0");
            _relationships.Add("Brother:0:0:0");
            _relationships.Add("Daughter:-1:0:0)");
            _relationships.Add("Son:-1:0:0");
            _relationships.Add("Cousin:0:1:0");
            _relationships.Add("Aunt:1:1:0");
            _relationships.Add("Uncle:1:1:0");
            _relationships.Add("Niece:-1:1:0");
            _relationships.Add("Nephew:-1:1:0");
            _relationships.Add("HalfBrother:0:0:0");
            _relationships.Add("HalfSister:0:0:0");
            _relationships.Add("Grandfather:2:0:0");
            _relationships.Add("Grandmother:2:0:0");
            _relationships.Add("Granddaughter:-2:0:0");
            _relationships.Add("Grandson:-2:0:0");
            _relationships.Add("GreatAunt:2:1:0");
            _relationships.Add("GreatUncl:2:1:0");
            _relationships.Add("GreatNiece:-2:1:0");
            _relationships.Add("GreatNephew:-2:1:0");
            _relationships.Add("Married:0:0:1");
            _relationships.Add("Partner:0:0:1");
            //perhaps colour the boxes and lines of different marriedoobles
        }

        public int GetVerticalOffset(Person one, Person two)
        {
            return 0;
        }

        public int GetHorizontalOffset(Person one, Person two)
        {//this both gets and applies the horizontal offset to person 2 from person 1 who has already had it applied
            return 0;
        }
    }
}