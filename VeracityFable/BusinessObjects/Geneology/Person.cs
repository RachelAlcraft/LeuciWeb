using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.Geneology
{
    public class Person
    {
        public string Name;
        public DateTime DoB;
        public DateTime DoD;
        public List<Relationship> Relations = new List<Relationship>();
        public string BirthLocation;
        public string LifeLocation;
        public string Occupation;
        public Person(string Name, DateTime dob, string relationships)
        {

        }
    }
}