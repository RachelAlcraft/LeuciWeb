using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VeracityFable.Controllers
{
    public class LeucippusController : Controller
    {
        //
        // GET: /Leucippus/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Leucippus/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Leucippus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Leucippus/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Leucippus/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Leucippus/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Leucippus/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Leucippus/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
