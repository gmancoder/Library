using BookLibrary.Functions;
using BookLibrary.Functions.Core;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Humanizer;
using BookLibrary.Models;
using BookLibrary.Services;
using BookLibrary.Models.ServiceModels.TheTVDb;

namespace BookLibrary.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //private void UpdateObjectToCategories()
        //{
        //    List<ObjectToCategory> allOCategories = db.ObjectToCategories.ToList();
        //    int idx = 1;
        //    foreach(ObjectToCategory category in allOCategories)
        //    {
        //        category.Id2 = idx;
        //        idx += 1;
        //        db.Entry(category).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //}
        public ActionResult TestTV()
        {
            TheTVDbService service = new TheTVDbService("grbrewer", "E72C866586E902FE");
            if(service.LoggedIn())
            {
                SearchResponse response = service.SearchForShow("Dukes of Hazzard");
                if(response != null && response.data.Count() > 0)
                {
                    SeriesResponse seriesInfo = service.GetShowData(response.data[0].id);
                    SeriesActorResponse seriesActors = service.GetShowActors(response.data[0].id);
                    SeriesEpisodeSummaryResponse seriesEpisodeData = service.GetEpisodeSummary(response.data[0].id);
                    SeriesImageResponse seriesImages = service.GetSeriesImages(response.data[0].id, "poster");
                }
            }
            return View("Index");
        }
        public ActionResult Index()
        {
            //UpdateObjectToCategories();
            
            HomeViewModel homeView = new HomeViewModel();
            TrackOfTheDay trackOfTheDay = Tracks.GetTrackOfTheDay();
            if (trackOfTheDay != null)
            {
                homeView.TrackOfTheDay = db.Tracks.Include(t => t.Album).Include(t => t.Artist).Where(t => t.Id == trackOfTheDay.TrackId).FirstOrDefault();
            }
            homeView.Reading = Books.GetReading();
            Dictionary<string, int> Counts = new Dictionary<string, int>();

            List<List<LibraryObject>> latest = new List<List<LibraryObject>>();
            List<LibraryObject> objects = Books.GetAsObject(8);
            Counts.Add("Book", Books.Count());
            latest.Add(objects);
            objects = MagazineIssues.GetAsObject(8);
            Counts.Add("Magazine", Magazines.Count());
            latest.Add(objects);
            objects = Albums.GetAsObject(8);
            Counts.Add("Album", Albums.Count());
            latest.Add(objects);
            objects = Movies.GetAsObject(8);
            Counts.Add("Movie", Movies.Count());
            latest.Add(objects);
            objects = TVShows.GetAsObject(8);
            Counts.Add("TV Show", TVShows.Count());
            latest.Add(objects);

            homeView.LatestAdditions = Core.FlattenObjects(latest, 8, true);

            String CurrentlyCataloged = "";
            int idx = 0;
            foreach(KeyValuePair<string, int> catalogCount in Counts)
            {
                idx += 1;
                if(CurrentlyCataloged != "")
                {
                    CurrentlyCataloged += ", ";
                }
                if(idx == Counts.Keys.Count())
                {
                    CurrentlyCataloged += " and ";
                }
                CurrentlyCataloged += "<a href='/" + catalogCount.Key.Pluralize().Replace(" ", "") + "'>" + catalogCount.Key.ToLower().ToQuantity(catalogCount.Value) + "</a>";
            }
            ViewBag.SubTitle = "We currently have " + CurrentlyCataloged + " cataloged here";

            return View(homeView);
        }

        public ActionResult Search()
        {
            string q = Request.QueryString["q"];
            if (String.IsNullOrEmpty(q))
            {
                RedirectToAction("Index");
            }
            Dictionary<string, List<LibraryObject>> results = new Dictionary<string, List<LibraryObject>>
            {
                {"Author", Authors.GetAsObject(q) },
                    {"Book", Books.GetAsObject(q) },
                    {"Magazine", Magazines.GetAsObject(q) },
                    {"Artist", Artists.GetAsObject(q) },
                    {"Album", Albums.GetAsObject(q) },
                    {"Track", Tracks.GetAsObject(q) },
                    {"Movie", Movies.GetAsObject(q) },
                    {"Movie Star", MovieStars.GetAsObject(q) },
                {"TV Show", TVShows.GetAsObject(q) },
                {"TV Star", TVStars.GetAsObject(q) }
            };
            SearchViewModel searchView = new SearchViewModel
            {
                Results = new Dictionary<string, List<LibraryObject>> (),
                Query = q
            };
            String countString = "";
            foreach(KeyValuePair<string, List<LibraryObject>> result in results)
            {
                if(result.Value.Count() > 0)
                {
                    searchView.Results.Add(result.Key, result.Value);
                    if(countString != "")
                    {
                        countString += "<br />";
                    }
                    countString += result.Value.Count() + " " + result.Key.Pluralize().ToLower();
                }
            }
            if (countString != "")
            {
                ViewBag.SubTitle = "Query returned " + countString;
            }
            else
            {
                ViewBag.SubTitle = "Query returned no results";
            }
            return View(searchView);
        }
    }
}