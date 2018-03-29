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
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(CategoriesController));
        // GET: Categories
        public ActionResult Index()
        {
            Category category = db.Categories.Where(c => c.CategoryId == null).FirstOrDefault();
            if(category != null)
            {
                return RedirectToAction("Details", new { id = category.Id });
            }
            return HttpNotFound();
        }

        // GET: Categories/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            CategoryIndexViewModel categoryIndexView = new CategoryIndexViewModel
            {
                CategoryHtml = Categories.GetCategoryHtml(new List<Guid> { category.Id }),
                Category = category,
                AllCategories = false
            };
            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Root Category", "/Categories/Create"));    
                links.Add(new PageLink("Add Child Category", "/Categories/Create?parent=" + category.Id));
                links.Add(new PageLink("Edit", "/Categories/Edit/" + category.Id));
                if (db.Categories.Where(c => c.CategoryId == category.Id).Count() == 0 && db.ObjectToCategories.Where(ac => ac.CategoryId == category.Id).Count() == 0)
                {
                    links.Add(new PageLink("Delete", "/Categories/Delete/" + category.Id, args: @"onClick=""return confirm('Are you sure?');"""));
                }
            }
            ViewBag.LinkList = links;
            return View("Index", categoryIndexView);
        }
        [Authorize]
        // GET: Categories/Create
        public ActionResult Create()
        {
            Guid parentId = Guid.Empty;
            if(!String.IsNullOrWhiteSpace(Request.QueryString["parent"]) && !Guid.TryParse(Request.QueryString["parent"], out parentId))
            {
                return HttpNotFound();
            }
           
            ViewBag.CategoryId = new SelectList(Categories.DrawCategorySelectList(parentId), "Value", "Text", parentId);
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(category));
            if (ModelState.IsValid)
            {
                category.Id = Guid.NewGuid();
                category.CreatedDate = DateTime.Now;
                category.ModifiedDate = DateTime.Now;
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Guid parentId = Guid.Empty;
            if (!String.IsNullOrWhiteSpace(Request.QueryString["parent"]) && !Guid.TryParse(Request.QueryString["parent"], out parentId))
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(Categories.DrawCategorySelectList(parentId), "Value", "Text", parentId);
            return View(category);
        }
        [Authorize]
        // GET: Categories/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(Categories.DrawCategorySelectList(category.CategoryId), "Value", "Text", category.CategoryId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(category));
            if (ModelState.IsValid)
            {
                category.ModifiedDate = DateTime.Now;
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(Categories.DrawCategorySelectList(category.CategoryId), "Value", "Text", category.CategoryId);
            return View(category);
        }
        [Authorize]
        // GET: Categories/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            if (db.Categories.Where(c => c.CategoryId == category.Id).Count() == 0 && db.ObjectToCategories.Where(ac => ac.CategoryId == category.Id).Count() == 0)
            {
                db.Categories.Remove(category);
                db.SaveChanges();
                return RedirectToAction("Details", new { Id = category.CategoryId });
            }
            return RedirectToAction("Details", new { Id = id });
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
