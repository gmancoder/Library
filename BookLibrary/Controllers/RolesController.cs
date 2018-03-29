using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Functions;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class RolesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(RolesController));
        // GET: Roles
        [Authorize]
        public ActionResult Index()
        {
            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Role", "/Roles/Create"));
                links.Add(new PageLink("Users", "/Users"));
            }
            ViewBag.LinkList = links;
            return View(db.IdentityRoles.ToList());
        }
        [Authorize]
        // GET: Roles/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationRole applicationRole = db.IdentityRoles.Find(id);
            if (applicationRole == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Edit", new { id = id });
        }

        [Authorize]
        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationRole applicationRole)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(applicationRole));
            if (ModelState.IsValid)
            {
                db.IdentityRoles.Add(applicationRole);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(applicationRole);
        }

        [Authorize]
        // GET: Roles/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationRole applicationRole = db.IdentityRoles.Find(id);
            if (applicationRole == null)
            {
                return HttpNotFound();
            }
            return View(applicationRole);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationRole applicationRole)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(applicationRole));
            if (ModelState.IsValid)
            {
                db.Entry(applicationRole).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(applicationRole);
        }

        // GET: Roles/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationRole applicationRole = db.IdentityRoles.Find(id);
            if (applicationRole == null)
            {
                return HttpNotFound();
            }
            db.IdentityRoles.Remove(applicationRole);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

       

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
