using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace VeracityFable.BusinessObjects.School
{
    abstract public class AllocMethod
    {
        private double _randomRadius = 0;
        private double _maxSibling = 0;
        protected double _maxChoices = 0;        
        protected Dictionary<string,Pupil> _pupils = new Dictionary<string,Pupil>();
        protected Dictionary<string,School> _schoolsFull = new Dictionary<string,School>();
        protected Dictionary<string, School> _schoolsNotFull = new Dictionary<string,School>();        

        public void SetRandomRadius(double radius)
        {
            _randomRadius = radius;
        }
        public void SetMaxSibling(double maxSib)
        {
            _maxSibling = maxSib;
        }
        public void SetMaxChoices(double maxChoice)
        {
            _maxChoices = maxChoice;
        }

        public string ErrorReason
        {
            get;
            set;
        }

        public void SetSchools(string schools)
        {
            string[] schoolsList = schools.Split('\n');
            foreach (string s in schoolsList)
            {
                string[] oneSchool = s.Split(';');
                if (oneSchool.Length > 2)
                {

                    string SchoolName = oneSchool[0];
                    int NumPlaces = Convert.ToInt32(oneSchool[1]);
                    bool OverSub = oneSchool[2].ToUpper() == "Y" ? true : false;

                    School sch = new School();
                    sch.Name = SchoolName;
                    sch.NumPlaces = NumPlaces;
                    sch.IsOversubscribed = OverSub;
                    _schoolsNotFull.Add(SchoolName,sch);
                }
            }
        }

        public void SetPupils(string pupils)
        {
            string[] pupilsList = pupils.Split('\n');
            foreach (string s in pupilsList)
            {
                try
                {
                    string[] onePupil = s.Split(';');

                    if (onePupil.Length > 4)
                    {
                        string PupilName = onePupil[0];                       
                        if (_pupils.ContainsKey(PupilName))
                        {                            
                            ErrorReason += "Already exists: " + s + "\n";
                        }
                        else
                        {                            
                            Pupil p = new Pupil(s);
                            _pupils.Add(PupilName, p);
                        }
                    }                    
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    ErrorReason += "Error:" + msg + ":" + s + "\n";
                    int i = 0;
                    ++i;
                }
                    
            }
        }

        public void Allocate()
        {
            Random rdm = new Random();
            int iChoice = 0; //choice;
            while (iChoice < _maxChoices)
            {
                ++iChoice;
                if (_schoolsNotFull.Count > 0)
                {
                    List<School> schoolQueue = _schoolsNotFull.Values.ToList();
                    //*** FIRST PASS
                    //find a random school until the list is empty
                    while (schoolQueue.Count > 0)
                    {
                        int ranNum = rdm.Next(schoolQueue.Count);
                        School s = schoolQueue[ranNum];
                        schoolQueue.RemoveAt(ranNum);
                        //find all the children in SpecialNeeds
                        if (s.StillPlaces())
                        {
                            List<Pupil> specialReasons = GetSpecialPupilsList(s.Name, iChoice);
                            for (int i = 0; i < specialReasons.Count; ++i)
                            {
                                if (s.StillPlaces())
                                {
                                    Pupil p = specialReasons[i];
                                    p.Reason = "Special";
                                    s.AddPupil(p);
                                    _pupils.Remove(p.Name);
                                }
                            }
                        }
                        //find all the relevant siblings                                                            
                        if (s.StillPlaces())
                        {
                            List<Pupil> siblings = GetSiblingPupilsList(s.Name, iChoice);
                            for (int i = 0; i < siblings.Count; ++i)
                            {
                                if (s.StillPlaces())
                                {
                                    Pupil p = siblings[i];
                                    p.Reason = "Sibling";
                                    s.AddPupil(p);
                                    _pupils.Remove(p.Name);
                                }
                            }

                        }
                        //find all the only choices                    
                        if (s.StillPlaces())
                        {
                            List<Pupil> onlyChoices = GetOnlySchoolPupilsList(s.Name, iChoice);
                            for (int i = 0; i < onlyChoices.Count; ++i)
                            {
                                if (s.StillPlaces())
                                {
                                    Pupil p = onlyChoices[i];
                                    p.Reason = "Only choice";
                                    s.AddPupil(p);
                                    _pupils.Remove(p.Name);
                                }
                            }

                        }
                        //randomly find all the 1st choices                    
                        if (s.StillPlaces())
                        {
                            List<Pupil> firstChoices = GetSchoolChoicePupilsList(s.Name, iChoice);
                            for (int i = 0; i < firstChoices.Count; ++i)
                            {
                                if (s.StillPlaces())
                                {
                                    Pupil p = firstChoices[i];
                                    p.Reason = "Choice " + iChoice;
                                    s.AddPupil(p);
                                    _pupils.Remove(p.Name);
                                }
                            }

                        }
                        //If the school is full move lists from NotFulltofull
                        if (s.StillPlaces() == false)
                        {
                            RemoveSchoolFromChoices(s.Name);
                            _schoolsNotFull.Remove(s.Name);
                            _schoolsFull.Add(s.Name, s);
                        }
                    }
                }
            }


            //At this point we have been through all the choices of those within the random radius.
            //We can now process any children left by distance.
            //now we Process a child still without a place that didnt get any choices by distance            
            List<School> notFull = SortByMethod(_schoolsNotFull.Values.ToList());
            //List<Pupil> notPlaced = SortByMethod(_pupils.Values.ToList());
            List<Pupil> notPlaced = _pupils.Values.ToList();
            for (int i = 0; i < _schoolsNotFull.Count; ++i)
            {
                School s = notFull[i];
                List<Pupil> viableList = GetViableSchoolPupilsList(s.Name);
                for (int j = 0; j < viableList.Count; ++j)
                {
                    if (s.StillPlaces())
                    {
                        Pupil p = viableList[j];
                        p.Reason = "From final pool";
                        s.AddPupil(p);
                        _pupils.Remove(p.Name);
                    }
                }

                //If the school is full move lists from NotFulltofull
                if (s.StillPlaces() == false)
                {
                    _schoolsNotFull.Remove(s.Name);
                    _schoolsFull.Add(s.Name, s);
                }
            }
        }

        protected List<Pupil> GetSpecialPupilsList(string schoolName, int choice)
        {
            Dictionary<Pupil, SchoolChoice> pups = new Dictionary<Pupil, SchoolChoice>();
            foreach (Pupil p in _pupils.Values)
            {
                SchoolChoice sc = null;
                if (p.GetChoice(choice, ref sc))
                {
                    if (sc.SchoolName == schoolName && sc.IsSpecial)
                    {
                        pups.Add(p,sc);
                        p.RemoveChoice(schoolName);
                    }
                }
            }
            return SortByMethod(pups);
        }

        protected List<Pupil> GetSiblingPupilsList(string schoolName, int choice)
        {
            Dictionary<Pupil, SchoolChoice> pups = new Dictionary<Pupil, SchoolChoice>();
            foreach (Pupil p in _pupils.Values)
            {
                SchoolChoice sc = null;
                if (p.GetChoice(choice, ref sc))
                {
                    if (sc.SchoolName == schoolName && sc.IsSibling && sc.IsWithinSiblingMax(_maxSibling))
                    {
                        pups.Add(p, sc);
                        p.RemoveChoice(schoolName);
                    }
                }
            }
            return SortByMethod(pups);
        }

        protected List<Pupil> GetOnlySchoolPupilsList(string schoolName, int choice)
        {
            Dictionary<Pupil, SchoolChoice> pups = new Dictionary<Pupil, SchoolChoice>();
            foreach (Pupil p in _pupils.Values)
            {
                SchoolChoice sc = null;
                if (p.GetChoice(choice, ref sc))
                {
                    if (sc.SchoolName == schoolName && p.IslastChoice() && p.InBorough)
                    {
                        pups.Add(p, sc);
                        p.RemoveChoice(schoolName);
                    }
                }
            }
            return SortByMethod(pups);
        }

        protected List<Pupil> GetSchoolChoicePupilsList(string schoolName, int choice)
        {
            Dictionary<Pupil, SchoolChoice> pups = new Dictionary<Pupil, SchoolChoice>();
            foreach (Pupil p in _pupils.Values)//BUG with the last chouice, distance away and sorting the choices where they are X
            {
                SchoolChoice sc = null;
                if (p.GetChoice(choice, ref sc))
                {
                    if (sc.SchoolName == schoolName && sc.IsWithinRandomRadius(_randomRadius))
                    {
                        pups.Add(p, sc);
                        p.RemoveChoice(schoolName);
                    }
                }
            }
            return SortByMethod(pups);
        }

        protected List<Pupil> GetViableSchoolPupilsList(string schoolName)
        {
            Dictionary<Pupil, SchoolChoice> sortPupils = new Dictionary<Pupil, SchoolChoice>();
            foreach (Pupil p in _pupils.Values)
            {
                if (p.InBorough)
                {
                    List<SchoolChoice> choices = p.GetNotChoices();
                    foreach (SchoolChoice sc in choices)
                    {
                        if (sc.SchoolName == schoolName)
                        {
                            p.RemoveChoice(schoolName);
                            sortPupils.Add(p, sc);
                        }
                    }
                }
                else
                {
                    p.Reason = "Not in borough";
                }
            }
            return DistanceSort(sortPupils);
        }

        protected abstract List<School> SortByMethod(List<School> schools);
        protected abstract List<Pupil> SortByMethod(Dictionary<Pupil, SchoolChoice> pupils);

        public void RemoveSchoolFromChoices(string schoolName)
        {
            foreach (Pupil p in _pupils.Values)
            {
                p.RemoveChoice(schoolName);
            }
        }

        public string GetSchoolAllocation()
        {
            string schoolsAlloc = "Schools that are full:\n";
            foreach (School s in _schoolsFull.Values)
            {
                schoolsAlloc += s.ToString() + "\n";
                foreach (Pupil p in s.OnRole())
                {
                    schoolsAlloc += "\t" + p.ToAlloAlgoString(s.Name) + "\n";
                }
            }

            schoolsAlloc += "Schools that still have places:\n";
            foreach (School s in _schoolsNotFull.Values)
            {
                schoolsAlloc += s.ToString() + "\n";
                foreach (Pupil p in s.OnRole())
                {
                    schoolsAlloc += "\t" + p.ToAlloAlgoString(s.Name) + "\n";
                }
            }

            schoolsAlloc += "Pupils with no assigned place:\n";            
            foreach (Pupil p in _pupils.Values)
            {
                schoolsAlloc += "\t" + p.ToString() + "\n";
            }
            
            return schoolsAlloc;
        }

        protected List<Pupil> DistanceSort(Dictionary<Pupil, SchoolChoice> pupils)
        {
            List<Pupil> newListPupils = new List<Pupil>();
            List<SchoolChoice> newListChoices = new List<SchoolChoice>();
            foreach (var pPair in pupils)
            {
                Pupil p = pPair.Key;
                double dist = pPair.Value.Distance;
                bool bInserted = false;
                for (int i = 0; i < newListPupils.Count; ++i)
                {
                    Pupil pComp = newListPupils[i];
                    double distComp = newListChoices[i].Distance;
                    if (dist < distComp)
                    {
                        bInserted = true;
                        newListPupils.Insert(i, p);
                        newListChoices.Insert(i, pPair.Value);
                        break;
                    }
                }
                if (!bInserted)
                {
                    newListPupils.Add(p);
                    newListChoices.Add(pPair.Value);
                }
            }
            return newListPupils;
        }

        public int UpdateCalculationCounter()
        {
            int count = Convert.ToInt32(HttpContext.Current.Application["UserCount"]);            
            ++count;
            HttpContext.Current.Application["UserCount"] = count;            
            return count;
            
        }
    }
}