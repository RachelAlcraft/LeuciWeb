using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.School
{
    public class School
    {
        public string Name
        {
            get;
            set;
        }
        public int NumPlaces
        {
            get;
            set;
        }
        public bool IsOversubscribed
        {
            get;
            set;
        }

        private List<Pupil> _onRole = new List<Pupil>();
        public bool AddPupil(Pupil p)
        {
            _onRole.Add(p);
            return (_onRole.Count < NumPlaces);
        }
        public List<Pupil> OnRole()
        {
            return _onRole;
        }

        public override string ToString()
        {
            return Name + "\t" + NumPlaces + "\t" + IsOversubscribed;
        }

        public bool StillPlaces()
        {
            return _onRole.Count < NumPlaces;
        }


        public static string GetShortSchoolList()
        {
            //SchoolName	NumberPlaces	IsOverSubscribed
            string schoolList = "Rosy Lane School;3;Y;\n";
            schoolList += "Plane School;2;N;\n";            
            return schoolList;
        }
        public static string GetLongSchoolList()
        {
            string schoolList = "Rosy Lane School;10;Y;\n";
            schoolList += "Lolnew School;20;N;\n";
            schoolList += "Plane School;20;N;\n";
            return schoolList;
        }
    }
}