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
using System.IO;
using BookLibrary.Functions.Core;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using Newtonsoft.Json;

namespace BookLibrary.Web.Controllers
{
    public class ArtistsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(ArtistsController));
        // GET: Artists
        public ActionResult Index()
        {
            //Artists.ApplySortName();
            //Artists.MergeToPeople();
            int page = 1;
            if (!String.IsNullOrEmpty(Request.QueryString["p"]))
            {
                Int32.TryParse(Request.QueryString["p"], out page);
            }
            int perPage = Config.Get<Int32>("ItemsPerPage");
            int skip = (page - 1) * perPage;
            if (User.Identity.IsAuthenticated)
            {
                List<PageLink> links = new List<PageLink>();
                links.Add(new PageLink("Add Artist", "/Artists/Create"));
                ViewBag.LinkList = links;
            }
            int artistCount = Artists.Count();
            double pageCount = Math.Ceiling((double)artistCount / (double)perPage);
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
                    pages.Add("<a href='/Artists?p=" + idx + "'>" + idx + "</a>&nbsp;");
                }
                else
                {
                    pages.Add("<strong>" + idx + "</strong>&nbsp;");
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            ViewBag.TotalItems = artistCount;

            List<Artist> allArtists = db.Artists.Include(a => a.Person).ToList();
            List<LibraryObject> artistsView = new List<LibraryObject>();
            foreach(Artist artist in allArtists)
            {
                LibraryObject newObject = new LibraryObject();
                newObject.Id = artist.Id;
                if(artist.PersonId.HasValue)
                {
                    newObject.Name = artist.Person.Name;
                    newObject.Image = artist.Person.DisplayImage;
                    newObject.SortName = artist.Person.SortName;
                }
                else
                {
                    newObject.Name = artist.Name;
                    newObject.Image = artist.DisplayImage;
                    newObject.SortName = artist.SortName;
                }
                artistsView.Add(newObject);
            }

            return View(artistsView.OrderBy(o => o.SortName).Skip(skip).Take(perPage).ToList());

        }

        // GET: Artists/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Include(a=>a.Person).Include(a => a.AssociatedArtists).Where(a => a.Id == id).FirstOrDefault();
            
            if (artist == null)
            {
                return HttpNotFound();
            }
            List<PageLink> links = new List<PageLink>();
            PersonViewModel viewModel;
            Exception ex;
            string view = "PeopleDetails";
            if (artist.IsGroup)
            {
                view = "Details";
                ArtistViewModel artistView = new ArtistViewModel
                {
                    Artist = artist,
                    Albums = db.Albums.Where(a => a.ArtistId == artist.Id).OrderBy(a => a.Title).ToList(),
                    AssociatedArtists = new List<LibraryObject>()
                };
                try
                {
                    artistView.NonAlbumTracks = Tracks.GetNonAlbumTracksForArtist(artist.Id);
                }
                catch
                {
                    artistView.NonAlbumTracks = new List<Track>();
                }
                
                if (artistView.Artist.AssociatedArtists.Count() > 0)
                {
                    links.Add(new PageLink("Associated Artists", "#associatedArtists"));
                    foreach (Artist assocArtist in artist.AssociatedArtists)
                    {
                        artistView.AssociatedArtists.Add(Artists.AsObject(assocArtist));
                    }
                }
                if (artistView.Albums.Count() > 0)
                {
                    links.Add(new PageLink("Albums", "#albums"));
                }
                if (artistView.NonAlbumTracks.Count() > 0)
                {
                    links.Add(new PageLink("Tracks on Other Albums", "#tracks"));
                }
                AppendUserActions(ref links, artist);
                ViewBag.LinkList = links;
                return View(view, artistView);
            }
            else
            {
                if (!People.GeneratePersonView(id.Value, "Artist", out viewModel, out ex))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
                }

                People.DrawLinkListForView(viewModel, "Artist", ref links);
                AppendUserActions(ref links, artist);
                ViewBag.LinkList = links;
                return View(view, viewModel);
            }
        }

        private void AppendUserActions(ref List<PageLink> links, Artist artist)
        {
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Add Album", "/Albums/Create?artist=" + artist.Id));
                if (artist.IsGroup)
                {
                    links.Add(new PageLink("Add Associated Artist(s)", "/Artists/AddAssociated/" + artist.Id));
                }
                links.Add(new PageLink("Edit", "/Artists/Edit/" + artist.Id));
                links.Add(new PageLink("Delete", "/Artists/Delete/" + artist.Id, args: @"onClick=""return confirm('Are you sure?');"""));
            }
        }

        [Authorize]
        // GET: Artists/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Artists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Artist artist)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(artist));
            Exception ex;
            if (ModelState.IsValid)
            {
                string Name = Request.Form["Name"];
                if (!String.IsNullOrEmpty(Name))
                {
                    if ((artist.IsGroup && Artists.GroupExists(Name)) || (!artist.IsGroup && Artists.ArtistExists(Name)))
                    {
                        ModelState.AddModelError("ERR", "Artist " + artist.Name + " already exists");
                        log.Error("Artist " + artist.Name + " already exists");
                        return View(artist);
                    }
                    artist.Id = Guid.NewGuid();
                    artist.CreatedDate = DateTime.Now;

                    if (!UpdateArtist(ref artist, Name, out ex))
                    {
                        ModelState.AddModelError("ERR", ex.Message);
                        log.Error("Error", ex);
                        return View(artist);
                    }

                    db.Artists.Add(artist);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = artist.Id });
                }
                ModelState.AddModelError("ERR", "Name Required");
            }

            return View(artist);
        }

        [Authorize]
        // GET: Artists/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }
            return View(artist);
        }

        // POST: Artists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Artist artist)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(artist));
            Exception ex;
            if (ModelState.IsValid)
            {
                string Name = Request.Form["Name"];
                if (!String.IsNullOrEmpty(Name))
                {
                    if (!UpdateArtist(ref artist, Name, out ex))
                    {
                        ModelState.AddModelError("ERR", ex.Message);
                        log.Error("Error", ex);
                        return View(artist);
                    }
                    db.Entry(artist).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = artist.Id });
                }
                ModelState.AddModelError("ERR", "Name Required");
            }
            return View(artist);
        }

        [Authorize]
        // GET: Artists/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }

            db.Artists.Remove(artist);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult AddAssociated(Guid? id)
        {
            Artist artist = db.Artists.Include(a => a.AssociatedArtists).Include(a => a.Person).Where(a => a.Id == id).FirstOrDefault();
            if (artist == null)
            {
                return HttpNotFound();
            }

            return View(artist);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateAssociated(Guid Id)
        {
            Artist artist = db.Artists.Include(a => a.AssociatedArtists).Where(a => a.Id == Id).FirstOrDefault();
            if (artist == null)
            {
                return HttpNotFound();
            }

            string[] associated_artist_ids = Request.Form.GetValues("artist_id[]");
            foreach(string associated_artist_id in associated_artist_ids)
            {
                Artist associated_artist = db.Artists.Find(new Guid(associated_artist_id));
                if(associated_artist != null)
                {
                    associated_artist.ArtistId = Id;
                    associated_artist.ModifiedDate = DateTime.Now;
                    db.Entry(associated_artist).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Details", new { Id = Id });
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UpdateArtist(ref Artist artist, string Name, out Exception ex)
        {
            ex = null;
            artist.ModifiedDate = DateTime.Now;
            if (artist.IsGroup)
            {
                artist.Name = Name;
                artist.SortName = Core.ApplySortTitle(Name);
                HttpPostedFileBase display_image = Request.Files["ArtistDisplayImage"];
                if (display_image.ContentLength == 0)
                {
                    ex = new Exception("Image Upload required for groups");
                    return false;
                }
                else if (HttpPostedFileBaseExtensions.IsImage(display_image))
                {
                    var fileName = Path.GetFileName(display_image.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/artists"), fileName);
                    display_image.SaveAs(path);
                    artist.DisplayImage = "/Content/images/artists/" + fileName;
                }
                else
                {
                    ex = new Exception("Artist requires an image");
                    return false;
                }
            }
            else
            {
                Person newArtist;
                if(!People.FindSavePerson(Name, true, out newArtist, out ex))
                {
                    return false;
                }
                artist.PersonId = newArtist.Id;
            }

            return true;
        }
    }
}
