using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.School
{
    public class AllocDistance:AllocMethod 
    {
               
        protected override List<School> SortByMethod(List<School> schools)
        {
            return schools;
        }

        protected override List<Pupil> SortByMethod(Dictionary<Pupil, SchoolChoice> pupils)
        {
            return DistanceSort(pupils);
        }
    }
}