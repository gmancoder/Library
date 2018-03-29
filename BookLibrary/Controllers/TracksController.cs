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
    public class TracksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Logger log = new Logger(typeof(TracksController));
        // GET: Tracks
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Albums");
        }

        // GET: Tracks/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Track track = db.Tracks.Include(t => t.Album).Include(t => t.Artist).Where(t => t.Id == id).FirstOrDefault();
            if (track == null)
            {
                return HttpNotFound();
            }
            List<PageLink> links = new List<PageLink>();
            if (User.Identity.IsAuthenticated)
            {
                links.Add(new PageLink("Edit", "/Tracks/Edit/" + track.Id));
                links.Add(new PageLink("Delete", "/Tracks/Delete/" + track.Id, args: @"onClick=""return confirm('Are you sure?');"""));
            }
            ViewBag.LinkList = links;
            return View(track);
        }

        [Authorize]
        // GET: Tracks/Create
        public ActionResult Create()
        {
            string albumId = Request.QueryString["album"];
            if(String.IsNullOrWhiteSpace(albumId))
            {
                return HttpNotFound();
            }
            if (SetupForm(new Guid(albumId)))
            {
                return View();
            }
            return HttpNotFound();
        }

        // POST: Tracks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Track track)
        {
            log.Info("Create");
            log.Info(JsonConvert.SerializeObject(track));
            try
            {
                if(Tracks.TrackExists(track.Name, track.AlbumId, track.TrackNumber, track.DiscNumber))
                {
                    ModelState.AddModelError("ERR", "Track " + track.Name + " already exists for this Album");
                    log.Error("Track " + track.Name + " already exists for this Album");
                    SetupForm(track.AlbumId);
                    return View(track);
                }
                track.Id = Guid.NewGuid();
                //track.Lyrics = track.Lyrics.Replace("\n", "<br />");
                if(String.IsNullOrEmpty(track.Lyrics))
                {
                    track.Lyrics = Tracks.GetLyrics(track.ArtistId, track.Name);
                }
                track.CreatedDate = DateTime.Now;
                track.ModifiedDate = DateTime.Now;
                db.Tracks.Add(track);
                db.SaveChanges();
                if (!String.IsNullOrWhiteSpace(Request.Form["new_on_save"]) && Request.Form["new_on_save"] == "1")
                {
                    return Redirect("/Tracks/Create?album=" + track.AlbumId);
                }
                else
                {
                    return RedirectToAction("Details", "Albums", new { @Id = track.AlbumId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ERR", ex.Message);
                log.Error("Error", ex);
            }

            SetupForm(track.AlbumId);
            return View(track);
        }

        [Authorize]
        // GET: Tracks/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Track track = db.Tracks.Find(id);
            if (track == null)
            {
                return HttpNotFound();
            }
            if (SetupForm(track.AlbumId))
            {
                return View(track);
            }
            return HttpNotFound();
        }

        // POST: Tracks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Track track)
        {
            log.Info("Edit");
            log.Info(JsonConvert.SerializeObject(track));
            try
            {
                track.ModifiedDate = DateTime.Now;
                db.Entry(track).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = track.Id });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("ERR", ex.Message);
                log.Error("Error", ex);
            }
            SetupForm(track.AlbumId);
            return View(track);
        }

        [Authorize]
        // GET: Tracks/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Track track = db.Tracks.Find(id);
            if (track == null)
            {
                return HttpNotFound();
            }

            db.Tracks.Remove(track);
            db.SaveChanges();

            return RedirectToAction("Details", "Albums", new { Id = track.AlbumId });
        }

        private bool SetupForm(Guid albumId)
        {
            Album album = db.Albums.Find(albumId);
            if (album == null)
            {
                return false;
            }
            ViewBag.Album = album;
            if (album.ArtistId == Artists.VariousArtists.Id)
            {
                ViewBag.ArtistId = new SelectList(db.Artists.OrderBy(a => a.Name), "Id", "Name");
                ViewBag.VariousArtists = true;
            }
            else
            {
                ViewBag.VariousArtists = false;
            }
            return true;
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
