using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeuciWeb.Controllers
{
    public class DensityController : Controller
    {
        // GET: DensityController
        public ActionResult Index()
        {
            return View();
        }

        // POST: DensityController/Details/
        [HttpPost]
        public ActionResult Details(string id)
        {
            ViewBag.RandomRadius = 17;
            return View();
        }

        // GET: DensityController/Create
        public ActionResult Create()
        {            
            return View();
        }

        // POST: DensityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DensityController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DensityController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DensityController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DensityController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
