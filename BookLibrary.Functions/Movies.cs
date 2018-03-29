using BookLibrary.Models;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BookLibrary.Models.ViewModels;

namespace BookLibrary.Functions
{
    public class Movies
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "ASIN","EAN","UPC","Title","ReleaseDate","Binding","ImageFileName","CreatedDate","ModifiedDate","EntryType","Url","AmazonResponse","Starring", "RunningTime", "Publisher", "ProductGroup", "Manufacturer", "Genre", "Director", "AudienceRating", "IsAdultProduct"
                };
            }
        }

        public static void ApplySortTitle()
        {
            List<Movie> allMovies = db.Movies.ToList();
            foreach(Movie movie in allMovies)
            {
                movie.SortTitle = Core.Core.ApplySortTitle(movie.Title);
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "Title","EntryType"
                };
            }
        }
        public static bool AmazonMovieExists(string aSIN)
        {
            return db.Movies.Where(m => m.ASIN == aSIN).Count() > 0;
        }

        public static bool AmazonMovie(ref Movie movie, out Exception ex, bool edit = false)
        {
            ex = null;
            ItemLookupResponse amzResponse;
            string amazonResponse;
            if (!GetAmazonMovie(movie.ASIN, out amzResponse, out amazonResponse, out ex))
            {
                return false;
            }

            try
            {
                movie.AmazonResponse = amazonResponse;
                Item amzItem = amzResponse.Items.Item;
                if(amzItem == null)
                {
                    ex = new Exception("No Amazon Item returned");
                    return false;
                }
                ItemAttributes amzItemAttrib = amzItem.ItemAttributes;
                movie.Binding = amzItemAttrib.Binding;
                movie.EAN = amzItemAttrib.EAN;
                movie.ReleaseDate = amzItemAttrib.ReleaseDate.ToShortDateString();
                movie.Title = amzItemAttrib.Title;
                movie.UPC = amzItemAttrib.UPC;
                movie.Url = amzItem.DetailPageURL;
                movie.RunningTime = amzItemAttrib.RunningTime;
                movie.Publisher = amzItemAttrib.Publisher;
                movie.ProductGroup = amzItemAttrib.ProductGroup;
                movie.Manufacturer = amzItemAttrib.Manufacturer;
                movie.Genre = String.Join(",", amzItemAttrib.Genre);
                movie.Director = amzItemAttrib.Director;
                movie.AudienceRating = amzItemAttrib.AudienceRating;
                movie.IsAdultProduct = amzItemAttrib.IsAdultProduct;
                List<string> actors = amzItemAttrib.Actors;
                movie.Starring = String.Join(",", actors);
                //Image
                string filename = ConfigurationManager.AppSettings["MovieImagePath"] + "\\unknown_movie.png";
                if (amzItem.LargeImage != null)
                {
                    string[] image_pieces = amzItem.LargeImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = movie.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["MovieImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.LargeImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if (amzItem.MediumImage != null)
                {
                    string[] image_pieces = amzItem.MediumImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = movie.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["MovieImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.MediumImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else if (amzItem.SmallImage != null)
                {
                    string[] image_pieces = amzItem.SmallImage.URL.Split('/');
                    filename = image_pieces[image_pieces.Length - 1];
                    filename = movie.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    string new_path = ConfigurationManager.AppSettings["MovieImagePath"] + "\\" + filename;
                    if (!Core.Core.DownloadFileFromUrl(amzItem.SmallImage.URL, new_path, out ex))
                    {
                        return false;
                    }
                }
                else
                {
                    string new_filename = ConfigurationManager.AppSettings["MovieImagePath"] + "\\" + movie.Id.ToString().Replace("-", "") + "." + Path.GetExtension(filename);
                    File.Copy(filename, new_filename);
                    filename = new_filename;
                }
                movie.ImageFileName = filename;
                movie.ModifiedDate = DateTime.Now;
                if (!edit)
                {
                    movie.SortTitle = Core.Core.ApplySortTitle(movie.Title);
                    db.Movies.Add(movie);
                    db.SaveChanges();
                }
                else
                {
                    db.Database.ExecuteSqlCommand("delete from MovieToMovieStars where MovieId = '" + movie.Id + "' and ManuallyAdded = 0");
                    Categories.Cleanup("Movie", movie.Id);
                }
                
                if(actors.Count() > 0)
                {
                    foreach (string actor in actors)
                    {
                        if (!MovieStars.AddMovieStarToMovie(actor, movie.Id, out ex))
                        {
                            return false;
                        }
                    }
                }

                
                Categories.PopulateCategories<Movie>(amzItem.BrowseNodes.ToList(), movie);

                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<LibraryObject> GetAsObject(int take = 5000)
        {
            List<Movie> movies = db.Movies.OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            return MoviesToObjects(movies);
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<Movie> movies = db.Movies.Where(m => m.Title.ToLower().Contains(q.ToLower())).OrderBy(m => m.Title).ToList();
            return MoviesToObjects(movies);
        }

        private static List<LibraryObject> MoviesToObjects(List<Movie> movies)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            
            foreach (Movie movie in movies)
            {
                objects.Add(MovieToObject(movie));
            }
            return objects;
        }
        private static LibraryObject MovieToObject(Movie movie)
        {
            return new LibraryObject
            {
                Type = "Movie",
                Id = movie.Id,
                Name = movie.Title,
                Image = "/Content/Images/movies/" + movie.ImageFileName,
                CreatedDate = movie.CreatedDate
            };
        }
        public static int Count()
        {
            return db.Movies.Count();
        }

        public static bool GetAmazonMovie(string aSIN, out ItemLookupResponse response, out string amazonXml, out Exception ex)
        {
            ex = null;
            response = null;
            amazonXml = "";
            AmazonService amzService = new AmazonService();
            int tries = 0;
            int retries = 5;
            while (true)
            {
                try
                {
                    amazonXml = amzService.SearchAmazon(aSIN, "ASIN").ToString();
                    if(!Core.Core.ParseAmazonXml(amazonXml, out response, out ex))
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    ex = e;
                    tries += 1;
                    if (tries > retries)
                    {
                        return false;
                    }
                    Thread.Sleep(15);
                }
            }
        }

        

        public static bool ManualMovieExists(string title)
        {
            return db.Movies.Where(m => m.Title == title).Count() > 0;
        }

        public static MovieStarDetailViewModel MovieStarToView(MovieStar star)
        {
            MovieStarDetailViewModel starView = new MovieStarDetailViewModel
            {
                Id = star.Id,
                Name = star.Person.Name,
                Image = star.Person.DisplayImage,
                MovieCount = 0,
                Movies = new List<Movie>()
            };
           
            foreach (MovieToMovieStar record in star.Movies)
            {
                starView.MovieCount += 1;
                starView.Movies.Add(db.Movies.Find(record.MovieId));
            }

            return starView;
        }

        public static bool PopulateMovieStars(Movie movie, out Exception ex)
        {
            if(!String.IsNullOrEmpty(movie.Starring))
            {
                string[] actors = movie.Starring.Split(',');
                foreach (string actor in actors)
                {
                    if (!MovieStars.AddMovieStarToMovie(actor.Trim(), movie.Id, out ex))
                    {
                        return false;
                    }
                }
            }
            ex = null;
            return true;
        }

        public static bool RefreshCast(Guid? id, out Movie movie, out Exception ex)
        {
            db.Configuration.LazyLoadingEnabled = false;
            movie = db.Movies.Find(id);
            if (movie == null)
            {
                ex = new KeyNotFoundException("Movie not found");
                return false;
            }

            db.Database.ExecuteSqlCommand("delete from MovieToMovieStars where MovieId = '" + movie.Id + "' and ManuallyAdded = 0");
            if (!Movies.PopulateMovieStars(movie, out ex))
            {
                return false;
            }

            ex = null;
            return true;
        }

        public static List<LibraryObject> GetMoviesForPerson(Guid personId)
        {
            List<LibraryObject> movies = new List<LibraryObject>();
            MovieStar star = db.MovieStars.Include(ms => ms.Movies).Where(ms => ms.PersonId == personId).FirstOrDefault();
            if(star != null)
            {
                foreach(MovieToMovieStar movie in star.Movies)
                {
                    Movie m = db.Movies.Find(movie.MovieId);
                    if(m != null)
                    {
                        movies.Add(MovieToObject(m));
                    }
                }
            }
            return movies;
        }
    }
}
