using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace BookLibrary.Functions
{
    public class MovieStars
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<string>
                {
                    "Name", "PersonId", "Image", "CreatedDate", "ModifiedDate"
                };
            }
        }

        //public static void MigrateToPeople()
        //{
        //    Exception ex;
        //    List<MovieStar> movieStars = db.MovieStars.ToList();
        //    foreach(MovieStar star in movieStars)
        //    {
        //        Person newStar;
        //        if(People.FindSavePerson(star.Name, true, out newStar, out ex))
        //        {
        //            star.PersonId = newStar.Id;
        //            db.Entry(star).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //        }
        //    }
        //}

        public static List<string> RequiredFields
        {
            get
            {
                return new List<string>
                {
                    "Name"
                };
            }
        }

        public static List<string> MovieToStarFields
        {
            get
            {
                return new List<string>
                {
                    "MovieId", "MovieStarId"
                };
            }
        }

        public static int Count()
        {
            return db.MovieStars.Count();
        }

        public static List<string> MovieToStarRequiredFields
        {
            get
            {
                return MovieToStarFields;
            }
        }

        public static bool FindCreateMovieStarByName(string name, out MovieStar movieStar, out Exception ex)
        {
            movieStar = db.MovieStars.Include(ms => ms.Person).Where(ms => ms.Person.Name == name).FirstOrDefault();
            if (movieStar == null)
            {
                Person newStar;
                if(People.FindSavePerson(name, true, out newStar, out ex))
                {
                    movieStar = new MovieStar
                    {
                        PersonId = newStar.Id,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Id = Guid.NewGuid()
                    };
                    db.MovieStars.Add(movieStar);
                    db.SaveChanges();
                }
            }
            ex = null;
            return true;
        }

        public static List<MovieStar> GetStarsForMovie(List<MovieToMovieStar> stars)
        {
            List<MovieStar> movieStars = new List<MovieStar>();
            foreach(MovieToMovieStar mMovieStar in stars)
            {
                movieStars.Add(db.MovieStars.Include(ms => ms.Person).Where(ms =>ms.Id == mMovieStar.MovieStarId).FirstOrDefault());
            }
            return movieStars.OrderBy(m => m.Person.Name).ToList();
        }

        public static bool AddMovieStarToMovie(string name, Guid movieId, out Exception ex, bool manual = false)
        {
            MovieStar star;
            
            if(FindCreateMovieStarByName(name, out star, out ex))
            {
                if(star != null)
                {
                    MovieToMovieStar movieStar = db.MovieToMovieStars.Where(mms => mms.MovieId == movieId && mms.MovieStarId == star.Id).FirstOrDefault();
                    if(movieStar == null)
                    {
                        movieStar = new MovieToMovieStar
                        {
                            Id = Guid.NewGuid(),
                            MovieId = movieId,
                            MovieStarId = star.Id,
                            ManuallyAdded = manual
                        };
                        try
                        {
                            db.MovieToMovieStars.Add(movieStar);
                            db.SaveChanges();
                            ex = null;
                            return true;
                        }
                        catch (Exception e)
                        {
                            ex = e;
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
        public static List<LibraryObject> GetAsObject(string q)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            List<MovieStar> stars = db.MovieStars.Include(ms => ms.Person).Where(m => m.Person.Name.ToLower().Contains(q.ToLower())).OrderBy(m => m.Person.Name).ToList();
            foreach(MovieStar star in stars)
            {
                
                    objects.Add(new LibraryObject
                    {
                        Id = star.Id,
                        Name = star.Person.Name,
                        Image = star.Person.DisplayImage,
                        Type = "MovieStar"
                    });
            }

            return objects;
        }
    }
}
