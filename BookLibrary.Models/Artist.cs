using System;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.Models
{
    public class Artist
    {
        private List<Album> _albumList = new List<Album>();
        private List<Track> _trackList = new List<Track>();
        private ApplicationDbContext _ctx = new ApplicationDbContext();
        public Guid Id { get; set; }
        public Guid? PersonId { get; set; }
        public virtual Person Person { get; set; }
        //public Guid? CelebrityId { get; set; }
        public Guid? ArtistId { get; set; }
        public virtual Artist AssociatedWith { get; set; }
        public virtual List<Artist> AssociatedArtists { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public bool IsGroup { get; set; }
        public string DisplayImage { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual List<Album> AlbumList { get; set; }

        public Artist()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}