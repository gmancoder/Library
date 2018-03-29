using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web;

namespace BookLibrary.Functions
{
    public class People
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static CelebrityCentralService ccService = new CelebrityCentralService("grbrewer@gmail.com", "!Pass248word");
        public static bool FindSavePerson(string name, bool celebrityRequired, out Person person, out Exception ex, string imageUrl = null)
        {
            if (ccService.LoggedIn())
            {
                ex = null;
                bool edit = true;
                if (!PersonExistsByName(name, out person))
                {
                    person = new Person();
                    person.Id = Guid.NewGuid();
                    person.CreatedDate = DateTime.Now;
                    edit = false;
                }

                
                person.Name = name;
                person.SortName = Core.Core.ApplySortTitle(person.Name);
                person.ModifiedDate = DateTime.Now;
                List<Celebrity> celebrities = ccService.FindCelebrity(name);
                if (celebrities.Count() == 0 && celebrityRequired)
                {
                    ex = new KeyNotFoundException("Celebrity required and not found for '" + name + "'");
                    return false;
                }
                else if (celebrities.Count() > 0)
                {
                    Celebrity celebrity = celebrities[0];
                    person.CelebrityId = celebrity.Id;
                    Uri celebImage;
                    if (!Artists.GetCelebrityImage(celebrity.Id, out celebImage, out ex))
                    {
                        return false;
                    }
                    person.DisplayImage = celebImage.AbsoluteUri;
                }
                else
                {
                    if (imageUrl != null)
                    {
                        person.DisplayImage = imageUrl;
                    }
                    else if(!edit)
                    {
                        person.DisplayImage = "/Content/Images/unknown_author.png";
                    }
                }
                if (!edit)
                {
                    db.People.Add(person);
                }
                else
                {
                    db.Entry(person).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                return true;
            }
            ex = new Exception("Login failed for Celebrity Central");
            person = null;
            return false;
        }

        public static bool PersonExistsByName(string name, out Person person)
        {
            person = db.People.Where(p => p.Name == name).FirstOrDefault();
            return person != null;
        }

        public static bool PersonExistsById(Guid Id)
        {
            return db.People.Find(Id) != null;
        }

        public static Person PersonById(Guid Id)
        {
            return db.People.Find(Id);
        }

        public static bool GetCelebrity(Guid celebrityId, out Celebrity celebrity, out Exception ex)
        {
            ex = null;
            celebrity = null;
            if(ccService.LoggedIn())
            {
                List<Celebrity> celebrities = ccService.GetCelebrity(celebrityId);
                if(celebrities.Count > 0)
                {
                    celebrity = celebrities[0];
                    return true;
                }
                ex = new KeyNotFoundException("Celebrity not found");
                return false;
            }
            ex = new Exception("Celebrity Central login failure");
            return false;
        }

        public static bool GeneratePersonView(Guid objectId, string objectType, out PersonViewModel view, out Exception ex)
        {
            Guid personId = Guid.Empty;
            view = new PersonViewModel();
            ex = null;
            switch(objectType)
            {
                case "Artist":
                    Artist artist = db.Artists.Include(a => a.AssociatedWith).Where(a => a.Id == objectId).FirstOrDefault();
                    if(artist == null)
                    {
                        ex = new KeyNotFoundException("Artist not found");
                        return false;
                    }
                    else if(!artist.PersonId.HasValue)
                    {
                        ex = new Exception("Artist is not a Person");
                        return false;
                    }
                    personId = artist.PersonId.Value;
                    if (artist.AssociatedWith != null)
                    {
                        view.AssociatedWith = Artists.AsObject(artist.AssociatedWith);
                    }
                    break;
                case "Author":
                    Author author = db.Authors.Find(objectId);
                    if(author == null)
                    {
                        ex = new KeyNotFoundException("Author not found");
                        return false;
                    }
                    personId = author.PersonId;
                    break;
                case "MovieStar":
                    MovieStar movieStar = db.MovieStars.Find(objectId);
                    if(movieStar == null)
                    {
                        ex = new KeyNotFoundException("Movie Star not found");
                        return false;
                    }
                    personId = movieStar.PersonId;
                    break;
                case "TVStar":
                    TVStar tvStar = db.TVStars.Find(objectId);
                    if (tvStar == null)
                    {
                        ex = new KeyNotFoundException("Movie Star not found");
                        return false;
                    }
                    personId = tvStar.PersonId;
                    break;
                default:
                    ex = new Exception("Object of type " + objectType + " not allowed here");
                    return false;
            }
            Person person = People.PersonById(personId);
            if(person == null)
            {
                ex = new KeyNotFoundException("Person not found");
                return false;
            }

            view.Person = person;
            if(person.CelebrityId.HasValue)
            {
                Celebrity celebrity;
                if(!GetCelebrity(person.CelebrityId.Value, out celebrity, out ex))
                {
                    return false;
                }
                view.Details = celebrity.Details;
            }

            view.Books = Books.GetBooksForPerson(person.Id);
            view.Albums = Albums.GetAlbumsForPerson(person.Id);
            view.Tracks = Tracks.GetNonAlbumTracksForPerson(person.Id);
            view.Movies = Movies.GetMoviesForPerson(person.Id);
            view.TVShows = TVShows.GetTVShowsForPerson(person.Id);
            return true;
        }

        public static bool DrawLinkListForView(PersonViewModel viewModel, string objectType, ref List<PageLink> links)
        {
            if (viewModel.Books.Count() > 0)
            {
                links.Add(new PageLink("Books", "#books"));
            }
            if (viewModel.AssociatedWith != null)
            {
                links.Add(new PageLink("Associated With", "#associated"));
            }
            if (viewModel.Albums.Count() > 0)
            {
                links.Add(new PageLink("Albums", "#albums"));
            }
            if (viewModel.Tracks.Count() > 0)
            {
                links.Add(new PageLink("Tracks On Other Albums", "#tracks"));
            }
            if (viewModel.Movies.Count() > 0)
            {
                links.Add(new PageLink("Movies", "#movies"));
            }
            if (viewModel.TVShows.Count() > 0)
            {
                links.Add(new PageLink("TV Shows", "#tvshows"));
            }

            if (viewModel.Person.CelebrityId.HasValue)
            {
                Celebrity celebrity;
                Exception ex;
                if (!GetCelebrity(viewModel.Person.CelebrityId.Value, out celebrity, out ex))
                {
                    return false;
                }
                string url_add = "";
                if (HttpContext.Current.Request.UserHostAddress.StartsWith("192.168.1."))
                {
                    url_add = ":8081";
                }
                links.Add(new PageLink(objectType + " Details", "http://celebritycentral.gmancoder.com" + url_add + "/Celebrities/Details/" + celebrity.Id, "_blank"));
            }

            return true;
        }
    }
}
