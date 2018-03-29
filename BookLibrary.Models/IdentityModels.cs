using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Infrastructure;

namespace BookLibrary.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            //((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 1800;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<Author> Authors { get; set; }
        public System.Data.Entity.DbSet<Book> Books { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public System.Data.Entity.DbSet<Category> Categories { get; set; }
        //public System.Data.Entity.DbSet<ObjectCategory> ObjectCategories { get; set; }
        public System.Data.Entity.DbSet<ObjectToCategory> ObjectToCategories { get; set; }
        public System.Data.Entity.DbSet<Magazine> Magazines { get; set; }
        public System.Data.Entity.DbSet<MagazineIssue> MagazineIssues { get; set; }
        public System.Data.Entity.DbSet<Album> Albums { get; set; }
        public System.Data.Entity.DbSet<Artist> Artists { get; set; }
        public System.Data.Entity.DbSet<Track> Tracks { get; set; }
        public System.Data.Entity.DbSet<TrackOfTheDay> TracksOfTheDay { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieStar> MovieStars { get; set; }
        public DbSet<MovieToMovieStar> MovieToMovieStars { get; set; }
        public DbSet<TVShow> TVShows { get; set; }
        public DbSet<TVStar> TVStars { get; set; }
        public DbSet<TVShowToTVStar> TVShowToTVStars { get; set; }
        public DbSet<Person> People { get; set; }
        public System.Data.Entity.DbSet<BookLibrary.Models.ApplicationRole> IdentityRoles { get; set; }
    }
}