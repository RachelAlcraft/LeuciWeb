using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VeracityFable.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "is a creative software house based in London, UK.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "is a creative software house based in London, UK.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "is run by Rachel Jones.";

            return View();
        }

        public ActionResult Projects()
        {
            ViewBag.Message = "currently has these projects in the pipeline:";

            return View();
        }

        public ActionResult TheProject()
        {
            ViewBag.Message = "not-for-profit training in software engineering:";

            return View();
        }

        public ActionResult TheStudio()
        {
            ViewBag.Message = "currently has these projects in the pipeline:";

            return View();
        }
    }
}
