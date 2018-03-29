using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.TheTVDb;
using BookLibrary.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookLibrary.Models.ViewModels;
using System.Data.Entity;

namespace BookLibrary.Functions
{
    public class TVShows
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "Title","SortTitle","DisplayImage","CreatedDate","ModifiedDate","Url","Stars"
                };
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "Title"
                };
            }
        }
        public static int Count()
        {
            return db.TVShows.Count();
        }

        public static bool RefreshCast(Guid? id, out TVShow tvShow, out Exception ex)
        {
            db.Configuration.LazyLoadingEnabled = false;
            tvShow = db.TVShows.Find(id);
            if (tvShow == null)
            {
                ex = new KeyNotFoundException("TV Show not found");
                return false;
            }

            db.Database.ExecuteSqlCommand("delete from TVShowToTVStars where TVShowId = '" + tvShow.Id + "' and ManuallyAdded = 0");
            if (!TVShows.PopulateTVStars(tvShow, out ex))
            {
                return false;
            }

            ex = null;
            return true;
        }

        public static bool PopulateTVStars(TVShow tvShow, out Exception ex)
        {
            if (!String.IsNullOrEmpty(tvShow.Stars))
            {
                string[] actors = tvShow.Stars.Split(',');
                foreach (string actor in actors)
                {
                    if (!TVStars.AddTVStarToTVShow(actor.Trim(), tvShow.Id, out ex))
                    {
                        return false;
                    }
                }
            }
            ex = null;
            return true;
        }

        public static bool UpdateShow(ref TVShow tvShow, out Exception ex)
        {
            ex = null;
            TheTVDbService tvService = new TheTVDbService("grbrewer", "E72C866586E902FE");
            if(tvService.LoggedIn())
            {
                SearchResponse response = tvService.SearchForShow(tvShow.Title);
                if(response == null)
                {
                    ex = new Exception("SearchResponse error");
                    return false;
                }

                Int32 seriesId = response.data[0].id;
                SeriesResponse dataResponse = tvService.GetShowData(seriesId);
                if(dataResponse == null)
                {
                    ex = new Exception("SeriesResponse error");
                    return false;
                }

                SeriesData data = dataResponse.data;
                tvShow.ModifiedDate = DateTime.Now;
                tvShow.Title = data.seriesName;
                tvShow.SortTitle = Core.Core.ApplySortTitle(tvShow.Title);
                tvShow.FirstAired = data.firstAired;
                tvShow.Genres = String.Join(", ", data.genre);
                tvShow.Network = data.network;
                tvShow.Overview = data.overview;
                tvShow.Rating = data.rating;
                tvShow.Runtime = data.runtime;
                tvShow.Status = data.status;
                tvShow.TVDbId = data.id;
                tvShow.Url = "http://www.imdb.com/title/" + data.imdbId;
                tvShow.TVDbResponse = JsonConvert.SerializeObject(response) + "~|~" + JsonConvert.SerializeObject(dataResponse);

                //Stars
                SeriesActorResponse actorData = tvService.GetShowActors(data.id);
                if(actorData == null)
                {
                    ex = new Exception("SeriesResponse error");
                    return false;
                }

                if (actorData.data != null && actorData.data.Count() > 0)
                {
                    string actorString = "";
                    foreach (SeriesActor actor in actorData.data)
                    {
                        if (actorString != "")
                        {
                            actorString += ",";
                        }
                        actorString += actor.name;
                    }
                    tvShow.Stars = actorString;
                }

                //Poster
                SeriesImageResponse imageData = tvService.GetSeriesImages(data.id, "poster");
                if(imageData == null)
                {
                    ex = new Exception("SeriesResponse error");
                    return false;
                }

                SeriesImage poster = imageData.data[0];
                string dlUrl = "http://thetvdb.com/banners/" + poster.fileName;
                string destination = ConfigurationManager.AppSettings["TVShowImagePath"] + "\\" + tvShow.Id.ToString() + "." + Path.GetExtension(poster.fileName);
                if(!Core.Core.DownloadFileFromUrl(dlUrl, destination, out ex))
                {
                    return false;
                }
                tvShow.DisplayImage = tvShow.Id.ToString() + "." + Path.GetExtension(poster.fileName);
                return true;
            }

            ex = new Exception("TVDB Service Login Failed");
            return false;
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            return GetAsObject(db.TVShows.Where(tv => tv.Title.ToLower().Contains(q.ToLower())).ToList());
        }

        public static List<LibraryObject> GetAsObject(int v = 0)
        {
            List<TVShow> tvShows = new List<TVShow>();
            if(v > 0)
            {
                tvShows = db.TVShows.OrderByDescending(tv => tv.CreatedDate).Take(v).ToList();
            }
            else
            {
                tvShows = db.TVShows.OrderByDescending(tv => tv.CreatedDate).ToList();
            }
            return GetAsObject(tvShows);
        }

        public static List<LibraryObject> GetAsObject(List<TVShow> tvShows)
        {
            List<LibraryObject> tvShowObjects = new List<LibraryObject>();
            foreach(TVShow tvShow in tvShows)
            {
                tvShowObjects.Add(AsObject(tvShow));
            }
            return tvShowObjects;
        }

        private static LibraryObject AsObject(TVShow show)
        {
            return new LibraryObject
            {
                Name = show.Title,
                SortName = show.SortTitle,
                CreatedDate = show.CreatedDate,
                Id = show.Id,
                Image = "/Content/Images/tvshows/" + show.DisplayImage,
                Type = "TVShow"
            };
        }

        public static List<LibraryObject> GetTVShowsForPerson(Guid id)
        {
            List<LibraryObject> tvShows = new List<LibraryObject>();
            TVStar tvStar = db.TVStars.Include(tv => tv.TVShows).Where(tv => tv.PersonId == id).FirstOrDefault();
            if(tvStar != null)
            {
                foreach(TVShowToTVStar show in tvStar.TVShows)
                {
                    TVShow tvShow = db.TVShows.Find(show.TVShowId);
                    if(tvShow != null)
                    {
                        tvShows.Add(AsObject(tvShow));
                    }
                }
            }
            return tvShows;
        }
    }
}
