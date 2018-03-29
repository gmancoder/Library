using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookLibrary.Models;
using BookLibrary.Functions.Core;
using System.IO;
using BookLibrary.Functions;
using BookLibrary.Models.ViewModels;
using BookLibrary.Models.ServiceModels.Amazon;
using Humanizer;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class MoviesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(MoviesController));
        // GET: Movies
        public ActionResult Index()
        {
            //Movies.ApplySortTitle();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Recently Added", "/Movies/Recent"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Movie", "/Movies/Create"));
            }
            int movieCount = Movies.Count();
            double pageCount = Math.Ceiling((double)movieCount / (double)perPage);
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
                    pages.Add("<a href='/Movies?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = movieCount;
            ViewBag.LinkList = links;
            return View(db.Movies.OrderBy(m => m.SortTitle).Skip(skip).Take(perPage).ToList());
        }

        // GET: Movies/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Include(m => m.Stars).Where(m => m.Id == id).FirstOrDefault();
            if (movie == null)
            {
                return HttpNotFound();
            }
            MovieDetailViewModel movieDetailView = new MovieDetailViewModel
            {
                Object = movie,
                Offers = new Offers(),
                Reviews = new List<EditorialReview>(),
                SimilarProducts = new List<Movie>(),
                CategoryStreams = new List<List<Category>>(),
                Stars = MovieStars.GetStarsForMovie(movie.Stars)
             
            };
            ItemLookupResponse amzResponse;
            Exception ex;

            if (Core.ParseAmazonXml(movie.AmazonResponse, out amzResponse, out ex))
            {
                movieDetailView.Offers = amzResponse.Items.Item.Offers;
                if (amzResponse.Items.Item.EditorialReviews != null)
                {
                    movieDetailView.Reviews = amzResponse.Items.Item.EditorialReviews.ToList();
                }
                if (amzResponse.Items.Item.SimilarProducts != null)
                {
                    foreach (SimilarProduct product in amzResponse.Items.Item.SimilarProducts)
                    {
                        Movie similarMovie = db.Movies.Where(a => a.ASIN == product.ASIN).FirstOrDefault();
                        if (similarMovie != null)
                        {
                            movieDetailView.SimilarProducts.Add(similarMovie);
                        }
                    }
                }
            }
            movieDetailView.CategoryStreams = Categories.DrawBreadcrumbsForObject(movie.Id);

            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Movie Information", movie.Url, "_blank"));
            if (movieDetailView.SimilarProducts.Count() > 0)
            {
                links.Add(new PageLink("Similar Items", "#similar"));
            }
            if (movieDetailView.Reviews.Count() > 0)
            {
                links.Add(new PageLink("Reviews", "#reviews"));
            }
            if (movieDetailView.Offers.TotalOffers > 0)
            {
                links.Add(new PageLink("Shopping Offers", "#shopping"));
            }
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Movie Star", "/MovieStars/Create?movie=" + movie.Id));
                links.Add(new PageLink("Edit", "/Movies/Edit/" + movie.Id));
                links.Add(new PageLink("Delete", "/Movies/Delete/" + movie.Id, args: @"onClick=""return confirm('Are you sure?');"""));
                links.Add(new PageLink("Refresh Cast", "/Movies/RefreshCast/" + movie.Id));
            }
            string details = "<strong>Running Time: </strong>" + movie.RunningTime + " minutes";
            if(!String.IsNullOrEmpty(movie.Director))
            {
                details += "<br /><strong>Director: </strong>" + movie.Director;
            }
            if(!String.IsNullOrEmpty(movie.Manufacturer))
            {
                details += "<br /><strong>Produced By: </strong>" + movie.Manufacturer;
            }
            if(!String.IsNullOrEmpty(movie.Genre))
            {
                string[] genres = movie.Genre.Split(',');
                details += "<br /><strong>" + String.Format("Genre".ToQuantity(genres.Length, ShowQuantityAs.None)) + ": </strong>" + movie.Genre;
            }
            if(!String.IsNullOrEmpty(movie.AudienceRating))
            {
                details += "<br /><strong>Rated: </strong>" + movie.AudienceRating;
            }
            ViewBag.SubTitle = details;
            ViewBag.LinkList = links;
            return View(movieDetailView);
        }
        [Authorize]
        // GET: Movies/Create
        public ActionResult Create()
        {
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Movie movie)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(movie));
            List<Guid> selectedCategories = new List<Guid>();
            try
            {
                bool amazon = false;
                Exception e;
                
                movie.Id = Guid.NewGuid();
                movie.CreatedDate = movie.ModifiedDate = DateTime.Now;
                if (movie.EntryType == "Amazon")
                {
                    if (Movies.AmazonMovieExists(movie.ASIN))
                    {
                        ModelState.AddModelError("ERR", "Movie with ASIN '" + movie.ASIN + "' already exists");
                        log.Error("Movie with ASIN '" + movie.ASIN + "' already exists");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    amazon = true;
                    if (!Movies.AmazonMovie(ref movie, out e))
                    {
                        ModelState.AddModelError("ERR", e.Message + "<br />" + e.StackTrace);
                        log.Error("Error", e);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    return RedirectToAction("Details", new { id = movie.Id });
                }
                else
                {
                    if (Movies.ManualMovieExists(movie.Title))
                    {
                        ModelState.AddModelError("ERR", "Movie '" + movie.Title + "' already exists");
                        log.Error("Movie '" + movie.Title + "' already exists");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength == 0)
                    {
                        ModelState.AddModelError("ERR", "Image Upload required for manual entry");
                        log.Error("Image Upload required for manual entry");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/movies"), fileName);
                        display_image.SaveAs(path);
                        movie.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Movie requires an image");
                        log.Error("Movie requires an image");
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    movie.SortTitle = Core.ApplySortTitle(movie.Title);
                    db.Movies.Add(movie);
                    db.SaveChanges();

                    if (!Movies.PopulateMovieStars(movie, out e))
                    {
                        ModelState.AddModelError("ERR", e.Message);
                        log.Error("Error", e);
                        ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
                        ViewBag.Error = true;
                        return View(movie);
                    }

                    if (!String.IsNullOrWhiteSpace(Request.Form["categoryTree"]))
                    {
                        string categoryTree = Request.Form["categoryTree"];
                        string[] categoryIds = categoryTree.Split(',');
                        foreach (string categoryId in categoryIds)
                        {
                            Guid CategoryId;
                            if (Guid.TryParse(categoryId, out CategoryId))
                            {
                                ObjectToCategory movieCategory = new ObjectToCategory
                                {
                                    ObjectId = movie.Id,
                                    ObjectType = "Movie",
                                    CategoryId = CategoryId,
                                    //Id = Guid.NewGuid()
                                };
                                selectedCategories.Add(CategoryId);
                                db.ObjectToCategories.Add(movieCategory);
                                db.SaveChanges();
                            }
                        }
                    }

                    return RedirectToAction("Details", new { id = movie.Id });
                }
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("ERR", exp.Message);
                log.Error("Error", exp);
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            ViewBag.Error = true;
            return View(movie);
        }
        [Authorize]
        // GET: Movies/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Movie movie)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(movie));
            List<Guid> selectedCategories = new List<Guid>();
            try
            {
                bool amazon = false;
                Exception e;

                if (movie.EntryType == "Amazon")
                {
                    amazon = true;
                    if (!Movies.AmazonMovie(ref movie, out e, true))
                    {
                        ModelState.AddModelError("ERR", e.Message + "<br />" + e.StackTrace);
                        log.Error("Error", e);
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    
                }
                else
                {
                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength == 0)
                    {
                        ModelState.AddModelError("ERR", "Image Upload required for manual entry");
                        log.Error("Image Upload required for manual entry");
                        ViewBag.Error = true;
                        return View(movie);
                    }
                    else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/movies"), fileName);
                        display_image.SaveAs(path);
                        movie.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Movie requires an image");
                        log.Error("Movie requires an image");
                        ViewBag.Error = true;
                        return View(movie);
                    }

                    db.Database.ExecuteSqlCommand("delete from ObjectToCategories where ObjectId = '" + movie.Id + "' and ObjectType = 'Movie'");
                    if (!String.IsNullOrWhiteSpace(Request.Form["categoryTree"]))
                    {
                        string categoryTree = Request.Form["categoryTree"];
                        string[] categoryIds = categoryTree.Split(',');
                        foreach (string categoryId in categoryIds)
                        {
                            Guid CategoryId;
                            if (Guid.TryParse(categoryId, out CategoryId))
                            {
                                ObjectToCategory movieCategory = new ObjectToCategory
                                {
                                    ObjectId = movie.Id,
                                    ObjectType = "Movie",
                                    CategoryId = CategoryId,
                                    //Id = Guid.NewGuid()
                                };
                                selectedCategories.Add(CategoryId);
                                db.ObjectToCategories.Add(movieCategory);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                movie.SortTitle = Core.ApplySortTitle(movie.Title);
                movie.ModifiedDate = DateTime.Now;
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                db.Database.ExecuteSqlCommand("delete from MovieToMovieStars where MovieId = '" + movie.Id + "' and ManuallyAdded = 0");
                if (!Movies.PopulateMovieStars(movie, out e))
                {
                    ModelState.AddModelError("ERR", e.Message);
                    log.Error("Error", e);
                    ViewBag.Error = true;
                    return View(movie);
                }
                return RedirectToAction("Details", new { id = movie.Id });
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("ERR", exp.Message);
                log.Error("Error", exp);
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            ViewBag.Error = true;
            return View(movie);
        }
        [Authorize]
        // GET: Movies/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movie = db.Movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            Categories.Cleanup("Movie", movie.Id);
            db.Movies.Remove(movie);
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
            Movie movie;
            Movies.RefreshCast(id, out movie, out ex);
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
            RecentMovieViewModel recentView = new RecentMovieViewModel
            {
                Today = new List<Movie>(),
                Yesterday = new List<Movie>(),
                ThisWeek = new List<Movie>(),
                LastWeek = new List<Movie>(),
                Older = new Dictionary<int, List<Movie>>(),
                Count = 0
            };
            List<Movie> movieList = db.Movies.OrderByDescending(o => o.CreatedDate).Skip(skip).Take(perPage).ToList();
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


            foreach (Movie movie in movieList)
            {
                string movieDate = movie.CreatedDate.ToString("yyyyMMdd");
                if (movieDate == today)
                {
                    recentView.Today.Add(movie);
                }
                else if (movieDate == yesterday)
                {
                    recentView.Yesterday.Add(movie);
                }
                else if (String.Compare(movieDate, thisWeekStart) >= 0)
                {
                    recentView.ThisWeek.Add(movie);
                }
                else if (String.Compare(movieDate, lastWeekStart) >= 0)
                {
                    recentView.LastWeek.Add(movie);
                }
                else
                {
                    int year = movie.CreatedDate.Year;
                    if (!recentView.Older.ContainsKey(year))
                    {
                        recentView.Older.Add(year, new List<Movie>());
                    }
                    recentView.Older[year].Add(movie);
                }
                recentView.Count += 1;
            }
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Movies By Name", "/Movies"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Movie", "/Movies/Create"));
            }
            int movieCount = Movies.Count();
            double pageCount = Math.Ceiling((double)movieCount / (double)perPage);
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
                    pages.Add("<a href='/Movies/Recent?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = movieCount;
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
