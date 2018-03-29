using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using BookLibrary.Functions;
using BookLibrary.Models.ViewModels;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class TVStarsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(TVStarsController));
        // GET: TVStars
        public ActionResult Index()
        {
            //TVStars.MigrateToPeople();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<TVStar> tvStars = db.TVStars.Include(ms => ms.Person).Include(ms => ms.TVShows).OrderBy(ms => ms.Person.Name).Skip(skip).Take(perPage).ToList();
            int tvStarCount = TVStars.Count();
            double pageCount = Math.Ceiling((double)tvStarCount / (double)perPage);
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
                    pages.Add("<a href='/TVStars?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }
            if (User.Identity.IsAuthenticated)
            {
                List<PageLink> links = new List<PageLink>();
                links.Add(new PageLink("Add TV Star", "/TVStars/Create"));
                ViewBag.LinkList = links;
            }
            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = tvStarCount;
            return View(tvStars);
        }

        // GET: TVStars/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVStar tvStar = db.TVStars.Find(id);
            if (tvStar == null)
            {
                return HttpNotFound();
            }
            List<PageLink> links = new List<PageLink>();
            PersonViewModel viewModel;
            Exception ex;
            if (!People.GeneratePersonView(id.Value, "TVStar", out viewModel, out ex))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            People.DrawLinkListForView(viewModel, "TV Star", ref links);

            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Delete", "/TVStars/Delete/" + id.Value, args: @"onClick=""return confirm('Are you sure?');"""));
            }

            ViewBag.LinkList = links;
            return View("PeopleDetails", viewModel);
        }

        // GET: TVStars/Create
        public ActionResult Create()
        {
            Guid tvShowId = Guid.Empty;
            if (!String.IsNullOrEmpty(Request.QueryString["tvshow"]))
            {
                if (!Guid.TryParse(Request.QueryString["tvshow"], out tvShowId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            ViewBag.TVShowId = new SelectList(db.TVShows.OrderBy(tv => tv.SortTitle), "Id", "Title", tvShowId);
            return View();
        }

        // POST: TVStars/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TVStar tvStar)
        {

            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(tvStar));
            if (ModelState.IsValid)
            {
                string name = Request.Form["Name"];
                if (!String.IsNullOrEmpty(name))
                {
                    Exception ex;
                    if (TVStars.AddTVStarToTVShow(name, new Guid(Request.Form["TVShowId"]), out ex, true))
                    {
                        return RedirectToAction("Details", "TVShows", new { id = new Guid(Request.Form["TVShowId"]) });
                    }
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                }
                ModelState.AddModelError("ERR", "Name required");
                log.Error("Name required");
            }

            ViewBag.TVShowId = new SelectList(db.TVShows.OrderBy(tv => tv.SortTitle), "Id", "Title");
            return View(tvStar);
        }

        
        // GET: TVStars/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVStar tVStar = db.TVStars.Find(id);
            if (tVStar == null)
            {
                return HttpNotFound();
            }
            return View(tVStar);
        }

        // POST: TVStars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            TVStar tVStar = db.TVStars.Find(id);
            db.TVStars.Remove(tVStar);
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
