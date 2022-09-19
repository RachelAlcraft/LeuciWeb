using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VeracityFable.Controllers
{
    public class GenieController : Controller
    {
        //
        // GET: /Genie/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GenieDisplay(FormCollection form)
        {
            string genieList = form["genieList"];
            

            ViewBag.GenieList = genieList;
            

            

            
            

            

            return View();

        }

    }
}
