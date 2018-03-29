using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public class Authors
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static List<string> Fields
        {
            get
            {
                return new List<String> { "Name", "DisplayImage", "PersonId", "CreatedDate", "ModifiedDate" };
            }
        }

        public static List<string> RequiredFields
        {
            get
            {
                return new List<String> {  };
            }
        }

        public static void MigrateToPeople()
        {
            //List<Author> allAuthors = db.Authors.ToList();
            //foreach(Author author in allAuthors)
            //{
            //    Person newPerson;
            //    Exception ex;
            //    People.FindSavePerson(author.Name, false, out newPerson, out ex);
            //    author.PersonId = newPerson.Id;
            //    db.Entry(author).State = EntityState.Modified;
            //    db.SaveChanges();
            //}
        }

        public static bool AuthorExists(string name)
        {
            return db.Authors.Include(a =>a.Person).Where(a => a.Person.Name == name).Count() > 0;
        }
        
        public static Author AuthorById(Guid authorId)
        {
            return db.Authors.Include(a => a.Person).Where(a => a.Id == authorId).FirstOrDefault();
        }

        public static bool GetCelebrityImage(Guid CelebrityId, out Uri image, out Exception ex)
        {
            image = null;
            ex = null;
            PhotoGalleryService psService = new PhotoGalleryService("grbrewer@gmail.com", "!Pass248word");

            if (psService.LoggedIn())
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

        public static int Count()
        {
            return db.Authors.Count();
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

        public static List<Author> GetAuthors(string letter = "")
        {
            db.Configuration.LazyLoadingEnabled = false;
            if (!String.IsNullOrEmpty(letter))
            {
                return db.Authors.Include(a => a.Person).Where(a => a.Person.Name.StartsWith(letter)).OrderBy(a => a.Person.Name).ToList();
            }
            return db.Authors.Include(a=>a.Person).OrderBy(a => a.Person.Name).ToList();

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

        public static bool FindCreateAuthorByName(string name, out Author author, out Exception ex)
        {
            ex = null;
            author = null;
            try
            {
                author = db.Authors.Include(a=>a.Person).Where(a => a.Person.Name == name).FirstOrDefault();
                if (author == null)
                {
                    author = new Author();
                    Person p;
                    author.Id = Guid.NewGuid();
                    author.CreatedDate = DateTime.Now;
                    author.ModifiedDate = DateTime.Now;
                    if(!People.FindSavePerson(name, false, out p, out ex))
                    {
                        return false;
                    }
                    author.PersonId = p.Id;
                    
                    db.Authors.Add(author);
                    db.SaveChanges();
                    //author.Person = p;
                }
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<LibraryObject> GetAsObject(string q)
        {
            List<LibraryObject> objects = new List<LibraryObject>();
            List<Author> authors = db.Authors.Include(a => a.Person).Where(a => a.Person.Name.ToLower().Contains(q.ToLower())).OrderBy(a => a.Person.Name).ToList();
            foreach(Author author in authors)
            {
                objects.Add(new LibraryObject
                {
                    Id = author.Id,
                    Name = author.Person.Name,
                    Image = author.Person.DisplayImage,
                    Type = "Author"
                });
            }

            return objects;
        }
    }
}
