using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.School
{
    public class AllocRandomRadius:AllocMethod
    {




        protected override List<Pupil> SortByMethod(Dictionary<Pupil, SchoolChoice> pupils)
        {
            Random rdm = new Random();
            List<Pupil> newListPupils = new List<Pupil>();
            List<Pupil> oldListPupils = pupils.Keys.ToList();                        
            while (oldListPupils.Count > 0)
            {
                int ranNum = rdm.Next(oldListPupils.Count);
                Pupil p = oldListPupils[ranNum];
                oldListPupils.RemoveAt(ranNum);
                newListPupils.Add(p);
            }
            return newListPupils;
        }

        protected override List<School> SortByMethod(List<School> schools)
        {
            Random rdm = new Random();
            List<School> newListSchool = new List<School>();
            List<School> oldListSchool = schools;
            while (oldListSchool.Count > 0)
            {
                int ranNum = rdm.Next(oldListSchool.Count);
                School s = oldListSchool[ranNum];
                oldListSchool.RemoveAt(ranNum);
                newListSchool.Add(s);
            }
            return newListSchool;
        }

          
    }
}