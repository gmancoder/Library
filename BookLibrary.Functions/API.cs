using BookLibrary.Models;
using BookLibrary.Models.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data.Entity;
using BookLibrary.Models.API;
using System.Configuration;
using BookLibrary.Services;

namespace BookLibrary.Functions
{
    public class API
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static bool Where(List<RequestFilter> where, string whereOp, out string condition, out List<object> values, out Exception ex)
        {
            if(String.IsNullOrWhiteSpace(whereOp))
            {
                whereOp = "AND";
            }
            condition = "";
            int idx = 0;
            values = new List<object>();
            try
            {
                foreach (RequestFilter filter in where)
                {
                    if (condition != "")
                    {
                        condition += " " + whereOp + " ";
                    }
                    switch (filter.@operator.ToLower())
                    {
                        case "in":
                        case "not in":
                            condition += String.Format("{0} {1} (", filter.field, filter.@operator);
                            foreach(object value in filter.values)
                            {
                                condition += "@value" + idx;
                                idx += 1;
                                values.Add(value);
                            }
                            condition += ")";
                            break;

                        case "like":
                        case "not like":
                            condition += String.Format("{0} {1} Concat('%',@value{2},'%')", filter.field, filter.@operator, idx);
                            values.Add(filter.values[0]);
                            idx++;
                            break;

                        case "starts with":
                            condition += String.Format("{0} like Concat(@value{1},'%')", filter.field, idx);
                            values.Add(filter.values[0]);
                            idx++;
                            break;

                        case "ends with":
                            condition += String.Format("{0} like Concat('%',@value{1})", filter.field, idx);
                            values.Add(filter.values[0]);
                            idx++;
                            break;

                        case "between":
                        case "not between":
                            condition += String.Format("{0} {1} between @value{2} and @value{3}", filter.field, filter.@operator, idx, idx + 1);
                            values.Add(filter.values[0]);
                            values.Add(filter.values[1]);
                            idx += 2;
                            break;

                        case "is not null":
                        case "not null":
                        case "not empty":
                            condition += String.Format("{0} is not null", filter.field);
                            break;

                        case "is empty":
                        case "is null":
                            condition += String.Format("{0} is null", filter.field);
                            break;
                        case "anniversary of":
                            DateTime inDate;
                            if(!DateTime.TryParse(filter.values[0].ToString(), out inDate))
                            {
                                ex = new Exception(filter.values[0] + " not a date");
                                return false;
                            }
                            condition += String.Format("DATEPART(mm, {0}) = {1} AND DATEPART(dd, {0}) = {2}", filter.field, inDate.Month, inDate.Day);
                            break;
                        default:
                            condition += String.Format("{0} {1} @value{2}", filter.field, filter.@operator, idx);
                            values.Add(filter.values[0]);
                            idx += 1;
                            break;
                    }
                }

                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        public static bool Order(List<string> order, string orderDirection, out string orders, out Exception ex)
        {
            orders = "";
            if (String.IsNullOrWhiteSpace(orderDirection))
            {
                orderDirection = "asc";
            }
            try
            {
                foreach (string o in order)
                {
                    if(orders != "")
                    {
                        orders += ", ";
                    }
                    orders += o + " " + orderDirection;
                }
                ex = null;
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<SqlParameter> ValuesToParameters(List<object> values)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            int idx = 0;
            foreach(object val in values)
            {
                string label = "@value" + idx;
                parameters.Add(new SqlParameter(label, val));
                idx += 1;
            }

            return parameters;
        }

        public static List<T> SqlQuery<T>(string sql, List<SqlParameter> parameters)
        {
            return db.Database.SqlQuery<T>(sql, parameters.ToArray()).ToList();
        }

        public static bool Create<T>(CreateRequest request, out T entity, out Exception ex)
        {
            db.Configuration.LazyLoadingEnabled = false;
            entity = default(T);
            ex = null;
            try
            {
                switch (request.Entity.ToLower())
                {
                    case "author":
                        Author author = new Author();
                        author.Id = Guid.NewGuid();
                        author.CreatedDate = DateTime.Now;
                        author.ModifiedDate = DateTime.Now;
                        if (!DictToAuthor(ref author, request.Data, out ex))
                        {
                            return false;
                        }
                        db.Authors.Add(author);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(author, typeof(T));
                        break;
                    case "book":
                        bool amazonBook;
                        Book book = new Book();
                        book.Id = Guid.NewGuid();
                        book.CreatedDate = DateTime.Now;
                        book.ModifiedDate = DateTime.Now;
                        if(!DictToBook(ref book, request.Data, out ex, out amazonBook))
                        {
                            return false;
                        }
                        if(!amazonBook)
                        {
                            db.Books.Add(book);
                            db.SaveChanges();
                            if (!Books.PopulateBookAuthors(book, out ex))
                            {
                                return false;
                            }
                        }
                        entity = (T)Convert.ChangeType(book, typeof(T));
                        break;
                    case "movie":
                        bool amazonMovie;
                        Movie movie = new Movie();
                        movie.Id = Guid.NewGuid();
                        movie.CreatedDate = DateTime.Now;
                        movie.ModifiedDate = DateTime.Now;
                        if (!DictToMovie(ref movie, request.Data, out ex, out amazonMovie))
                        {
                            return false;
                        }
                        if (!amazonMovie)
                        {
                            db.Movies.Add(movie);
                            db.SaveChanges();
                            if(!Movies.PopulateMovieStars(movie, out ex))
                            {
                                return false;
                            }
                        }
                        entity = (T)Convert.ChangeType(movie, typeof(T));
                        break;
                    case "moviestar":
                        MovieStar movieStar = new MovieStar();
                        movieStar.Id = Guid.NewGuid();
                        movieStar.CreatedDate = DateTime.Now;
                        movieStar.ModifiedDate = DateTime.Now;
                        
                        if (!DictToMovieStar(ref movieStar, request.Data, out ex))
                        {
                            return false;
                        }
                        db.MovieStars.Add(movieStar);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(movieStar, typeof(T));
                        break;
                    case "movietomoviestar":
                        MovieToMovieStar movieToStar = new MovieToMovieStar();
                        movieToStar.Id = Guid.NewGuid();
                        if(!MovieToStarToDict(ref movieToStar, request.Data, out ex))
                        {
                            return false;
                        }
                        db.MovieToMovieStars.Add(movieToStar);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(movieToStar, typeof(T));
                        break;
                    case "tvshow":
                        TVShow tvShow = new TVShow();
                        tvShow.Id = Guid.NewGuid();
                        tvShow.CreatedDate = DateTime.Now;
                        tvShow.ModifiedDate = DateTime.Now;

                        if (!DictToTVShow(ref tvShow, request.Data, out ex))
                        {
                            return false;
                        }
                        db.TVShows.Add(tvShow);
                        db.SaveChanges();
                        if(!TVShows.PopulateTVStars(tvShow, out ex))
                        {
                            return false;
                        }
                        entity = (T)Convert.ChangeType(tvShow, typeof(T));
                        break;
                    case "tvstar":
                        TVStar tvStar = new TVStar();
                        tvStar.Id = Guid.NewGuid();
                        tvStar.CreatedDate = DateTime.Now;
                        tvStar.ModifiedDate = DateTime.Now;

                        if (!DictToTVStar(ref tvStar, request.Data, out ex))
                        {
                            return false;
                        }
                        db.TVStars.Add(tvStar);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(tvStar, typeof(T));
                        break;
                    case "tvshowtotvstar":
                        TVShowToTVStar tvToStar = new TVShowToTVStar();
                        tvToStar.Id = Guid.NewGuid();
                        if (!TVShowToStarToDict(ref tvToStar, request.Data, out ex))
                        {
                            return false;
                        }
                        db.TVShowToTVStars.Add(tvToStar);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(tvToStar, typeof(T));
                        break;
                    case "album":
                        bool amazonAlbum;
                        Album album = new Album();
                        album.Id = Guid.NewGuid();
                        album.CreatedDate = DateTime.Now;
                        album.ModifiedDate = DateTime.Now;
                        if (!DictToAlbum(ref album, request.Data, out ex, out amazonAlbum))
                        {
                            return false;
                        }
                        if (!amazonAlbum)
                        {
                            db.Albums.Add(album);
                            db.SaveChanges();
                        }
                        entity = (T)Convert.ChangeType(album, typeof(T));
                        break;
                    case "artist":
                        Artist artist = new Artist();
                        artist.Id = Guid.NewGuid();
                        artist.CreatedDate = DateTime.Now;
                        artist.ModifiedDate = DateTime.Now;
                        if (!DictToArtist(ref artist, request.Data, out ex))
                        {
                            return false;
                        }
                        db.Artists.Add(artist);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(artist, typeof(T));
                        break;
                    case "track":
                        Track Track = new Track();
                        Track.Id = Guid.NewGuid();
                        Track.CreatedDate = DateTime.Now;
                        Track.ModifiedDate = DateTime.Now;
                        if (!DictToTrack(ref Track, request.Data, out ex))
                        {
                            return false;
                        }
                        db.Tracks.Add(Track);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(Track, typeof(T));
                        break;
                    case "category":
                        Category Category = new Category();
                        Category.Id = Guid.NewGuid();
                        Category.CreatedDate = DateTime.Now;
                        Category.ModifiedDate = DateTime.Now;
                        if (!DictToCategory(ref Category, request.Data, out ex))
                        {
                            return false;
                        }
                        db.Categories.Add(Category);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(Category, typeof(T));
                        break;
                    
                    case "objecttocategory":
                        ObjectToCategory ObjectToCategory = new ObjectToCategory();
                        //ObjectToCategory.Id = Guid.NewGuid();
                        if (!DictToObjectToCategory(ref ObjectToCategory, request.Data, out ex))
                        {
                            return false;
                        }
                        db.ObjectToCategories.Add(ObjectToCategory);
                        db.SaveChanges();
                        entity = (T)Convert.ChangeType(ObjectToCategory, typeof(T));
                        break;
                    case "magazine":
                        Magazine Magazine = new Magazine();
                        Magazine.Id = Guid.NewGuid();
                        Magazine.CreatedDate = DateTime.Now;
                        Magazine.ModifiedDate = DateTime.Now;
                        Magazine.PdfCategoryFolderId = -1;
                        if (!DictToMagazine(ref Magazine, request.Data, out ex))
                        {
                            return false;
                        }
                        db.Magazines.Add(Magazine);
                        db.SaveChanges();

                        if(Magazine.PdfCategoryFolderId != -1)
                        {
                            if(!MagazineIssues.SyncIssues(Magazine, out ex))
                            {
                                return false;
                            }
                        }

                        entity = (T)Convert.ChangeType(Magazine, typeof(T));
                        break;
                }
                return true;
            }
            catch(Exception e)
            {
                ex = e;
                return false;
            }
        }

        

        public static bool Update<T>(UpdateRequest request, out T entity, out Exception ex) where T: class
        {
            db.Configuration.LazyLoadingEnabled = false;
            entity = default(T);
            ex = null;
            try
            {
                switch (request.Entity.ToLower())
                {
                    case "album":
                        bool amazonAlbum;
                        Album album = db.Albums.Find(new Guid(request.Id.ToString()));
                        if(album == null)
                        {
                            ex = new Exception("Album not found");
                            return false;
                        }
                        if(DictToAlbum(ref album, request.Data, out ex, out amazonAlbum, true))
                        {
                            album.ModifiedDate = DateTime.Now;
                            album.SortTitle = Core.Core.ApplySortTitle(album.Title);
                            db.Entry(album).State = EntityState.Modified;
                            db.SaveChanges();
                            if (album is T)
                            {
                                entity = album as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Album");
                            return false;
                        }
                        return false;
                    case "author":
                        Author author = db.Authors.Find(new Guid(request.Id.ToString()));
                        if (author == null)
                        {
                            ex = new Exception("Author not found");
                            return false;
                        }
                        if (DictToAuthor(ref author, request.Data, out ex, true))
                        {
                            author.ModifiedDate = DateTime.Now;
                            db.Entry(author).State = EntityState.Modified;
                            db.SaveChanges();
                            if (author is T)
                            {
                                entity = author as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Author");
                            return false;
                        }
                        return false;
                    case "book":
                        bool amazonBook;
                        Book book = db.Books.Find(new Guid(request.Id.ToString()));
                        if (book == null)
                        {
                            ex = new Exception("Book not found");
                            return false;
                        }
                        if (DictToBook(ref book, request.Data, out ex, out amazonBook, true))
                        {
                            book.ModifiedDate = DateTime.Now;
                            book.SortTitle = Core.Core.ApplySortTitle(book.Title);
                            db.Entry(book).State = EntityState.Modified;
                            db.SaveChanges();
                            if (!amazonBook)
                            {
                                db.Database.ExecuteSqlCommand("delete from BookAuthors where BookId = '" + book.Id + "'");
                                if (!Books.PopulateBookAuthors(book, out ex))
                                {
                                    return false;
                                }
                            }
                            if (book is T)
                            {
                                entity = book as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Book");
                            return false;
                        }
                        return false;
                    case "artist":
                        Artist artist = db.Artists.Find(new Guid(request.Id.ToString()));
                        if (artist == null)
                        {
                            ex = new Exception("Artist not found");
                            return false;
                        }
                        if (DictToArtist(ref artist, request.Data, out ex, true))
                        {
                            artist.ModifiedDate = DateTime.Now;
                            db.Entry(artist).State = EntityState.Modified;
                            db.SaveChanges();
                            if (artist is T)
                            {
                                entity = artist as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Artist");
                            return false;
                        }
                        return false;
                    case "track":
                        Track track = db.Tracks.Find(new Guid(request.Id.ToString()));
                        if (track == null)
                        {
                            ex = new Exception("Track not found");
                            return false;
                        }
                        if (DictToTrack(ref track, request.Data, out ex, true))
                        {
                            track.ModifiedDate = DateTime.Now;
                            db.Entry(track).State = EntityState.Modified;
                            db.SaveChanges();
                            if (track is T)
                            {
                                entity = track as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Track");
                            return false;
                        }
                        return false;
                    case "category":
                        Category category = db.Categories.Find(new Guid(request.Id.ToString()));
                        if (category == null)
                        {
                            ex = new Exception("Category not found");
                            return false;
                        }
                        if (DictToCategory(ref category, request.Data, out ex, true))
                        {
                            category.ModifiedDate = DateTime.Now;
                            db.Entry(category).State = EntityState.Modified;
                            db.SaveChanges();
                            if (category is T)
                            {
                                entity = category as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Category");
                            return false;
                        }
                        return false;
                    case "objecttocategory":
                        ObjectToCategory objectCategory = db.ObjectToCategories.Find(Convert.ToInt32(request.Id));
                        if (objectCategory == null)
                        {
                            ex = new Exception("AlbumCategory not found");
                            return false;
                        }
                        if (DictToObjectToCategory(ref objectCategory, request.Data, out ex, true))
                        {
                            db.Entry(objectCategory).State = EntityState.Modified;
                            db.SaveChanges();
                            if (objectCategory is T)
                            {
                                entity = objectCategory as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Object Category");
                            return false;
                        }
                        return false;
                    case "movie":
                        bool amazonMovie;
                        Movie movie = db.Movies.Find(new Guid(request.Id.ToString()));
                        if (movie == null)
                        {
                            ex = new Exception("Movie not found");
                            return false;
                        }
                        if (DictToMovie(ref movie, request.Data, out ex, out amazonMovie, true))
                        {
                            movie.ModifiedDate = DateTime.Now;
                            movie.SortTitle = Core.Core.ApplySortTitle(movie.Title);
                            db.Entry(movie).State = EntityState.Modified;
                            db.SaveChanges();
                            if (!amazonMovie)
                            {
                                db.Database.ExecuteSqlCommand("delete from MovieToMovieStars where MovieId = '" + movie.Id + "' and ManuallyAdded = 0");
                                if (!Movies.PopulateMovieStars(movie, out ex))
                                {
                                    return false;
                                }
                            }
                            if (movie is T)
                            {
                                entity = movie as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Movie");
                            return false;
                        }
                        return false;
                    case "moviestar":
                        MovieStar moviestar = db.MovieStars.Find(new Guid(request.Id.ToString()));
                        if (moviestar == null)
                        {
                            ex = new Exception("MovieStar not found");
                            return false;
                        }
                        if (DictToMovieStar(ref moviestar, request.Data, out ex, true))
                        {
                            moviestar.ModifiedDate = DateTime.Now;
                            db.Entry(moviestar).State = EntityState.Modified;
                            db.SaveChanges();
                            if (moviestar is T)
                            {
                                entity = moviestar as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Movie Star");
                            return false;
                        }
                        return false;
                    case "tvshow":
                        TVShow tvShow = db.TVShows.Find(new Guid(request.Id.ToString()));
                        if(tvShow == null)
                        {
                            ex = new Exception("TV Show not found");
                            return false;
                        }
                        if(DictToTVShow(ref tvShow, request.Data, out ex, true))
                        {
                            tvShow.ModifiedDate = DateTime.Now;
                            db.Entry(tvShow).State = EntityState.Modified;
                            db.SaveChanges();
                            db.Database.ExecuteSqlCommand("delete from TVShowToTVStars where MovieId = '" + tvShow.Id + "' and ManuallyAdded = 0");
                            if (!TVShows.PopulateTVStars(tvShow, out ex))
                            {
                                return false;
                            }
                            if (tvShow is T)
                            {
                                entity = tvShow as T;
                                return true;
                            }
                            ex = new Exception("Unable to return TV Show");
                            return false;
                        }
                        return false;
                    case "tvstar":
                        TVStar tvstar = db.TVStars.Find(new Guid(request.Id.ToString()));
                        if (tvstar == null)
                        {
                            ex = new Exception("TV Star not found");
                            return false;
                        }
                        if (DictToTVStar(ref tvstar, request.Data, out ex, true))
                        {
                            tvstar.ModifiedDate = DateTime.Now;
                            db.Entry(tvstar).State = EntityState.Modified;
                            db.SaveChanges();
                            if (tvstar is T)
                            {
                                entity = tvstar as T;
                                return true;
                            }
                            ex = new Exception("Unable to return TV Star");
                            return false;
                        }
                        return false;
                    case "magazine":
                        Magazine magazine = db.Magazines.Find(new Guid(request.Id.ToString()));
                        if (magazine == null)
                        {
                            ex = new Exception("Magazine not found");
                            return false;
                        }
                        if (DictToMagazine(ref magazine, request.Data, out ex, true))
                        {
                            magazine.ModifiedDate = DateTime.Now;
                            db.Entry(magazine).State = EntityState.Modified;
                            db.SaveChanges();
                            if (magazine.PdfCategoryFolderId != -1)
                            {
                                if (!MagazineIssues.SyncIssues(magazine, out ex))
                                {
                                    return false;
                                }
                            }
                            if (magazine is T)
                            {
                                entity = magazine as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Movie Star");
                            return false;
                        }
                        return false;
                    case "magazineissue":
                        MagazineIssue magazineissue = db.MagazineIssues.Find(new Guid(request.Id.ToString()));
                        if (magazineissue == null)
                        {
                            ex = new Exception("Magazine Issue not found");
                            return false;
                        }
                        if (DictToMagazineIssue(ref magazineissue, request.Data, out ex, true))
                        {
                            magazineissue.ModifiedDate = DateTime.Now;
                            db.Entry(magazineissue).State = EntityState.Modified;
                            db.SaveChanges();
                            if (magazineissue is T)
                            {
                                entity = magazineissue as T;
                                return true;
                            }
                            ex = new Exception("Unable to return Magazine Issue");
                            return false;
                        }
                        return false;
                    default:
                        ex = new Exception("Entity " + request.Entity + " not valid");
                        return false;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        

        public static bool Delete<T>(DeleteRequest request, out T entity, out Exception ex) where T: class
        {
            db.Configuration.LazyLoadingEnabled = false;
            entity = default(T);
            ex = null;
            try
            {
                switch (request.Entity.ToLower())
                {
                    case "album":
                        Album album = db.Albums.Find(new Guid(request.Id.ToString()));
                        if(album == null)
                        {
                            ex = new Exception("Album not found");
                            return false;
                        }
                        Categories.Cleanup("Album", album.Id);
                        db.Albums.Remove(album);
                        db.SaveChanges();
                        entity = album as T;
                        return true;
                    case "artist":
                        Artist artist = db.Artists.Find(new Guid(request.Id.ToString()));
                        if (artist == null)
                        {
                            ex = new Exception("Artist not found");
                            return false;
                        }
                        db.Artists.Remove(artist);
                        db.SaveChanges();
                        entity = artist as T;
                        return true;
                    case "track":
                        Track track = db.Tracks.Find(new Guid(request.Id.ToString()));
                        if (track == null)
                        {
                            ex = new Exception("Track not found");
                            return false;
                        }
                        db.Tracks.Remove(track);
                        db.SaveChanges();
                        entity = track as T;
                        return true;
                    case "category":
                        Category category = db.Categories.Find(new Guid(request.Id.ToString()));
                        if (category == null)
                        {
                            ex = new Exception("Category not found");
                            return false;
                        }
                        db.Categories.Remove(category);
                        db.SaveChanges();
                        entity = category as T;
                        return true;
                    case "objecttocategory":
                        ObjectToCategory objectCategory = db.ObjectToCategories.Find(Convert.ToInt32(request.Id));
                        if (objectCategory == null)
                        {
                            ex = new Exception("Object Category not found");
                            return false;
                        }
                        db.ObjectToCategories.Remove(objectCategory);
                        db.SaveChanges();
                        entity = objectCategory as T;
                        return true;
                    case "author":
                        Author author = db.Authors.Find(new Guid(request.Id.ToString()));
                        if (author == null)
                        {
                            ex = new Exception("Author not found");
                            return false;
                        }
                        db.Authors.Remove(author);
                        db.SaveChanges();
                        entity = author as T;
                        return true;
                    case "book":
                        Book book = db.Books.Find(new Guid(request.Id.ToString()));
                        if (book == null)
                        {
                            ex = new Exception("Book not found");
                            return false;
                        }
                        Categories.Cleanup("Book", book.Id);
                        db.Books.Remove(book);
                        db.SaveChanges();
                        entity = book as T;
                        return true;
                    case "magazine":
                        Magazine magazine = db.Magazines.Find(new Guid(request.Id.ToString()));
                        if (magazine == null)
                        {
                            ex = new Exception("Magazine not found");
                            return false;
                        }
                        Categories.Cleanup("Magazine", magazine.Id);
                        db.Magazines.Remove(magazine);
                        db.SaveChanges();
                        entity = magazine as T;
                        return true;
                    case "movie":
                        Movie movie = db.Movies.Find(new Guid(request.Id.ToString()));
                        if (movie == null)
                        {
                            ex = new Exception("Movie not found");
                            return false;
                        }
                        Categories.Cleanup("Movie", movie.Id);
                        db.Movies.Remove(movie);
                        db.SaveChanges();
                        entity = movie as T;
                        return true;
                    case "moviestar":
                        MovieStar moviestar = db.MovieStars.Find(request.Id);
                        if (moviestar == null)
                        {
                            ex = new Exception("Movie Star not found");
                            return false;
                        }
                        db.MovieStars.Remove(moviestar);
                        db.SaveChanges();
                        entity = moviestar as T;
                        return true;
                    case "tvshow":
                        TVShow tvshow = db.TVShows.Find(new Guid(request.Id.ToString()));
                        if (tvshow == null)
                        {
                            ex = new Exception("TV Show not found");
                            return false;
                        }
                        db.TVShows.Remove(tvshow);
                        db.SaveChanges();
                        entity = tvshow as T;
                        return true;
                    case "tvstar":
                        TVStar tvstar = db.TVStars.Find(request.Id);
                        if (tvstar == null)
                        {
                            ex = new Exception("TV Star not found");
                            return false;
                        }
                        db.TVStars.Remove(tvstar);
                        db.SaveChanges();
                        entity = tvstar as T;
                        return true;
                    default:
                        ex = new Exception("Entity " + request.Entity + " not valid");
                        return false;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        private static bool CheckFields(string entity, List<string> keys, out string badField, out Exception ex, bool update = false)
        {
            List<string> fields = new List<string>();
            List<string> requiredFields = new List<string>();
            badField = "";
            ex = null;
            try
            {
                switch(entity.ToLower())
                {
                    case "album":
                        fields = Albums.Fields;
                        requiredFields = Albums.RequiredFields;
                        break;
                    case "artist":
                        fields = Artists.Fields;
                        requiredFields = Artists.RequiredFields;
                        break;
                    case "track":
                        fields = Tracks.Fields;
                        requiredFields = Tracks.RequiredFields;
                        break;
                    case "category":
                        fields = Categories.CategoryFields;
                        requiredFields = Categories.CategoryRequiredFields;
                        break;
                    case "objecttocategory":
                        fields = Categories.ObjectToCategoryFields;
                        requiredFields = Categories.ObjectToCategoryRequiredFields;
                        break;
                    case "author":
                        fields = Authors.Fields;
                        requiredFields = Authors.RequiredFields;
                        break;
                    case "book":
                        fields = Books.Fields;
                        requiredFields = Books.RequiredFields;
                        break;
                    case "movie":
                        fields = Movies.Fields;
                        requiredFields = Movies.RequiredFields;
                        break;
                    case "moviestar":
                        fields = MovieStars.Fields;
                        requiredFields = MovieStars.RequiredFields;
                        break;
                    case "movietomoviestar":
                        fields = MovieStars.MovieToStarFields;
                        requiredFields = MovieStars.MovieToStarRequiredFields;
                        break;
                    case "tvshow":
                        fields = TVShows.Fields;
                        requiredFields = TVShows.RequiredFields;
                        break;
                    case "tvshowstar":
                        fields = TVStars.Fields;
                        requiredFields = TVStars.RequiredFields;
                        break;
                    case "tvshowtotvshowstar":
                        fields = TVStars.TVShowToStarFields;
                        requiredFields = TVStars.TVShowToStarRequiredFields;
                        break;
                    case "magazine":
                        fields = Magazines.Fields;
                        requiredFields = Magazines.RequiredFields;
                        break;
                    case "magazineissue":
                        fields = MagazineIssues.Fields;
                        requiredFields = MagazineIssues.RequiredFields;
                        break;

                }
                if(fields.Count() == 0)
                {
                    ex = new Exception("Fields not found");
                    return false;
                }
                foreach (string key in keys)
                {
                    bool found = false;
                    foreach (string field in fields)
                    {
                        if(field.ToLower() == key.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found && key != "autocelebrity" && key != "autoperson")
                    {
                        ex = new Exception("Field " + key + " does not exist");
                        return false;
                    }
                }

                if (!update)
                {
                    foreach (string field in requiredFields)
                    {
                        bool found = false;
                        foreach (string key in keys)
                        {
                            if (field.ToLower() == key.ToLower())
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            ex = new Exception(field + " is a required field");
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
            
            return true;
        }

        private static bool DictToAlbum(ref Album album, Dictionary<string, object> data, out Exception ex, out bool amazonAlbum, bool edit = false)
        {
            ex = null;
            string badField = "";
            amazonAlbum = false;
            if(!CheckFields("album", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach(KeyValuePair<string,object> attrib in data)
            {
                if(attrib.Key.ToLower() == "entrytype" && attrib.Value.ToString().ToLower() == "amazon")
                {
                    amazonAlbum = true;
                    if(!data.ContainsKey("ASIN") && !data.ContainsKey("asin"))
                    {
                        ex = new Exception("ASIN required for Amazon Album");
                        return false;
                    }
                    album.EntryType = "Amazon";
                }
                else
                {
                    //album.EntryType = "Manual";
                    switch(attrib.Key.ToLower())
                    {
                        case "artistid":
                            album.ArtistId = new Guid(attrib.Value.ToString());
                            break;
                        case "numberofdiscs":
                            album.NumberOfDiscs = Convert.ToInt32(attrib.Value);
                            break;
                        case "asin":
                            album.ASIN = attrib.Value.ToString();
                            break;
                        case "ean":
                            album.EAN = attrib.Value.ToString();
                            break;
                        case "upc":
                            album.UPC = attrib.Value.ToString();
                            break;
                        case "title":
                            album.Title = attrib.Value.ToString();
                            album.SortTitle = Core.Core.ApplySortTitle(album.Title);
                            break;
                        case "releasedate":
                            album.ReleaseDate = Convert.ToDateTime(attrib.Value).ToShortDateString();
                            break;
                        case "binding":
                            album.Binding = attrib.Value.ToString();
                            break;
                        case "imagefilename":
                            album.ImageFileName = attrib.Value.ToString();
                            break;
                        case "createddate":
                            album.CreatedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "modifieddate":
                            album.ModifiedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "entrytype":
                            album.EntryType = attrib.Value.ToString();
                            break;
                        case "url":
                            album.Url = attrib.Value.ToString();
                            break;
                        case "amazonresponse":
                            album.AmazonResponse = attrib.Value.ToString();
                            break;
                    }
                }
            }

            if(amazonAlbum)
            {
                if(!edit && Albums.AmazonAlbumExists(album.ASIN))
                {
                    ex = new Exception("Album with ASIN " + album.ASIN + " already exists");
                    return false;
                }
                bool noDiscs;
                if(!Albums.AmazonAlbum(ref album,out noDiscs,out ex, edit: edit ))
                {
                    return false;
                }
            }
            else if(!edit && Albums.ManualAlbumExists(album.Title, album.ArtistId))
            {
                ex = new Exception("Album " + album.Title + " already exists for the selected artist");
                return false;
            }

            return true;
        }


        private static bool DictToArtist(ref Artist artist, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            bool autoPerson = false;
            string name = "";
            string displayImage = null;
            bool isGroup = false;
            if (!CheckFields("artist", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "autoperson":
                        autoPerson = Convert.ToBoolean(attrib.Value);
                        if (data.Keys.Contains("Name"))
                        {
                            name = data["Name"].ToString();
                        }
                        else if (data.Keys.Contains("name"))
                        {
                            name = data["name"].ToString();
                        }
                        else if (data.Keys.Contains("NAME"))
                        {
                            name = data["NAME"].ToString();
                        }
                        else
                        {
                            ex = new KeyNotFoundException("automatic person generation requires name");
                            return false;
                        }
                        break;
                    case "personid":
                        artist.PersonId = new Guid(attrib.Value.ToString());
                        break;
                    case "name":
                        name = attrib.Value.ToString();
                        break;
                    case "isgroup":
                        artist.IsGroup = Convert.ToBoolean(attrib.Value);
                        isGroup = artist.IsGroup;
                        break;
                    case "createddate":
                        artist.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        artist.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "displayimage":
                        displayImage = attrib.Value.ToString();
                        break;
                }
            }

            if (autoPerson)
            {
                if(artist.IsGroup)
                {
                    ex = new Exception("AutoPerson and isGroup cannot both be true");
                    return false;
                }
                if (!edit && Artists.ArtistExists(name))
                {
                    ex = new Exception("Artist " + name + " already exists");
                    return false;
                }

                Person person;
                if (!People.FindSavePerson(name, false, out person, out ex, displayImage))
                {
                    return false;
                }
                artist.PersonId = person.Id;
            }
            else if (artist.PersonId != null)
            {
                if (artist.IsGroup)
                {
                    ex = new Exception("IsGroup cannot be true when PersonId is present");
                    return false;
                }
                if (!People.PersonExistsById(artist.PersonId.Value))
                {
                    ex = new KeyNotFoundException("Person ID not found");
                    return false;
                }
            }
            else if(artist.IsGroup)
            {
                artist.Name = name;
                artist.SortName = Core.Core.ApplySortTitle(name);
                artist.DisplayImage = displayImage;
            }
            else
            {
                ex = new Exception("AutoPerson must be true, isGroup must be true, or PersonId included in the request");
                return false;
            }

            return true;
        }

        private static bool DictToTrack(ref Track track, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("track", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "name":
                        track.Name = attrib.Value.ToString();
                        break;
                    case "createddate":
                        track.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        track.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "lyrics":
                        track.Lyrics = attrib.Value.ToString();
                        break;
                    case "artistid":
                        track.ArtistId = new Guid(attrib.Value.ToString());
                        break;
                    case "albumid":
                        track.AlbumId = new Guid(attrib.Value.ToString());
                        break;
                    case "discnumber":
                        track.DiscNumber = Convert.ToInt32(attrib.Value);
                        break;
                    case "tracknumber":
                        track.TrackNumber = Convert.ToInt32(attrib.Value);
                        break;
                }
            }

            if(!edit && Tracks.TrackExists(track.Name, track.AlbumId, track.TrackNumber, track.DiscNumber))
            {
                ex = new Exception("Track " + track.Name + " already exists for the selected Album");
                return false;
            }
            return true;
        }

        private static bool DictToObjectToCategory(ref ObjectToCategory objectCategory, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("objecttocategory", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "objectid":
                        objectCategory.ObjectId = new Guid(attrib.Value.ToString());
                        break;
                    case "objecttype":
                        objectCategory.ObjectType = attrib.Value.ToString();
                        break;
                    case "categoryid":
                        objectCategory.CategoryId = new Guid(attrib.Value.ToString());
                        break;
                    case "browsenodeid":
                        objectCategory.BrowseNodeId = Convert.ToInt64(attrib.Value);
                        break;
                }
            }
            return true;
        }

        private static bool DictToCategory(ref Category category, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("category", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "name":
                        category.Name = attrib.Value.ToString();
                        break;
                    case "createddate":
                        category.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        category.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "categoryid":
                        category.CategoryId = new Guid(attrib.Value.ToString());
                        break;
                    
                }
            }

            if(!edit && Categories.CategoryExists(category.Name, category.CategoryId))
            {
                ex = new Exception("Category " + category.Name + " already exists");
                return false;
            }
            return true;
        }

        private static bool DictToAuthor(ref Author author, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            bool autoPerson = false;
            string name = "";
            string displayImage = null;
            if (!CheckFields("author", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "autoperson":
                        autoPerson = Convert.ToBoolean(attrib.Value);
                        if(data.Keys.Contains("Name"))
                        {
                            name = data["Name"].ToString();
                        }
                        else if(data.Keys.Contains("name"))
                        {
                            name = data["name"].ToString();
                        }
                        else if (data.Keys.Contains("NAME"))
                        {
                            name = data["NAME"].ToString();
                        }
                        else
                        {
                            ex = new KeyNotFoundException("automatic person generation requires name");
                            return false;
                        }
                        break;
                    case "personid":
                        author.PersonId = new Guid(attrib.Value.ToString());
                        break;
                    case "createddate":
                        author.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        author.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "displayimage":
                        displayImage = attrib.Value.ToString();
                        break;
                }
            }

            if (autoPerson)
            {
                if (!edit && Authors.AuthorExists(name))
                {
                    ex = new Exception("Author " + name + " already exists");
                    return false;
                }

                Person person;
                if(!People.FindSavePerson(name, false, out person, out ex, displayImage))
                {
                    return false;
                }
                author.PersonId = person.Id;
            }
            else if(author.PersonId != null)
            {
                if(!People.PersonExistsById(author.PersonId))
                {
                    ex = new KeyNotFoundException("Person ID not found");
                    return false;
                }
            }
            else
            {
                ex = new Exception("Either AutoPerson must be set or PersonId included in the request");
                return false;
            }

                

                return true;
        }

        private static bool DictToBook(ref Book book, Dictionary<string, object> data, out Exception ex, out bool amazonBook, bool edit = false)
        {
            ex = null;
            string badField = "";
            amazonBook = false;
            if (!CheckFields("book", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                if (attrib.Key.ToLower() == "entrytype" && attrib.Value.ToString().ToLower() == "amazon")
                {
                    amazonBook = true;
                    if (!data.ContainsKey("ISBN") && !data.ContainsKey("isbn"))
                    {
                        ex = new Exception("ISBN required for Amazon Book");
                        return false;
                    }
                    book.EntryType = "Amazon";
                }
                else
                {
                    switch (attrib.Key.ToLower())
                    {
                        case "authors":
                            book.Authors = attrib.Value.ToString();
                            break;
                        case "asin":
                            book.ASIN = attrib.Value.ToString();
                            break;
                        case "isbn":
                            book.ISBN = attrib.Value.ToString();
                            break;
                        case "title":
                            book.Title = attrib.Value.ToString();
                            book.SortTitle = Core.Core.ApplySortTitle(book.Title);
                            break;
                        case "releasedate":
                            book.ReleaseDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "imagefilename":
                            book.ImageFileName = attrib.Value.ToString();
                            break;
                        case "createddate":
                            book.CreatedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "modifieddate":
                            book.ModifiedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "entrytype":
                            book.EntryType = attrib.Value.ToString();
                            break;
                        case "detailpageurl":
                            book.DetailPageUrl = attrib.Value.ToString();
                            break;
                        case "amazonresponse":
                            book.AmazonResponse = attrib.Value.ToString();
                            break;
                        case "manufacturer":
                            book.Manufacturer = attrib.Value.ToString();
                            break;
                        case "amazonproductgroup":
                            book.AmazonProductGroup = attrib.Value.ToString();
                            break;
                        case "reading":
                            book.Reading = Convert.ToBoolean(attrib.Value);
                            break;
                        case "pdfid":
                            book.PdfId = Convert.ToInt32(attrib.Value);
                            break;
                        case "publicationdate":
                            book.PublicationDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "publisher":
                            book.Publisher = attrib.Value.ToString();
                            break;
                    }
                }
            }

            if (amazonBook)
            {
                if (!edit && Books.AmazonBookExists(book.ISBN))
                {
                    ex = new Exception("Book with ISBN " + book.ISBN + " already exists");
                    return false;
                }
                bool noDiscs;
                if (!Books.AmazonBook(ref book, out ex, edit))
                {
                    return false;
                }
            }
            else if (!edit && Books.ManualBookExists(book.Title, book.Authors))
            {
                ex = new Exception("Book " + book.Title + " already exists for the selected author");
                return false;
            }

            return true;


        }

        private static bool DictToMovie(ref Movie Movie, Dictionary<string, object> data, out Exception ex, out bool amazonMovie, bool edit = false)
        {
            ex = null;
            string badField = "";
            amazonMovie = false;
            if (!CheckFields("Movie", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                if (attrib.Key.ToLower() == "entrytype" && attrib.Value.ToString().ToLower() == "amazon")
                {
                    amazonMovie = true;
                    if (!data.ContainsKey("ASIN") && !data.ContainsKey("asin"))
                    {
                        ex = new Exception("ASIN required for Amazon Movie");
                        return false;
                    }
                    Movie.EntryType = "Amazon";
                }
                else
                {
                    //Movie.EntryType = "Manual";
                    switch (attrib.Key.ToLower())
                    {
                       case "asin":
                            Movie.ASIN = attrib.Value.ToString();
                            break;
                        case "ean":
                            Movie.EAN = attrib.Value.ToString();
                            break;
                        case "upc":
                            Movie.UPC = attrib.Value.ToString();
                            break;
                        case "title":
                            Movie.Title = attrib.Value.ToString();
                            Movie.SortTitle = Core.Core.ApplySortTitle(Movie.Title);
                            break;
                        case "releasedate":
                            Movie.ReleaseDate = Convert.ToDateTime(attrib.Value).ToShortDateString();
                            break;
                        case "binding":
                            Movie.Binding = attrib.Value.ToString();
                            break;
                        case "imagefilename":
                            Movie.ImageFileName = attrib.Value.ToString();
                            break;
                        case "createddate":
                            Movie.CreatedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "modifieddate":
                            Movie.ModifiedDate = Convert.ToDateTime(attrib.Value);
                            break;
                        case "entrytype":
                            Movie.EntryType = attrib.Value.ToString();
                            break;
                        case "url":
                            Movie.Url = attrib.Value.ToString();
                            break;
                        case "amazonresponse":
                            Movie.AmazonResponse = attrib.Value.ToString();
                            break;
                        case "starring":
                            Movie.Starring = attrib.Value.ToString();
                            break;
                        case "runningtime":
                            Movie.RunningTime = Convert.ToInt32(attrib.Value);
                            break;
                        case "publisher":
                            Movie.Publisher = attrib.Value.ToString();
                            break;
                        case "productgroup":
                            Movie.ProductGroup = attrib.Value.ToString();
                            break;
                        case "manufacturer":
                            Movie.Manufacturer = attrib.Value.ToString();
                            break;
                        case "genre":
                            Movie.Genre = attrib.Value.ToString();
                            break;
                        case "director":
                            Movie.Director = attrib.Value.ToString();
                            break;
                        case "audiencerating":
                            Movie.AudienceRating = attrib.Value.ToString();
                            break;
                        case "isadultproduct":
                            Movie.IsAdultProduct = Convert.ToBoolean(attrib.Value);
                            break;
                    }
                }
            }

            if (amazonMovie)
            {
                if (!edit && Movies.AmazonMovieExists(Movie.ASIN))
                {
                    ex = new Exception("Movie with ASIN " + Movie.ASIN + " already exists");
                    return false;
                }
                if (!Movies.AmazonMovie(ref Movie, out ex, edit: edit))
                {
                    return false;
                }
            }
            else if (!edit && Movies.ManualMovieExists(Movie.Title))
            {
                ex = new Exception("Movie " + Movie.Title + " already exists");
                return false;
            }

            return true;
        }

        private static bool DictToMovieStar(ref MovieStar tvStar, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            bool autoPerson = false;
            string name = "";
            string displayImage = null;
            if (!CheckFields("MovieStar", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "autoperson":
                        autoPerson = Convert.ToBoolean(attrib.Value);
                        if (data.Keys.Contains("Name"))
                        {
                            name = data["Name"].ToString();
                        }
                        else if (data.Keys.Contains("name"))
                        {
                            name = data["name"].ToString();
                        }
                        else if (data.Keys.Contains("NAME"))
                        {
                            name = data["NAME"].ToString();
                        }
                        else
                        {
                            ex = new KeyNotFoundException("automatic person generation requires name");
                            return false;
                        }
                        break;
                    case "personid":
                        tvStar.PersonId = new Guid(attrib.Value.ToString());
                        break;
                    case "createddate":
                        tvStar.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        tvStar.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "image":
                        displayImage = attrib.Value.ToString();
                        break;
                }
            }

            if (autoPerson)
            {
                Person person;
                if (!People.FindSavePerson(name, false, out person, out ex, displayImage))
                {
                    return false;
                }
                tvStar.PersonId = person.Id;
            }
            else if (tvStar.PersonId != null)
            {
                if (!People.PersonExistsById(tvStar.PersonId))
                {
                    ex = new KeyNotFoundException("Person ID not found");
                    return false;
                }
            }
            else
            {
                ex = new Exception("Either AutoPerson must be set or PersonId included in the request");
                return false;
            }

            return true;
        }

        private static bool MovieToStarToDict(ref MovieToMovieStar tvStar, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("MovieToMovieStar", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }
            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "movieid":
                        tvStar.MovieId = new Guid(attrib.Value.ToString());
                        break;
                    case "moviestarid":
                        tvStar.MovieStarId = new Guid(attrib.Value.ToString());
                        break;
                }
            }
            return true;
        }

        private static bool DictToTVShow(ref TVShow tvShow, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("TVShow", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }
            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "title":
                        tvShow.Title = attrib.Value.ToString();
                        tvShow.SortTitle = Core.Core.ApplySortTitle(tvShow.Title);
                        break;
                    case "createddate":
                        tvShow.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        tvShow.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "displayimage":
                        tvShow.DisplayImage = attrib.Value.ToString();
                        break;
                    case "url":
                        tvShow.Url = attrib.Value.ToString();
                        break;
                    case "stars":
                        tvShow.Stars = attrib.Value.ToString();
                        break;

                }
            }

            if(!TVShows.UpdateShow(ref tvShow, out ex))
            {
                return false;
            }
            return true;
        }

        private static bool DictToTVStar(ref TVStar tvStar, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            bool autoPerson = false;
            string name = "";
            string displayImage = null;
            if (!CheckFields("TVStar", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "autoperson":
                        autoPerson = Convert.ToBoolean(attrib.Value);
                        if (data.Keys.Contains("Name"))
                        {
                            name = data["Name"].ToString();
                        }
                        else if (data.Keys.Contains("name"))
                        {
                            name = data["name"].ToString();
                        }
                        else if (data.Keys.Contains("NAME"))
                        {
                            name = data["NAME"].ToString();
                        }
                        else
                        {
                            ex = new KeyNotFoundException("automatic person generation requires name");
                            return false;
                        }
                        break;
                    case "personid":
                        tvStar.PersonId = new Guid(attrib.Value.ToString());
                        break;
                    case "createddate":
                        tvStar.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        tvStar.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "image":
                        displayImage = attrib.Value.ToString();
                        break;
                }
            }

            if (autoPerson)
            {
                Person person;
                if (!People.FindSavePerson(name, false, out person, out ex, displayImage))
                {
                    return false;
                }
                tvStar.PersonId = person.Id;
            }
            else if (tvStar.PersonId != null)
            {
                if (!People.PersonExistsById(tvStar.PersonId))
                {
                    ex = new KeyNotFoundException("Person ID not found");
                    return false;
                }
            }
            else
            {
                ex = new Exception("Either AutoPerson must be set or PersonId included in the request");
                return false;
            }

            return true;
        }
        private static bool TVShowToStarToDict(ref TVShowToTVStar tvStar, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!CheckFields("TVShowToTVStar", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }
            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "tvshowid":
                        tvStar.TVShowId = new Guid(attrib.Value.ToString());
                        break;
                    case "tvstarid":
                        tvStar.TVStarId = new Guid(attrib.Value.ToString());
                        break;
                }
            }
            return true;
        }

        

        private static bool DictToMagazine(ref Magazine magazine, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            PDFLibraryService pdfService = new PDFLibraryService("grbrewer@gmail.com", "!Pass248word");
            ex = null;
            string badField = "";
            bool auto_pdf = true;
            if (!CheckFields("Magazine", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "pdfcategoryfolderid":
                        magazine.PdfCategoryFolderId = Convert.ToInt32(attrib.Value);
                        auto_pdf = false;
                        break;
                    case "title":
                        magazine.Title = attrib.Value.ToString();
                        magazine.SortTitle = Core.Core.ApplySortTitle(magazine.Title);
                        break;
                    
                    case "createddate":
                        magazine.CreatedDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "modifieddate":
                        magazine.ModifiedDate = Convert.ToDateTime(attrib.Value);
                        break;
                }
            }

            if(pdfService.LoggedIn() && auto_pdf)
            {
                if(!Magazines.GetCategoryFolder(ref magazine, out ex))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DictToMagazineIssue(ref MagazineIssue magazineissue, Dictionary<string, object> data, out Exception ex, bool edit = false)
        {
            ex = null;
            string badField = "";
            if (!edit)
            {
                ex = new Exception("Unable to manually create a Magazine Issue");
                return false;
            }

            if (!CheckFields("MagazineIssue", data.Keys.ToList(), out badField, out ex, edit))
            {
                return false;
            }

            foreach (KeyValuePair<string, object> attrib in data)
            {
                switch (attrib.Key.ToLower())
                {
                    case "releasedate":
                        magazineissue.ReleaseDate = Convert.ToDateTime(attrib.Value);
                        break;
                    case "releasedatetext":
                        magazineissue.ReleaseDateText = attrib.Value.ToString();
                        break;
                    default:
                        ex = new Exception("Field " + attrib.Key + " not a valid edit field for MagazineIssue");
                        return false;
                }
            }

            return true;
        }
    }
}
