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
    public class TVStars
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
        //    List<TVStar> tvStars = db.TVStars.ToList();
        //    foreach(TVStar star in tvStars)
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

        public static List<string> TVShowToStarFields
        {
            get
            {
                return new List<string>
                {
                    "TVShowId", "TVStarId"
                };
            }
        }

        public static int Count()
        {
            return db.TVStars.Count();
        }

        public static List<string> TVShowToStarRequiredFields
        {
            get
            {
                return TVShowToStarFields;
            }
        }

        public static bool FindCreateTVStarByName(string name, out TVStar tvStar, out Exception ex)
        {
            tvStar = db.TVStars.Include(ms => ms.Person).Where(ms => ms.Person.Name == name).FirstOrDefault();
            if (tvStar == null)
            {
                Person newStar;
                if (People.FindSavePerson(name, true, out newStar, out ex))
                {
                    tvStar = new TVStar
                    {
                        PersonId = newStar.Id,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Id = Guid.NewGuid()
                    };
                    db.TVStars.Add(tvStar);
                    db.SaveChanges();
                }
            }
            ex = null;
            return true;
        }

        public static List<TVStar> GetStarsForTV(List<TVShowToTVStar> stars)
        {
            List<TVStar> tvStars = new List<TVStar>();
            foreach (TVShowToTVStar mTVStar in stars)
            {
                tvStars.Add(db.TVStars.Include(ms => ms.Person).Where(ms => ms.Id == mTVStar.TVStarId).FirstOrDefault());
            }
            return tvStars.OrderBy(m => m.Person.Name).ToList();
        }

        public static bool AddTVStarToTVShow(string name, Guid tvShowId, out Exception ex, bool manual = false)
        {
            TVStar star;

            if (FindCreateTVStarByName(name, out star, out ex))
            {
                if (star != null)
                {
                    TVShowToTVStar tvStar = db.TVShowToTVStars.Where(mms => mms.TVShowId == tvShowId && mms.TVStarId == star.Id).FirstOrDefault();
                    if (tvStar == null)
                    {
                        tvStar = new TVShowToTVStar
                        {
                            Id = Guid.NewGuid(),
                            TVShowId = tvShowId,
                            TVStarId = star.Id,
                            ManuallyAdded = manual
                        };
                        try
                        {
                            db.TVShowToTVStars.Add(tvStar);
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
            List<TVStar> stars = db.TVStars.Include(ms => ms.Person).Where(m => m.Person.Name.ToLower().Contains(q.ToLower())).OrderBy(m => m.Person.Name).ToList();
            foreach (TVStar star in stars)
            {

                objects.Add(new LibraryObject
                {
                    Id = star.Id,
                    Name = star.Person.Name,
                    Image = star.Person.DisplayImage,
                    Type = "TVStar"
                });
            }

            return objects;
        }
    }
}
