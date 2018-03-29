using BookLibrary.Functions;
using BookLibrary.Models;
using BookLibrary.Models.API;
using BookLibrary.WebAPI.Models;
using Humanizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BookLibrary.WebAPI.Controllers
{
    [System.Web.Http.RoutePrefix("api")]
    public class LibraryAPIController : ApiController
    {
        private AuthRepository _repo;
        private ApplicationDbContext ctx = new ApplicationDbContext();
        private Logger log = new Logger(typeof(LibraryAPIController));
        public LibraryAPIController()
        {
            _repo = new AuthRepository();
            HttpContext.Current.Server.ScriptTimeout = 300;
        }

        [Authorize]
        [Route("Retrieve")]
        [HttpPost]
        public IHttpActionResult Retrieve(RetrieveRequest request)
        {
            
            log.Info("Retrieve");
            log.Info(JsonConvert.SerializeObject(request));
            Exception ex;
            if (ModelState.IsValid)
            {
                string table = "";
                string conditions = "";
                List<object> conditionValues = new List<object>();
                string orders = "";
                table = request.Entity.ToLower().Pluralize();
                string sql = "select ";
                if(request.Fields != null && request.Fields.Count() > 0)
                {
                    sql += String.Join(", ", request.Fields);
                }
                else
                {
                    sql += "*";
                }
                sql += " from " + table;
                if(request.Conditions != null && request.Conditions.Count() > 0)
                {
                    sql += " where ";
                    
                    if (!API.Where(request.Conditions, request.WhereOperator, out conditions, out conditionValues, out ex))
                    {
                        return InternalServerError(log.Error(request.Conditions, ex));
                    }
                    sql += conditions;
                }
                if(request.Order != null && request.Order.Count() > 0)
                {
                    sql += " order by ";
                    if (!API.Order(request.Order, request.OrderDirection, out orders, out ex))
                    {
                        return InternalServerError(log.Error(request.Order, ex));
                    }
                    sql += orders;
                }
                List<SqlParameter> parameters = API.ValuesToParameters(conditionValues);
                try
                {
                    switch (request.Entity.ToLower())
                    {
                        case "person":
                            return Ok<RetrieveResponse<Person>>(new RetrieveResponse<Person>
                            {
                                Request = request,
                                Results = API.SqlQuery<Person>(sql, parameters),
                                RawSql = sql
                            });
                        case "album":
                            return Ok<RetrieveResponse<Album>>(new RetrieveResponse<Album>
                            {
                                Request = request,
                                Results = API.SqlQuery<Album>(sql, parameters),
                                RawSql = sql
                            });
                        case "artist":
                            return Ok<RetrieveResponse<Artist>>(new RetrieveResponse<Artist>
                            {
                                Request = request,
                                Results = API.SqlQuery<Artist>(sql, parameters),
                                RawSql = sql
                            });
                        case "track":
                            return Ok<RetrieveResponse<Track>>(new RetrieveResponse<Track>
                            {
                                Request = request,
                                Results = API.SqlQuery<Track>(sql, parameters),
                                RawSql = sql
                            });
                        case "category":
                            return Ok<RetrieveResponse<Category>>(new RetrieveResponse<Category>
                            {
                                Request = request,
                                Results = API.SqlQuery<Category>(sql, parameters),
                                RawSql = sql
                            });
                        case "objectcategory":
                            return Ok<RetrieveResponse<ObjectToCategory>>(new RetrieveResponse<ObjectToCategory>
                            {
                                Request = request,
                                Results = API.SqlQuery<ObjectToCategory>(sql, parameters),
                                RawSql = sql
                            });
                        case "book":
                            return Ok<RetrieveResponse<Book>>(new RetrieveResponse<Book>
                            {
                                Request = request,
                                Results = API.SqlQuery<Book>(sql, parameters),
                                RawSql = sql
                            });
                        case "author":
                            return Ok<RetrieveResponse<Author>>(new RetrieveResponse<Author>
                            {
                                Request = request,
                                Results = API.SqlQuery<Author>(sql, parameters),
                                RawSql = sql
                            });
                        case "magazine":
                            return Ok<RetrieveResponse<Magazine>>(new RetrieveResponse<Magazine>
                            {
                                Request = request,
                                Results = API.SqlQuery<Magazine>(sql, parameters),
                                RawSql = sql
                            });
                        case "magazineissue":
                            return Ok<RetrieveResponse<MagazineIssue>>(new RetrieveResponse<MagazineIssue>
                            {
                                Request = request,
                                Results = API.SqlQuery<MagazineIssue>(sql, parameters),
                                RawSql = sql
                            });
                        case "movie":
                            return Ok<RetrieveResponse<Movie>>(new RetrieveResponse<Movie>
                            {
                                Request = request,
                                Results = API.SqlQuery<Movie>(sql, parameters),
                                RawSql = sql
                            });
                        case "moviestar":
                            return Ok<RetrieveResponse<MovieStar>>(new RetrieveResponse<MovieStar>
                            {
                                Request = request,
                                Results = API.SqlQuery<MovieStar>(sql, parameters),
                                RawSql = sql
                            });
                        case "tvshow":
                            return Ok<RetrieveResponse<TVShow>>(new RetrieveResponse<TVShow>
                            {
                                Request = request,
                                Results = API.SqlQuery<TVShow>(sql, parameters),
                                RawSql = sql
                            });
                        case "tvstar":
                            return Ok<RetrieveResponse<TVStar>>(new RetrieveResponse<TVStar>
                            {
                                Request = request,
                                Results = API.SqlQuery<TVStar>(sql, parameters),
                                RawSql = sql
                            });
                    }
                }
                catch (Exception e)
                {
                    return InternalServerError(log.Error(sql, e));
                }
            }
            log.Error(JsonConvert.SerializeObject(ModelState));
            return BadRequest(ModelState);
        }

        [HttpPost]
        [Authorize]
        [Route("Create")]
        public IHttpActionResult Create(CreateRequest request)
        {
            log.Info("Create");
            log.Info(request);
            if(!ModelState.IsValid)
            {
                log.Error(ModelState);
                return BadRequest(ModelState);
            }
            Exception ex;
            switch(request.Entity.ToLower())
            {
                case "album":
                    Album album;
                    if(!API.Create<Album>(request, out album, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Album, CreateRequest>(album, request, album.Id);
                case "artist":
                    Artist Artist;
                    if (!API.Create<Artist>(request, out Artist, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Artist, CreateRequest>(Artist, request, Artist.Id);
                case "track":
                    Track Track;
                    if (!API.Create<Track>(request, out Track, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Track, CreateRequest>(Track, request, Track.Id);
                case "category":
                    Category Category;
                    if (!API.Create<Category>(request, out Category, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Category, CreateRequest>(Category, request, Category.Id);
                case "objectcategory":
                    ObjectToCategory ObjectToCategory;
                    if (!API.Create<ObjectToCategory>(request, out ObjectToCategory, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<ObjectToCategory, CreateRequest>(ObjectToCategory, request, ObjectToCategory.Id);
                case "book":
                    Book Book;
                    if (!API.Create<Book>(request, out Book, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Book, CreateRequest>(Book, request, Book.Id);
                case "author":
                    Author Author;
                    if (!API.Create<Author>(request, out Author, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Author, CreateRequest>(Author, request, Author.Id);
                case "magazine":
                    Magazine Magazine;
                    if (!API.Create<Magazine>(request, out Magazine, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Magazine, CreateRequest>(Magazine, request, Magazine.Id);
                case "movie":
                    Movie Movie;
                    if (!API.Create<Movie>(request, out Movie, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Movie, CreateRequest>(Movie, request, Movie.Id);
                case "moviestar":
                    MovieStar MovieStar;
                    if (!API.Create<MovieStar>(request, out MovieStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<MovieStar, CreateRequest>(MovieStar, request, MovieStar.Id);
                case "movietomoviestar":
                    MovieToMovieStar movieToStar;
                    if(!API.Create<MovieToMovieStar>(request, out movieToStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<MovieToMovieStar, CreateRequest>(movieToStar, request, movieToStar.Id);
                case "tvshow":
                    TVShow TVShow;
                    if (!API.Create<TVShow>(request, out TVShow, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<TVShow, CreateRequest>(TVShow, request, TVShow.Id);
                case "tvshowstar":
                    TVStar TVStar;
                    if (!API.Create<TVStar>(request, out TVStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<TVStar, CreateRequest>(TVStar, request, TVStar.Id);
                case "tvshowtotvstar":
                    TVShowToTVStar tvshowToStar;
                    if (!API.Create<TVShowToTVStar>(request, out tvshowToStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<TVShowToTVStar, CreateRequest>(tvshowToStar, request, tvshowToStar.Id);
                default:
                    return InternalServerError(log.Error("Entity " + request.Entity + " not valid"));
            }
        }

        [HttpPost]
        [Authorize]
        [Route("Update")]
        public IHttpActionResult Update(UpdateRequest request)
        {
            log.Info("Update");
            log.Info(request);
            if (!ModelState.IsValid)
            {
                log.Error(ModelState);
                return BadRequest(ModelState);
            }
            Exception ex;
            switch (request.Entity.ToLower())
            {
                case "album":
                    Album album;
                    if(!API.Update<Album>(request, out album, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Album, UpdateRequest>(album, request, album.Id);
                case "artist":
                    Artist Artist;
                    if (!API.Update<Artist>(request, out Artist, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Artist, UpdateRequest>(Artist, request, Artist.Id);
                case "track":
                    Track Track;
                    if (!API.Update<Track>(request, out Track, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Track, UpdateRequest>(Track, request, Track.Id);
                case "category":
                    Category Category;
                    if (!API.Update<Category>(request, out Category, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Category, UpdateRequest>(Category, request, Category.Id);
                case "objectcategory":
                    ObjectToCategory ObjectToCategory;
                    if (!API.Update<ObjectToCategory>(request, out ObjectToCategory, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<ObjectToCategory, UpdateRequest>(ObjectToCategory, request, ObjectToCategory.Id);
                case "book":
                    Book Book;
                    if (!API.Update<Book>(request, out Book, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Book, UpdateRequest>(Book, request, Book.Id);
                case "author":
                    Author Author;
                    if (!API.Update<Author>(request, out Author, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Author, UpdateRequest>(Author, request, Author.Id);
                case "magazine":
                    Magazine Magazine;
                    if (!API.Update<Magazine>(request, out Magazine, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Magazine, UpdateRequest>(Magazine, request, Magazine.Id);
                case "magazineissue":
                    MagazineIssue MagazineIssue;
                    if (!API.Update<MagazineIssue>(request, out MagazineIssue, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<MagazineIssue, UpdateRequest>(MagazineIssue, request, MagazineIssue.Id);
                case "movie":
                    Movie Movie;
                    if (!API.Update<Movie>(request, out Movie, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<Movie, UpdateRequest>(Movie, request, Movie.Id);
                case "moviestar":
                    MovieStar MovieStar;
                    if (!API.Update<MovieStar>(request, out MovieStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<MovieStar, UpdateRequest>(MovieStar, request, MovieStar.Id);
                
                case "tvshow":
                    TVShow TVShow;
                    if (!API.Update<TVShow>(request, out TVShow, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<TVShow, UpdateRequest>(TVShow, request, TVShow.Id);
                case "tvshowstar":
                    TVStar TVStar;
                    if (!API.Update<TVStar>(request, out TVStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Data, ex));
                    }
                    return NonQueryOK<TVStar, UpdateRequest>(TVStar, request, TVStar.Id);
                
                default:
                    return InternalServerError(log.Error("Entity " + request.Entity + " not valid"));
            }
        }

        private IHttpActionResult NonQueryOK<TObj, TReq>(TObj obj, TReq request, Guid Id)
        {
            return Ok<NonQueryResponse<TObj, TReq, Guid>>(new NonQueryResponse<TObj, TReq, Guid>
            {
                Request = request,
                Result = obj,
                Id = Id
            });
        }

        private IHttpActionResult NonQueryOK<TObj, TReq>(TObj obj, TReq request, Int32 Id)
        {
            return Ok<NonQueryResponse<TObj, TReq, Int32>>(new NonQueryResponse<TObj, TReq, Int32>
            {
                Request = request,
                Result = obj,
                Id = Id
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete")]
        public IHttpActionResult Delete(DeleteRequest request)
        {
            log.Info("Delete");
            log.Info(request);
            if (!ModelState.IsValid)
            {
                log.Error(ModelState);
                return BadRequest(ModelState);
            }

            Exception ex;
            switch (request.Entity.ToLower())
            {
                case "album":
                    Album album;
                    if (!API.Delete<Album>(request, out album, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Album, DeleteRequest>(album, request, album.Id);
                case "artist":
                    Artist Artist;
                    if (!API.Delete<Artist>(request, out Artist, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Artist, DeleteRequest>(Artist, request, Artist.Id);
                case "track":
                    Track Track;
                    if (!API.Delete<Track>(request, out Track, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Track, DeleteRequest>(Track, request, Track.Id);
                case "category":
                    Category Category;
                    if (!API.Delete<Category>(request, out Category, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Category, DeleteRequest>(Category, request, Category.Id);
                case "objectcategory":
                    ObjectToCategory ObjectToCategory;
                    if (!API.Delete<ObjectToCategory>(request, out ObjectToCategory, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<ObjectToCategory, DeleteRequest>(ObjectToCategory, request, ObjectToCategory.Id);
                case "book":
                    Book Book;
                    if (!API.Delete<Book>(request, out Book, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Book, DeleteRequest>(Book, request, Book.Id);
                case "author":
                    Author Author;
                    if (!API.Delete<Author>(request, out Author, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Author, DeleteRequest>(Author, request, Author.Id);
                case "magazine":
                    Magazine Magazine;
                    if (!API.Delete<Magazine>(request, out Magazine, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Magazine, DeleteRequest>(Magazine, request, Magazine.Id);
                case "magazineissue":
                    MagazineIssue MagazineIssue;
                    if (!API.Delete<MagazineIssue>(request, out MagazineIssue, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<MagazineIssue, DeleteRequest>(MagazineIssue, request, MagazineIssue.Id);
                case "movie":
                    Movie Movie;
                    if (!API.Delete<Movie>(request, out Movie, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<Movie, DeleteRequest>(Movie, request, Movie.Id);
                case "moviestar":
                    MovieStar MovieStar;
                    if (!API.Delete<MovieStar>(request, out MovieStar, out ex))
                    {
                        return InternalServerError(log.Error(request.Id, ex));
                    }
                    return NonQueryOK<MovieStar, DeleteRequest>(MovieStar, request, MovieStar.Id);
                default:
                    return InternalServerError(log.Error("Entity " + request.Entity + " not valid"));
            }
        }

        [HttpPost]
        [Authorize]
        [Route("Sync")]
        public IHttpActionResult Sync(SyncRequest request)
        {
            log.Info("Sync");
            log.Info(JsonConvert.SerializeObject(request));
            if (!ModelState.IsValid)
            {
                log.Error(JsonConvert.SerializeObject(ModelState));
                return BadRequest(ModelState);
            }

            Exception ex;
            switch (request.Entity.ToLower())
            {
                case "movie":
                    Movie Movie;
                    if(!Movies.RefreshCast(new Guid(request.Id.ToString()), out Movie, out ex))
                    {
                        log.Error(ex.Message, ex);
                        return InternalServerError(ex);
                    }
                    return NonQueryOK<Movie, SyncRequest>(Movie, request, Movie.Id);
                case "magazine":
                    Magazine Magazine;
                    if(!Magazines.SyncIssues(new Guid(request.Id.ToString()), out Magazine, out ex))
                    {
                        log.Error(ex.Message, ex);
                        return InternalServerError(ex);
                    }
                    return NonQueryOK<Magazine, SyncRequest>(Magazine, request, Magazine.Id);
                case "tvshow":
                    TVShow tvShow;
                    if (!TVShows.RefreshCast(new Guid(request.Id.ToString()), out tvShow, out ex))
                    {
                        log.Error(ex.Message, ex);
                        return InternalServerError(ex);
                    }
                    return NonQueryOK<TVShow, SyncRequest>(tvShow, request, tvShow.Id);
                default:
                    log.Error("Entity '" + request.Entity + "' not syncable");
                    return BadRequest();
            }   
        }
    }
}