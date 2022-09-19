using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VeracityFable.Models;
using VeracityFable.BusinessObjects.School;

namespace VeracityFable.Controllers
{
    public class AlloAlgoController : Controller
    {
        //
        // GET: /AlloAlgo/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AlloAlgoForm()
        {
            SchoolModel sm = new SchoolModel();
            ViewBag.PupilList = Pupil.GetLongPupilList();
            ViewBag.SchoolList = School.GetLongSchoolList();
            ViewBag.RandomRadius = 1;
            ViewBag.MaxChoices = 3;
            ViewBag.SiblingMax = 3;
            return View(sm);
        }

        [HttpPost]
        public ActionResult CalcAlloAlgoForm(FormCollection form)
        {
            string randomRadius = form["randomRadius"];
            string siblingMax = form["siblingMax"];
            string maxChoices = form["maxChoices"];
            string schoolList = form["schoolList"];
            string pupilList = form["pupilList"];

            ViewBag.RandomRadius = randomRadius;
            ViewBag.SiblingMax = siblingMax;
            ViewBag.SchoolList = schoolList;
            ViewBag.PupilList = pupilList;

            //Calculate randomly
            AllocRandomRadius arr = new AllocRandomRadius();
            arr.SetMaxSibling(Convert.ToDouble(siblingMax));
            arr.SetRandomRadius(Convert.ToDouble(randomRadius));
            arr.SetMaxChoices(Convert.ToInt32(maxChoices));
            arr.SetSchools(schoolList);
            arr.SetPupils(pupilList);
            arr.Allocate();

            //Calculate by distance
            AllocDistance diss = new AllocDistance();
            diss.SetMaxSibling(Convert.ToDouble(siblingMax));
            diss.SetRandomRadius(Convert.ToDouble(randomRadius));
            diss.SetMaxChoices(Convert.ToInt32(maxChoices));
            diss.SetSchools(schoolList);
            diss.SetPupils(pupilList);
            diss.Allocate();

            string alloAlgoResult = arr.GetSchoolAllocation();
            string alloAlgoResultDistance = diss.GetSchoolAllocation();

            ViewBag.AlloAlgoResult = alloAlgoResult;
            ViewBag.AlloAlgoResultDistance = alloAlgoResultDistance;

            ViewBag.ErrorRandom = arr.ErrorReason;
            ViewBag.ErrorDistance = diss.ErrorReason;

            ViewBag.Counter = arr.UpdateCalculationCounter();

            return View();
            
        }

    }
}
