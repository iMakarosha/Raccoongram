using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;
using Racoonogram.Services;

namespace Racoonogram.Controllers
{
    public class ImagesController : Controller
    {
        // GET: Images
        public ActionResult Index()
        {
            return View(new ImageService().GetImages().ToList());
        }

        // GET: Images/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = new ImageService().GetImage(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // GET: Images/Create
        public ActionResult Create()
        {
            ViewBag.ApplicationUserId = new UserService().GetUsers();
            return View();
        }

        // POST: Images/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImageId,ApplicationUserId,Category,KeyWords,Price,Description,Url,Date")] Image image)
        {
            if (ModelState.IsValid)
            {
                new ImageService().AddImage(image);
                return RedirectToAction("Index");
            }

            ViewBag.ApplicationUserId = new UserService().GetUsers(image.ApplicationUserId);
            return View(image);
        }

        // GET: Images/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = new ImageService().GetImage(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationUserId = new UserService().GetUsers(image.ApplicationUserId);
            return View(image);
        }

        // POST: Images/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImageId,ApplicationUserId,Category,KeyWords,Price,Description,Url,Date")] Image image)
        {
            if (ModelState.IsValid)
            {
                new ImageService().ModifyImage(image);
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationUserId = new UserService().GetUsers(image.ApplicationUserId);
            return View(image);
        }

        // GET: Images/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = new ImageService().GetImage(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            new ImageService().DeleteImage(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                new ImageService().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
