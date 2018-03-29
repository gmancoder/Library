using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class Album
    {
        private List<Track> _trackList = new List<Track>();
        private ApplicationDbContext _ctx = new ApplicationDbContext();
        public Guid Id { get; set; }
        public string EntryType { get; set; }
        public Guid ArtistId { get; set; }
        public virtual Artist Artist { get; set; }
        
        public int? NumberOfDiscs { get; set; }
        
        public string ASIN { get; set; }
        public string EAN { get; set; }
        public string UPC { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string ReleaseDate { get; set; }
        public string Binding { get; set; }
        public string ImageFileName { get; set; }
        public string Url { get; set; }
        public string AmazonResponse { get; set; }
        //public virtual List<AlbumCategory> Categories { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public virtual List<Track> TrackList { get; set; }

        public Album()
        {
            NumberOfDiscs = 1;
        }
    }
}
