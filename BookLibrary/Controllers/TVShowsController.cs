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

namespace BookLibrary.Web.Controllers
{
    public class TVShowsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(TVShowsController));
        // GET: TVShows
        public ActionResult Index()
        {
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Recently Added", "/TVShows/Recent"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Show", "/TVShows/Create"));
            }
            int tvShowCount = TVShows.Count();
            double pageCount = Math.Ceiling((double)tvShowCount / (double)perPage);
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
                    pages.Add("<a href='/TVShows?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = tvShowCount;
            ViewBag.LinkList = links;
            return View(db.TVShows.OrderBy(m => m.SortTitle).Skip(skip).Take(perPage).ToList());
        }

        // GET: TVShows/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVShow tvShow = db.TVShows.Include(tv => tv.TVStars).Where(tv => tv.Id == id).FirstOrDefault();
            if (tvShow == null)
            {
                return HttpNotFound();
            }

            TVShowDetailViewModel view = new TVShowDetailViewModel
            {
                Show = tvShow,
                Stars = TVStars.GetStarsForTV(tvShow.TVStars)
            };

            string details = "";
            if(!String.IsNullOrEmpty(tvShow.FirstAired))
            {
                details += "<strong>First Aired: </strong>" + tvShow.FirstAired + "<br />";
            }
            if(!String.IsNullOrEmpty(tvShow.Network))
            {
                details += "<strong>Network: </strong>" + tvShow.Network + "<br />";
            }
            if(!String.IsNullOrEmpty(tvShow.Runtime))
            {
                details += "<strong>Rating: </strong>" + tvShow.Rating + "</br />";
            }
            if(!String.IsNullOrEmpty(tvShow.Genres))
            {
                details += "<strong>Genre: </strong>" + tvShow.Genres + "<br />";
            }
            if(!String.IsNullOrEmpty(tvShow.Status))
            {
                details += "<strong>Airing Status: </strong>" + tvShow.Status + "<br />";
            }
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Show Information", tvShow.Url, "_blank"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add TV Star", "/TVStars/Create?movie=" + tvShow.Id));
                links.Add(new PageLink("Edit", "/TVShows/Edit/" + tvShow.Id));
                links.Add(new PageLink("Delete", "/TVShows/Delete/" + tvShow.Id, args: @"onClick=""return confirm('Are you sure?');"""));
                links.Add(new PageLink("Refresh Cast", "/TVShows/RefreshCast/" + tvShow.Id));
            }
            ViewBag.SubTitle = details;
            ViewBag.LinkList = links;
            return View(view);
        }
        [Authorize]
        // GET: TVShows/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TVShows/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TVShow tvShow)
        {
            log.Info("Create");
            if (ModelState.IsValid)
            {
                Exception ex;
                tvShow.Id = Guid.NewGuid();
                tvShow.CreatedDate = DateTime.Now;
                if (TVShows.UpdateShow(ref tvShow, out ex))
                {
                    db.TVShows.Add(tvShow);
                    db.SaveChanges();
                    if (TVShows.RefreshCast(tvShow.Id, out tvShow, out ex))
                    {
                        return RedirectToAction("Details", new { id = tvShow.Id });
                    }
                }
                ModelState.AddModelError("ERR", ex.Message);
            }

            return View(tvShow);
        }
        [Authorize]
        // GET: TVShows/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVShow tvShow = db.TVShows.Find(id);
            if (tvShow == null)
            {
                return HttpNotFound();
            }
            return View(tvShow);
        }

        // POST: TVShows/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TVShow tvShow)
        {
            Exception ex;
            if (ModelState.IsValid)
            {
                if (TVShows.UpdateShow(ref tvShow, out ex))
                {
                    db.Entry(tvShow).State = EntityState.Modified;
                    db.SaveChanges();
                    if (TVShows.RefreshCast(tvShow.Id, out tvShow, out ex))
                    {
                        return RedirectToAction("Details", new { id = tvShow.Id });
                    }
                }
                ModelState.AddModelError("ERR", ex.Message);
            }
            return View(tvShow);
        }

        // GET: TVShows/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVShow tvShow = db.TVShows.Find(id);
            if (tvShow == null)
            {
                return HttpNotFound();
            }

            db.TVShows.Remove(tvShow);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult RefreshCast(Guid? id)
        {
            Exception ex;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TVShow tvShow;
            TVShows.RefreshCast(id, out tvShow, out ex);
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult Recent()
        {
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            RecentTVShowViewModel recentView = new RecentTVShowViewModel
            {
                Today = new List<TVShow>(),
                Yesterday = new List<TVShow>(),
                ThisWeek = new List<TVShow>(),
                LastWeek = new List<TVShow>(),
                Older = new Dictionary<int, List<TVShow>>(),
                Count = 0
            };
            List<TVShow> tvShowList = db.TVShows.OrderByDescending(o => o.CreatedDate).Skip(skip).Take(perPage).ToList();
            string today = DateTime.Now.ToString("yyyyMMdd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            DateTime todayDate = DateTime.Now;
            while (todayDate.DayOfWeek != DayOfWeek.Sunday)
            {
                todayDate = todayDate.AddDays(-1);
            }
            string thisWeekStart = todayDate.ToString("yyyyMMdd");
            todayDate = todayDate.AddDays(-1);
            string lastWeekEnd = todayDate.ToString("yyyyMMdd");
            while (todayDate.DayOfWeek != DayOfWeek.Sunday)
            {
                todayDate = todayDate.AddDays(-1);
            }
            string lastWeekStart = todayDate.ToString("yyyyMMdd");


            foreach (TVShow tvShow in tvShowList)
            {
                string tvShowDate = tvShow.CreatedDate.ToString("yyyyMMdd");
                if (tvShowDate == today)
                {
                    recentView.Today.Add(tvShow);
                }
                else if (tvShowDate == yesterday)
                {
                    recentView.Yesterday.Add(tvShow);
                }
                else if (String.Compare(tvShowDate, thisWeekStart) >= 0)
                {
                    recentView.ThisWeek.Add(tvShow);
                }
                else if (String.Compare(tvShowDate, lastWeekStart) >= 0)
                {
                    recentView.LastWeek.Add(tvShow);
                }
                else
                {
                    int year = tvShow.CreatedDate.Year;
                    if (!recentView.Older.ContainsKey(year))
                    {
                        recentView.Older.Add(year, new List<TVShow>());
                    }
                    recentView.Older[year].Add(tvShow);
                }
                recentView.Count += 1;
            }
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("TV Shows By Name", "/TVShows"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add TV Show", "/TVShows/Create"));
            }
            int tvShowCount = TVShows.Count();
            double pageCount = Math.Ceiling((double)tvShowCount / (double)perPage);
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
                    pages.Add("<a href='/TVShows/Recent?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = tvShowCount;
            ViewBag.LinkList = links;
            return View(recentView);
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
