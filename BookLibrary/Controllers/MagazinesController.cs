using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using BookLibrary.Services;
using BookLibrary.Functions;
using BookLibrary.Models.ViewModels;
using Newtonsoft.Json;
using BookLibrary.Functions.Core;
using System.Threading;

namespace BookLibrary.Web.Controllers
{
    public class MagazinesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private PDFLibraryService pdfService = new PDFLibraryService("grbrewer@gmail.com", "!Pass248word");
        private Logger log = new Logger(typeof(MagazinesController));
        // GET: Magazines
        public ActionResult Index()
        {
            //Magazines.ApplySortTitle();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<MagazineListItemViewModel> magazinesView = new List<MagazineListItemViewModel>();
            List<Magazine> magazines = db.Magazines.Include(m => m.MagazineIssues).OrderBy(m => m.SortTitle).Skip(skip).Take(perPage).ToList();
            foreach(Magazine magazine in magazines)
            {
                magazinesView.Add(Magazines.MagazineToView(magazine));
            }
            List<PageLink> links = new List<PageLink>();

            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Magazine", "/Magazines/Create"));
                links.Add(new PageLink("Sync Issues", "/Magazines/SyncAllIssues"));
            }
            links.Add(new PageLink("Issues By Date", "/Magazines/Issues"));
            int magazineCount = Magazines.Count();
            double pageCount = Math.Ceiling((double)magazineCount / (double)perPage);
            int start = page - 4;
            if (start < 1)
            {
                start = 1;
            }
            int end = page + 4;
            if (end > pageCount)
            {
                end = (int)pageCount;
            }

            List<string> pages = new List<string>();
            for (int idx = start; idx <= end; idx++)
            {
                if (idx != page)
                {
                    pages.Add("<a href='/Magazines?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }
            ViewBag.LinkList = links;
            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = magazineCount;
            return View(magazinesView.ToList());
        }

        // GET: Magazines/Details/5
        public ActionResult Details(Guid? id)
        {
            

            if (id == null || !pdfService.LoggedIn())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Magazine magazine = db.Magazines.Find(id);
            if (magazine == null)
            {
                return HttpNotFound();
            }
            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Edit", "/Magazines/Edit/" + magazine.Id));
                links.Add(new PageLink("Delete", "/Magazines/Delete/" + magazine.Id, args: @"onClick=""return confirm('Are you sure?');"""));
                links.Add(new PageLink("Sync Issues", "/Magazines/SyncIssues/" + magazine.Id));
            }
            ViewBag.LinkList = links;
            ViewBag.PDFLibraryRoot = pdfService.ExternalUrl;
            return View(Magazines.MagazineToView(magazine));
        }

        [Authorize]
        // GET: Magazines/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Magazines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Magazine magazine)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(magazine));
            Exception ex;
            if (ModelState.IsValid)
            {
                magazine.Id = Guid.NewGuid();
                magazine.CreatedDate = magazine.ModifiedDate = DateTime.Now;
                if (!Magazines.GetCategoryFolder(ref magazine, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(magazine);
                }
                magazine.SortTitle = Core.ApplySortTitle(magazine.Title);
                db.Magazines.Add(magazine);
                db.SaveChanges();
                if(!Magazines.FindCreateCategory(magazine, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(magazine);
                }

                if(!MagazineIssues.SyncIssues(magazine, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(magazine);
                }
                return RedirectToAction("Details", new { id = magazine.Id });
            }

            return View(magazine);
        }
        [Authorize]
        // GET: Magazines/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Magazine magazine = db.Magazines.Find(id);
            
            if (magazine == null)
            {
                return HttpNotFound();
            }
            ViewBag.PageImage = Magazines.GetMagazineThumb(id.Value);
            return View(magazine);
        }

        // POST: Magazines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Magazine magazine)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(magazine));
            if (ModelState.IsValid)
            {
                Exception ex;
                magazine.ModifiedDate = DateTime.Now;
                magazine.SortTitle = Core.ApplySortTitle(magazine.Title);
                db.Entry(magazine).State = EntityState.Modified;
                db.SaveChanges();

                Categories.Cleanup("Magazine", magazine.Id);
                if (!Magazines.FindCreateCategory(magazine, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(magazine);
                }

                if (!MagazineIssues.SyncIssues(magazine, out ex))
                {
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                    return View(magazine);
                }
                return RedirectToAction("Details", new { id = magazine.Id });
            }
            return View(magazine);
        }
        [Authorize]
        // GET: Magazines/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Magazine magazine = db.Magazines.Find(id);
            if (magazine == null)
            {
                return HttpNotFound();
            }
            Categories.Cleanup("Magazine", magazine.Id);
            db.Magazines.Remove(magazine);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Issues()
        {
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            IssueByDateViewModel issues = MagazineIssues.GetMagazineIssuesByDate(skip, perPage);
            
            ViewBag.PDFLibraryRoot = pdfService.ExternalUrl;
            List<PageLink> links = new List<PageLink>();

            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Magazine", "/Magazines/Create"));
                ViewBag.LinkList = links;
            }
            links.Add(new PageLink("All Magazines", "/Magazines"));
            int issueCount = MagazineIssues.Count();
            double pageCount = Math.Ceiling((double)issueCount / (double)perPage);
            int start = page - 4;
            if (start < 1)
            {
                start = 1;
            }
            int end = page + 4;
            if (end > pageCount)
            {
                end = (int)pageCount;
            }

            List<string> pages = new List<string>();
            for (int idx = start; idx <= end; idx++)
            {
                if (idx != page)
                {
                    pages.Add("<a href='/Magazines?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = issueCount;
            return View(issues);
        }

        public ActionResult SyncIssues(Guid? id)
        {
            if (id == null || !pdfService.LoggedIn())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Magazine magazine = db.Magazines.Include(m => m.MagazineIssues).Where(m => m.Id == id).FirstOrDefault();
            if (magazine == null)
            {
                return HttpNotFound();
            }
            Exception ex = null;
            MagazineIssues.SyncIssues(magazine, out ex);

            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult SyncAllIssues()
        {
            if(!pdfService.LoggedIn())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Magazine> magazines = db.Magazines.Include(m => m.MagazineIssues).ToList();
            Exception ex = null;
            foreach(Magazine magazine in magazines)
            {
                try
                {
                    MagazineIssues.SyncIssues(magazine, out ex);
                }
                catch
                {
                    Thread.Sleep(15);
                }
            }
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
