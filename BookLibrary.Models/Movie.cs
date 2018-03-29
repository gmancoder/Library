using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class Movie
    {
        public Guid Id { get; set; }
        public string EntryType { get; set; }
        public string ASIN { get; set; }
        public string EAN { get; set; }
        public string UPC { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string ReleaseDate { get; set; }
        public string Binding { get; set; }
        public string ImageFileName { get; set; }
        public string Url { get; set; }
        public Int32 RunningTime { get; set; }
        public string Publisher { get; set; }
        public string ProductGroup { get; set; }
        public string Manufacturer { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public string AudienceRating { get; set; }
        public bool IsAdultProduct { get; set; }
        public string AmazonResponse { get; set; }
        //public virtual List<AlbumCategory> Categories { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Starring { get; set; }
        public virtual List<MovieToMovieStar> Stars { get; set; }
    }
}
