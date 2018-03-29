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
using BookLibrary.Models.ServiceModels.Amazon;
using System.Xml.Serialization;
using System.IO;
using BookLibrary.Functions.Core;
using System.Text.RegularExpressions;
using BookLibrary.Functions;
using BookLibrary.Models.ViewModels;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class AlbumsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(AlbumsController));
        // GET: Albums
        public ActionResult Index()
        {
            //Albums.ApplySortTitle();
            int page = 1;
            if(!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            var albums = db.Albums.Include(a => a.Artist).OrderBy(a => a.SortTitle).Skip(skip).Take(perPage);
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Recently Added", "/Albums/Recent"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Album", "/Albums/Create"));
            }

            int albumCount = Albums.Count();
            double pageCount = Math.Ceiling((double)albumCount / (double)perPage);
            int start = page - 4;
            if(start < 1)
            {
                start = 1;
            }
            int end = page + 4;
            if(end > pageCount)
            {
                end = (int)pageCount;
            }

            List<string> pages = new List<string>();
            for(int idx = start; idx <= end; idx ++)
            {
                if (idx != page)
                {
                    pages.Add("<a href='/Albums?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = albumCount;
            ViewBag.LinkList = links;
            return View(albums.ToList());
        }

        // GET: Albums/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Include(a => a.Artist).Include(a => a.TrackList).Where(a=>a.Id == id).FirstOrDefault();
            if (album == null)
            {
                return HttpNotFound();
            }
            if(album.NumberOfDiscs == 0)
            {
                album.NumberOfDiscs = Albums.DetermineDiscCountFromTracks(album.TrackList);
                album.ModifiedDate = DateTime.Now;
                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
            }
            AlbumDetailViewModel albumDetailView = new AlbumDetailViewModel
            {
                Object = album,
                Offers = new Offers(),
                Reviews = new List<EditorialReview>(),
                SimilarProducts = new List<Album>(),
                NumberOfDiscs = 1,
                CategoryStreams = new List<List<Category>>()
            };
            ItemLookupResponse amzResponse;
            Exception ex;

            if (Core.ParseAmazonXml(album.AmazonResponse, out amzResponse, out ex))
            {
                albumDetailView.Offers = amzResponse.Items.Item.Offers;
                if (amzResponse.Items.Item.EditorialReviews != null)
                {
                    albumDetailView.Reviews = amzResponse.Items.Item.EditorialReviews.ToList();
                }
                if (amzResponse.Items.Item.SimilarProducts != null)
                {
                    foreach (SimilarProduct product in amzResponse.Items.Item.SimilarProducts)
                    {
                        Album similarAlbum = db.Albums.Where(a => a.ASIN == product.ASIN).FirstOrDefault();
                        if (similarAlbum != null)
                        {
                            albumDetailView.SimilarProducts.Add(similarAlbum);
                        }
                    }
                }
                if (amzResponse.Items.Item.Discs != null && amzResponse.Items.Item.Discs.Count() > 0)
                {
                    albumDetailView.NumberOfDiscs = amzResponse.Items.Item.Discs.Count();
                }
                else
                {
                    albumDetailView.NumberOfDiscs = 1;
                }
            }
            if (album.Artist.PersonId.HasValue)
            {
                Person person = People.PersonById(album.Artist.PersonId.Value);
                if (person.CelebrityId.HasValue)
                {
                    Celebrity celebrity;
                    if (!Artists.GetCelebrity(person.CelebrityId.Value, out celebrity, out ex))
                    {
                        return RedirectToAction("Index");
                    }
                    albumDetailView.ArtistDetail = celebrity;
                    string url_add = "";
                    if (Request.UserHostAddress.StartsWith("192.168.1."))
                    {
                        url_add = ":8081";
                    }
                    //links.Add(new PageLink("Artist Details", "http://celebritycentral.gmancoder.com" + url_add + "/Celebrities/Details/" + celebrity.Id, "_blank"));
                }
            }

            albumDetailView.CategoryStreams = Categories.DrawBreadcrumbsForObject(album.Id);
            string artistName = "";
            if (albumDetailView.Object.Artist.PersonId.HasValue)
            {
                artistName = albumDetailView.Object.Artist.Person.Name;
            }
            else
            {
                artistName = albumDetailView.Object.Artist.Name;
            }
            string details = "By <a href=\"/Artists/Details/" + albumDetailView.Object.Artist.Id + "\">" + artistName + "</a>; " + albumDetailView.Object.TrackList.Count() + " Tracks on " + albumDetailView.NumberOfDiscs + " Disc(s)";

            if (!String.IsNullOrEmpty(album.ReleaseDate))
            {
                details += "<br /><strong>Released: </strong>" + album.ReleaseDate;
            }
            ViewBag.SubTitle = details;
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Album Information", album.Url, "_blank"));
            if(albumDetailView.ArtistDetail != null && !String.IsNullOrWhiteSpace(albumDetailView.ArtistDetail.Details))
            {
                links.Add(new PageLink("About the Artist", "#artist"));
            }
            if(albumDetailView.SimilarProducts.Count() > 0)
            {
                links.Add(new PageLink("Similar Items", "#similar"));
            }
            if(albumDetailView.Reviews.Count() > 0)
            {
                links.Add(new PageLink("Reviews", "#reviews"));
            }
            if(albumDetailView.Offers.TotalOffers > 0)
            {
                links.Add(new PageLink("Shopping Offers", "#shopping"));
            }
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Track", "/Tracks/Create?album=" + album.Id));
                links.Add(new PageLink("Edit", "/Albums/Edit/" + album.Id));
                links.Add(new PageLink("Delete", "/Albums/Delete/" + album.Id, args: @"onClick=""return confirm('Are you sure?');"""));
            }
            ViewBag.LinkList = links;
            return View(albumDetailView);
        }

        [Authorize]
        // GET: Albums/Create
        public ActionResult Create()
        {
            Guid artistId = Guid.Empty;
            if(Request.QueryString["artist"] != null)
            {
                Guid.TryParse(Request.QueryString["artist"], out artistId);
            }
            ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", artistId);
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(null);
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Album album)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(album));
            List<Guid> selectedCategories = new List<Guid>();
            try
            {
                bool amazon = false;
                bool noDiscs = false;
                Exception e;
                album.Id = Guid.NewGuid();
                album.CreatedDate = album.ModifiedDate = DateTime.Now;
                if(album.EntryType == "Amazon")
                {
                    if(Albums.AmazonAlbumExists(album.ASIN))
                    {
                        ModelState.AddModelError("ERR", "Album with ASIN '" + album.ASIN + "' already exists");
                        log.Error("Album with ASIN '" + album.ASIN + "' already exists");
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }
                    amazon = true;
                    if(!Albums.AmazonAlbum(ref album, out noDiscs, out e))
                    {
                        ModelState.AddModelError("ERR", e.Message + "<br />" + e.StackTrace);
                        log.Error("Error", e);
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }

                    if(noDiscs)
                    {
                        return Redirect("/Tracks/Create?album=" + album.Id);
                    }
                    else
                    {
                        return RedirectToAction("Details", new { id = album.Id });
                    }
                }
                else
                {
                    if (Albums.ManualAlbumExists(album.Title, album.ArtistId))
                    {
                        ModelState.AddModelError("ERR", "Album '" + album.Title + "' already exists for the selected Artist");
                        log.Error("Album '" + album.Title + "' already exists for the selected Artist");
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }
                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength == 0)
                    {
                        ModelState.AddModelError("ERR", "Image Upload required for manual entry");
                        log.Error("Image Upload required for manual entry");
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }
                    else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/albums"), fileName);
                        display_image.SaveAs(path);
                        album.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Album requires an image");
                        log.Error("Album requires an image");
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }
                    album.SortTitle = Core.ApplySortTitle(album.Title);
                    db.Albums.Add(album);
                    db.SaveChanges();

                    if(!String.IsNullOrWhiteSpace(Request.Form["categoryTree"]))
                    {
                        string categoryTree = Request.Form["categoryTree"];
                        string[] categoryIds = categoryTree.Split(',');
                        foreach(string categoryId in categoryIds)
                        {
                            Guid CategoryId;
                            if (Guid.TryParse(categoryId, out CategoryId))
                            {
                                ObjectToCategory albumCategory = new ObjectToCategory
                                {
                                    ObjectId = album.Id,
                                    ObjectType = "Album",
                                    CategoryId = CategoryId,
                                    //Id = Guid.NewGuid()
                                };
                                selectedCategories.Add(CategoryId);
                                db.ObjectToCategories.Add(albumCategory);
                                db.SaveChanges();
                            }
                        }
                    }

                    return Redirect("/Tracks/Create?album=" + album.Id);
                }
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("ERR", exp.Message);
                log.Error("Error", exp);
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
            ViewBag.Error = true;
            return View(album);
        }
        [Authorize]
        // GET: Albums/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            List<ObjectToCategory> albumCategories = db.ObjectToCategories.Where(ac => ac.ObjectId == id && ac.ObjectType == "Album").ToList();
            List<Guid> selectedCategories = new List<Guid>();
            foreach(ObjectToCategory albumCategory in albumCategories)
            {
                selectedCategories.Add(albumCategory.CategoryId);
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Album album)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(album));
            List<Guid> selectedCategories = new List<Guid>();
            if (ModelState.IsValid)
            {
                bool amazon = false;
                bool noDiscs = false;
                Exception e;
                if (album.EntryType == "Amazon")
                {
                    amazon = true;
                    if (!Albums.AmazonAlbum(ref album, out noDiscs, out e, true))
                    {
                        ModelState.AddModelError("ERR", e.Message);
                        log.Error("Error", e);
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }
                }
                else
                {
                    HttpPostedFileBase display_image = Request.Files["ImageFileName"];
                    if (display_image.ContentLength != 0 && HttpPostedFileBaseExtensions.IsImage(display_image))
                    {
                        var fileName = Path.GetFileName(display_image.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images/albums"), fileName);
                        display_image.SaveAs(path);
                        album.ImageFileName = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("ERR", "Album requires an image");
                        log.Error("Album requires an image");
                        ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
                        ViewBag.Error = true;
                        return View(album);
                    }

                    db.Database.ExecuteSqlCommand("delete from ObjectToCategories where ObjectId = '" + album.Id + "' and ObjectType = 'Album'");
                    if (!String.IsNullOrWhiteSpace(Request.Form["categoryTree"]))
                    {
                        string categoryTree = Request.Form["categoryTree"];
                        string[] categoryIds = categoryTree.Split(',');
                        foreach (string categoryId in categoryIds)
                        {
                            Guid CategoryId;
                            if (Guid.TryParse(categoryId, out CategoryId))
                            {
                                ObjectToCategory albumCategory = new ObjectToCategory
                                {
                                    ObjectId = album.Id,
                                    ObjectType = "Album",
                                    CategoryId = CategoryId,
                                    //Id = Guid.NewGuid()
                                };
                                selectedCategories.Add(CategoryId);
                                db.ObjectToCategories.Add(albumCategory);
                                db.SaveChanges();
                            }
                        }
                    }

                }
                album.SortTitle = Core.ApplySortTitle(album.Title);
                album.ModifiedDate = DateTime.Now;
                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { @Id = album.Id });
            }
            ViewBag.CategoryHtml = Categories.GetCategoryHtml(selectedCategories);
            ViewBag.ArtistId = new SelectList(Artists.GetAsObject(null), "Id", "Name", album.ArtistId);
            return View(album);
        }
        [Authorize]
        // GET: Albums/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            Categories.Cleanup("Album", album.Id);
            db.Albums.Remove(album);
            db.SaveChanges();
            return RedirectToAction("Index");
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
            RecentAlbumViewModel recentView = new RecentAlbumViewModel
            {
                Today = new List<Album>(),
                Yesterday = new List<Album>(),
                ThisWeek = new List<Album>(),
                LastWeek = new List<Album>(),
                Older = new Dictionary<int, List<Album>>(),
                Count = 0
            };
            List<Album> albumList = db.Albums.Include(a => a.Artist).OrderByDescending(o => o.CreatedDate).Skip(skip).Take(perPage).ToList();
            string today = DateTime.Now.ToString("yyyyMMdd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

            DateTime todayDate = DateTime.Now;
            while(todayDate.DayOfWeek != DayOfWeek.Sunday)
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


            foreach (Album album in albumList)
            {
                string albumDate = album.CreatedDate.ToString("yyyyMMdd");
                if(albumDate == today)
                {
                    recentView.Today.Add(album);
                }
                else if(albumDate == yesterday)
                {
                    recentView.Yesterday.Add(album);
                }
                else if(String.Compare(albumDate, thisWeekStart) >= 0)
                {
                    recentView.ThisWeek.Add(album);
                }
                else if(String.Compare(albumDate, lastWeekStart) >= 0)
                {
                    recentView.LastWeek.Add(album);
                }
                else
                {
                    int year = album.CreatedDate.Year;
                    if(!recentView.Older.ContainsKey(year))
                    {
                        recentView.Older.Add(year, new List<Album>());
                    }
                    recentView.Older[year].Add(album);
                }
                recentView.Count += 1;
            }
            List<PageLink> links = new List<PageLink>();
            links.Add(new PageLink("Albums By Name", "/Albums"));
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Album", "/Albums/Create"));
            }

            int albumCount = Albums.Count();
            double pageCount = Math.Ceiling((double)albumCount / (double)perPage);
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
                    pages.Add("<a href='/Albums/Recent?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = albumCount;
            ViewBag.LinkList = links;
            return View(recentView);
        }

        //public ActionResult PopulateCategories()
        //{
        //    db.Database.ExecuteSqlCommand("delete from ObjectToCategories");
        //    List<Album> albumList = db.Albums.ToList();
        //    foreach (Album album in albumList)
        //    {               
        //        string ASIN = album.ASIN;
        //        ItemLookupResponse amzResponse;
        //        string amazonXml;
        //        Exception ex;
        //        if (Albums.GetAmazonAlbum(ASIN, out amzResponse, out amazonXml, out ex))
        //        {
        //            List<BrowseNode> categoryNodes = amzResponse.Items.Item.BrowseNodes.ToList();
        //            Categories.PopulateCategories(categoryNodes, album);
        //        }
        //    }
        //    return RedirectToAction("Index");
        //}

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
