using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public static class Artists
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        
        public static List<string> Fields
        {
            get
            {
                return new List<String> { "PersonId", "Name", "IsGroup", "CreatedDate", "ModifiedDate", "DisplayImage" };
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<String> { "Name", "IsGroup" };
            }
        }

        public static void MergeToPeople()
        {
            Exception ex;
            List<Artist> allArtists = db.Artists.ToList();
            foreach(Artist artist in allArtists)
            {
                if (!artist.IsGroup)
                {
                    Person newArtist;
                    if (People.FindSavePerson(artist.Name, true, out newArtist, out ex))
                    {
                        artist.PersonId = newArtist.Id;
                        artist.Name = null;
                        artist.SortName = null;
                        artist.DisplayImage = null;
                        db.Entry(artist).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        //public static void ApplySortName()
        //{
        //    List<Artist> allArtists = db.Artists.ToList();
        //    foreach(Artist artist in allArtists)
        //    {
        //        artist.SortName = Core.Core.ApplySortTitle(artist.Name);
        //        db.Entry(artist).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //}

        public static Artist VariousArtists
        {
            get
            {
                return db.Artists.Where(a => a.Name == "Various Artists").FirstOrDefault();
            }
        }

       public static string GetArtistName(Guid artistId)
        {
            Artist artist = db.Artists.Include(a => a.Person).Where(a => a.Id == artistId).FirstOrDefault();
            if(artist == null)
            {
                return "";
            }
            else if(artist.IsGroup)
            {
                return artist.Name;
            }
            else
            {
                return artist.Person.Name;
            }
        }

        public static bool ArtistExists(string Name)
        {
            Person person;
            People.PersonExistsByName(Name, out person);
            return person != null;
        }

        public static bool GroupExists(string Name)
        {
            return db.Artists.Where(a => a.Name == Name).Count() > 0;
        }

        public static int Count()
        {
            return db.Artists.Count();
        }

        public static bool GetCelebrityImage(Guid CelebrityId, out Uri image, out Exception ex)
        {
            image = null;
            ex = null;
            PhotoGalleryService psService = new PhotoGalleryService("grbrewer@gmail.com", "!Pass248word");
            
            if(psService.LoggedIn())
            {
                Celebrity celebrity;
                if (GetCelebrity(CelebrityId, out celebrity, out ex))
                {
                    image = psService.GetPrimaryImage(celebrity.AlbumId);
                    if (image == null)
                    {
                        ex = new Exception("Primary Image for Album not found");
                        return false;
                    }

                    return true;
                }
            }
            ex = new Exception("Login Failed for Photo Gallery");
            return false;
        }

        public static bool GetCelebrity(Guid CelebrityId, out Celebrity celebrity, out Exception ex)
        {
            celebrity = null;
            ex = null;
            CelebrityCentralService ccService = new CelebrityCentralService("grbrewer@gmail.com", "!Pass248word");
            if (ccService.LoggedIn())
            {
                List<Celebrity> celebrities = ccService.GetCelebrity(CelebrityId);
                if (celebrities.Count() == 0)
                {
                    ex = new Exception("Celebrity with ID " + CelebrityId.ToString() + " not found");
                    return false;
                }
                celebrity = celebrities[0];
                return true;
            }
            ex = new Exception("Login Failed for Celebrity Central");
            return false;
        }

        public static List<Artist> GetArtists(string letter = "")
        {
            db.Configuration.LazyLoadingEnabled = false;
            if (!String.IsNullOrEmpty(letter))
            {
                return db.Artists.Include(a => a.Person).Where(a => a.Person.Name.StartsWith(letter)).OrderBy(a => a.Person.Name).ToList();
            }
            return db.Artists.Include(a => a.Person).OrderBy(a => a.Person.Name).ToList();
            
        }

        public static bool FindCelebrityByName(string name, out Celebrity celebrity, out Exception ex)
        {
            ex = null;
            celebrity = null;
            CelebrityCentralService ccService = new CelebrityCentralService("grbrewer@gmail.com", "!Pass248word");
            if (ccService.LoggedIn())
            {
                List<Celebrity> celebrities = ccService.FindCelebrity(name);
                if (celebrities.Count() == 0)
                {
                    ex = new Exception("Celebrity with Name " + name + " not found");
                    return false;
                }
                celebrity = celebrities[0];
                return true;
            }
            ex = new Exception("Login Failed for Celebrity Central");
            return false;
        }

        public static bool FindCreateArtistByName(string name, out Artist artist, out Exception ex)
        {
            ex = null;
            artist = null;
            try
            {
                artist = db.Artists.Where(a => a.Name == name).FirstOrDefault();
                if (artist == null)
                {
                    Artist a = new Artist();
                    a.Id = Guid.NewGuid();
                    a.CreatedDate = DateTime.Now;
                    a.ModifiedDate = DateTime.Now;
                    Person newArtist;
                    if (People.FindSavePerson(name, true, out newArtist, out ex))
                    {
                        a.IsGroup = false;
                        a.PersonId = newArtist.Id;
                    }
                    else
                    {
                        a.Name = name;
                        a.SortName = Core.Core.ApplySortTitle(name);
                        a.IsGroup = true;
                        a.DisplayImage = "/Content/images/UnknownArtistLogo.jpg";
                    }
                    
                    db.Artists.Add(a);
                    db.SaveChanges();
                    artist = a;
                }
                return true;
            }
            catch(Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            List<Artist> artists = new List<Artist>();
            if(!String.IsNullOrEmpty(q))
            {
                artists = db.Artists.Include(a => a.Person).Where(a => a.Name.ToLower().Contains(q.ToLower())).ToList();
            }
            else
            {
                artists = db.Artists.Include(a => a.Person).ToList();
            }
            foreach (Artist artist in artists)
            {
                objects.Add(AsObject(artist));
            }

            return objects.OrderBy(o => o.SortName).ToList();
        }

        public static LibraryObject AsObject(Artist artist)
        {
            if (!artist.IsGroup)
            {
                return new LibraryObject
                {
                    Id = artist.Id,
                    Name = artist.Person.Name,
                    Image = artist.Person.DisplayImage,
                    SortName = artist.Person.Name,
                    Type = "Artist"
                };
            }
            else
            {
                return new LibraryObject
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Image = artist.DisplayImage,
                    SortName = Core.Core.ApplySortTitle(artist.Name),
                    Type = "Artist"
                };
            }
        }

        
    }
}
