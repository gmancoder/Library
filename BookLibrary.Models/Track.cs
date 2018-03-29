
using System;
using System.Web.Mvc;

namespace BookLibrary.Models
{
    public class Track
    {
        private ApplicationDbContext _ctx = new ApplicationDbContext();
        public Guid Id { get; set; }
        public Guid ArtistId { get; set; }
        public virtual Artist Artist { get; set; }
        public Guid AlbumId { get; set; }
        public Album Album { get; set; }
        public string Name { get; set; }
        public int DiscNumber { get; set; }
        public int TrackNumber { get; set; }
        [AllowHtml]
        public string Lyrics { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        

        public Track()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}