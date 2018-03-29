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
    public class MovieStarsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(MovieStarsController));
        // GET: MovieStars
        public ActionResult Index()
        {
            //MovieStars.MigrateToPeople();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<MovieStar> movieStars = db.MovieStars.Include(ms => ms.Person).Include(ms =>ms.Movies).OrderBy(ms => ms.Person.Name).Skip(skip).Take(perPage).ToList();
            int movieStarCount = MovieStars.Count();
            double pageCount = Math.Ceiling((double)movieStarCount / (double)perPage);
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
                    pages.Add("<a href='/MovieStars?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }
            if(User.Identity.IsAuthenticated)
            {
                List<PageLink> links = new List<PageLink>();
                links.Add(new PageLink("Add Movie Star", "/MovieStars/Create"));
                ViewBag.LinkList = links;
            }
            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = movieStarCount;
            return View(movieStars);
        }

        // GET: MovieStars/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MovieStar movieStar = db.MovieStars.Find(id);
            if (movieStar == null)
            {
                return HttpNotFound();
            }
            List<PageLink> links = new List<PageLink>();
            PersonViewModel viewModel;
            Exception ex;
            if (!People.GeneratePersonView(id.Value, "MovieStar", out viewModel, out ex))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            People.DrawLinkListForView(viewModel, "Movie Star", ref links);

            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Delete", "/MovieStars/Delete/" + id.Value, args: @"onClick=""return confirm('Are you sure?');"""));
            }

            ViewBag.LinkList = links;
            return View("PeopleDetails", viewModel);
        }
        [Authorize]
        // GET: MovieStars/Create
        public ActionResult Create()
        {
            Guid movieId = Guid.Empty; 
            if(!String.IsNullOrEmpty(Request.QueryString["movie"]))
            {
                if(!Guid.TryParse(Request.QueryString["movie"], out movieId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            ViewBag.MovieId = new SelectList(db.Movies.OrderBy(m => m.Title).ToList(), "Id", "Title", movieId);
            return View();
        }

        // POST: MovieStars/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MovieStar movieStar)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(movieStar));
            if (ModelState.IsValid)
            {
                string name = Request.Form["Name"];
                if (!String.IsNullOrEmpty(name))
                {
                    Exception ex;
                    if (MovieStars.AddMovieStarToMovie(name, new Guid(Request.Form["MovieId"]), out ex, true))
                    {
                        return RedirectToAction("Details", "Movies", new { id = new Guid(Request.Form["MovieId"]) });
                    }
                    ModelState.AddModelError("ERR", ex.Message);
                    log.Error("Error", ex);
                }
                ModelState.AddModelError("ERR", "Name required");
                log.Error("Name required");
            }

            return View(movieStar);
        }
        [Authorize]
        // GET: MovieStars/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MovieStar movieStar = db.MovieStars.Find(id);
            if (movieStar == null)
            {
                return HttpNotFound();
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
