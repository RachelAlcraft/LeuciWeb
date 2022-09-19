using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace VeracityFable.BusinessObjects.School
{
    public class Pupil
    {
        public Pupil(string line)
        {//PupilName;SchoolChoice;ChoiceNumber;DistanceFromSchool;SpecialReason;Sibling;SocialBracket;
            //PupilName;SocialBracket;Sex;InBorough;SchoolChoice,ChoiceNumber,DistanceFromSchool,SpecialReason,Sibling;SchoolChoice,,,            
            string[] parsed = line.Split(';');
            if (parsed.Length > 4)
            {
                Name = parsed[0];
                SocialBracket = parsed[1];
                Sex = parsed[2];
                InBorough = parsed[3].ToUpper() == "Y" ? true : false;
                for (int i = 4; i < parsed.Length; ++i)
                {//SchoolChoice,ChoiceNumber,DistanceFromSchool,SpecialReason,Sibling
                    if (parsed[i].Length > 2)//just making sure its not a delim
                    {
                        string[] choices = parsed[i].Split(',');
                        if (choices.Length > 3)
                        {
                            string SchoolName = choices[0];
                            int ChoiceNumber = Convert.ToInt32(choices[1]);
                            double DistanceFromSchool = Convert.ToDouble(choices[2]);
                            string SpecialReason = choices[3].ToUpper();
                            string Sibling = choices[4].ToUpper();
                            AddChoice(SchoolName, ChoiceNumber, SpecialReason, Sibling, DistanceFromSchool);
                        }
                    }
                }
            }
        }

        public string Name
        {
            get;
            set;
        }
        public string Sex
        {
            get;
            set;
        }
        public string SocialBracket
        {
            get;
            set;
        }
        public string Reason
        {
            get;
            set;
        }
        public bool InBorough
        {
            get;
            set;
        }
        private List<SchoolChoice> _choices = new List<SchoolChoice>();
        public void AddChoice(
            string schoolName, 
            int choiceNumber,
            string isSpecial, 
            string isSibling, 
            double distance)
        {
        SchoolChoice sc = new SchoolChoice();
        sc.SchoolName = schoolName;
        sc.IsSpecial = isSpecial.ToUpper()=="Y"?true:false;
        sc.IsSibling =  isSibling.ToUpper()=="Y"?true:false;        
        sc.ChoiceNumber = choiceNumber;
        sc.Distance = distance;         
        _choices.Add(sc);        
        }
        public bool IslastChoice()
        {
            return _choices.Count == 1;
        }

        
        public string ToAlloAlgoString(string name)
        {
            string schoolBit = "";
            foreach (SchoolChoice sc in _choices)
            {
                if (sc.SchoolName == name)
                    schoolBit = sc.SchoolName + "\t" + sc.Distance + "\t" + sc.IsSibling + "\t" + sc.IsSpecial;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            while (sb.Length < 10)
                sb.Append(".");
            return sb.ToString() + "\t" + Sex + "\t" + SocialBracket + "\t" + InBorough + "\t" + schoolBit + "\t" + Reason;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            while (sb.Length < 8)
                sb.Append(" ");
            return sb.ToString() + "\t" + Sex + "\t" + SocialBracket + "\t" + InBorough + "\t" + Reason;
        }

        public bool GetChoice(int choice, ref SchoolChoice schoolChoice)
        {
            schoolChoice = null;
            foreach (SchoolChoice sc in _choices)
            {
                if (sc.ChoiceNumber == choice)
                {
                    schoolChoice = sc;
                    return true;
                }                    
            }            
            return false;
        }
        public List<SchoolChoice> GetNotChoices()
        {
            List<SchoolChoice> choices = new List<SchoolChoice>();
            foreach (SchoolChoice sc in _choices)
            {
                if (sc.ChoiceNumber == -1)
                {
                    choices.Add(sc);
                }
            }
            return choices;
        }
        public void RemoveChoice(string schoolName)
        {            
            int remAt = -1;
            for (int i = 0; i < _choices.Count; ++i)
            {
                SchoolChoice sc = _choices[i];
                if (sc.SchoolName == schoolName)
                {
                    remAt = i;                                        
                }                    
            }
            if (remAt > -1)
            {
                _choices.RemoveAt(remAt);
            }
        }

        public static string GetShortPupilList()
        {
                                //PupilName     SchoolChoice            ChoiceNumber    DistanceFromSchool  SpecialReason   Sibling SocialBracket   M/F InBorough
            string pupilList =  "Aline;"        + "Rosy Lane School;"   + "1;"          + "1.2;"            + "N;"          + "Y;"  + "1;" + "F;" + "Y;\n";
            pupilList +=        "Angelika;"     + "Rosy Lane School;"   + "1;"          + "1.1;"            + "N;"          + "N;"  + "1;" + "F;" + "N;\n";
            pupilList +=        "Anitra;"       + "Rosy Lane School;"   + "1;"          + "0.1;"            + "N;"          + "N;"  + "1;" + "F;" + "Y;\n";
            pupilList +=        "Anthony;"      + "Rosy Lane School;"   + "1;"          + "0.2;"            + "N;"          + "N;"  + "2;" + "M;" + "Y;\n";
            pupilList +=        "Antone;"       + "Rosy Lane School;"   + "1;"          + "0.7;"            + "N;"          + "N;"  + "3;" + "M;" + "Y;\n";
            
            pupilList +=        "Anitra;"       + "Plane School;"       + "-1;"         + "0.9;"            + "N;"          + "N;"  + "1;" + "F;" + "Y;\n";
            pupilList +=        "Anthony;"      + "Plane School;"       + "2;"          + "0.7;"            + "N;"          + "N;"  + "2;" + "M;" + "Y;\n";
            pupilList +=        "Antone;"       + "Plane School;"       + "-1;"         + "0.1;"            + "N;"          + "N;"  + "3;" + "M;" + "Y;\n";
            return pupilList;
        }
        public static string GetLongPupilList()
        {
            string pupilList = "Aline;1;F;Y;Rosy Lane School,1,0.1,N,Y;Lolnew School,2,1.1,N,N;Plane School,-1,1.2,N,N;\n";                        
            pupilList += "Angelika;1;F;Y;Rosy Lane School,1,0.1,N,Y;Lolnew School,2,0.9,N,N;Plane School,-1,0.9,N,N;\n";
            pupilList += "Anitra;1;F;Y;Rosy Lane School,1,4,N,Y;Lolnew School,2,2,N,N;Plane School,-1,0.8,N,N;\n";                        
            pupilList += "Anthony;1;M;Y;Rosy Lane School,1,2,N,Y;Lolnew School,2,3,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Antone;1;M;Y;Rosy Lane School,1,0.2,N,Y;Lolnew School,2,0.9,N,N;Plane School,-1,1.2,N,N;\n";                        
            pupilList += "Art;1;M;Y;Rosy Lane School,1,0.8,N,Y;Lolnew School,2,0.9,N,N;Plane School,-1,1.3,N,N;\n";                        
            pupilList += "Arturo;1;M;Y;Rosy Lane School,2,0.3,N,N;Lolnew School,1,0.9,N,Y;Plane School,-1,0.8,N,N;\n";                        
            pupilList += "Augustus;1;M;Y;Rosy Lane School,2,0.3,N,N;Lolnew School,1,0.9,N,Y;Plane School,-1,0.9,N,N;\n";                        
            pupilList += "Bobbi;1;F;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,1,0.5,N,Y;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Brandy;1;F;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,1,0.5,N,Y;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Caridad;1;F;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,1,0.5,N,Y;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Carina;1;`;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,1,0.5,N,Y;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Charles;1;M;N;Rosy Lane School,1,0.1,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Cherry;1;F;Y;Rosy Lane School,1,0.1,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Colby;1;M;Y;Rosy Lane School,1,0.1,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Corazon;1;F;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Demarcus;2;M;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Donita;2;F;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Edmundo;2;M;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Eleonore;2;F;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Ema;2;F;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Erik;2;M;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Evan;2;M;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Fallon;2;F;Y;Rosy Lane School,1,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,-1,0.5,N,N;\n";                        
            pupilList += "Forrest;2;M;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,1,0.5,N,Y;\n";                        
            pupilList += "Ginette;2;F;Y;Rosy Lane School,2,0.5,N,N;Lolnew School,2,0.5,N,N;Plane School,1,0.5,N,Y;\n";                        
            pupilList += "Jamie;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Jimmy;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Julian;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Kendrick;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Kim;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Kurtis;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,1.1,N,N;\n";                        
            pupilList += "Lenny;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,1.2,N,N;\n";                        
            pupilList += "Levi;2;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Loise;2;F;Y;Rosy Lane School,1,1.1,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Loraine;2;F;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Lore;2;F;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,1.5,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Mardell;2;F;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Naida;3;F;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Nidia;3;F;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,0.9,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Norman;3;M;Y;Rosy Lane School,1,0.7,N,N;Lolnew School,2,1.3,N,N;Plane School,-1,0.7,N,N;\n";                        
            pupilList += "Regenia;3;F;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,1,0.7,N,Y;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Robbyn;3;F;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,1,0.7,N,Y;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Roland;3;M;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Rory;3;M;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Rosario;3;M;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Sammie;3;M;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,N;\n";                        
            pupilList += "Sandy;3;M;Y;Rosy Lane School,3,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,N;\n";                        
            pupilList += "Santana;3;F;Y;Rosy Lane School,3,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,N;\n";                        
            pupilList += "Serena;3;F;Y;Rosy Lane School,3,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,N;\n";                        
            pupilList += "Stefan;3;M;Y;Rosy Lane School,3,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,N;\n";                        
            pupilList += "Tamar;3;F;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Tequila;3;F;Y;Rosy Lane School,1,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Theola;3;F;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,Y;\n";                        
            pupilList += "Theron;3;M;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,N,Y;\n";                        
            pupilList += "Tory;3;M;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,1,0.7,Y,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Veronica;3;F;Y;Rosy Lane School,2,0.8,N,N;Lolnew School,2,0.7,N,N;Plane School,1,0.1,Y,Y;\n";                        
            pupilList += "Von;3;M;Y;Rosy Lane School,1,0.8,Y,N;Lolnew School,2,0.7,N,N;Plane School,-1,0.1,N,N;\n";                        
            pupilList += "Yasmin;3;F;Y;Rosy Lane School,1,1,Y,N;Lolnew School,2,0.9,N,N;Plane School,-1,1,N,N;\n";
            pupilList += "Yoko;3;F;Y;Rosy Lane School,1,2,Y,N;Lolnew School,2,1,N,N;Plane School,-1,0.8,N,N;\n";                        
            return pupilList;
        }
    }
}