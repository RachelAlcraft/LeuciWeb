using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VeracityFable.BusinessObjects.Geneology
{
    public class GenieManager
    {
        private List<Person> _people = new List<Person>();
        private List<Relationship> _relations = new List<Relationship>();
        public void Add(string personLine)
        {//in the format Name;DoB;Relationship
            string[] person = personLine.Split(';');


        }

        public void SetReferencePerson(string refPerson)
        {//set the person who is to be the reference for the canvas

        }

        public void CalculateAllOffsets()
        {/*Go through all people from the RefPerson calculating the total offsets to find the width of the canvas in relative terms
          * In so doing mark each person with a relative position
          * From the width of the canvas find the we can place each person
            */

        }

        public void RenderPeople()
        {//go through all the people and draw boxes for them
            //and save them somewhere

            //1 go through all the relations of the 0 person, remove people marked from the list
            //2 pick the first already marked person, go through all of them
            //3 pick the next marked person
            //once we have been through all the marked people if there is anyone left they are unrelated and should be report as such

        }

        public void ClearOffsetCalculation()
        {

        }

        //when we get each relationship we mark the lines positions and hold them elsewhere, when we come to render we simply have a list of lines and boxes
        public List<Person> GetRelations(Person someone)
        {
            return null;
        }

        public void RenderRelation (Person one, Person two)
        {//get the line we will draw and save it somewhrre

        }
    }
}