using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesData
    {
        public string added { get; set; }
        public string airsDayOfWeek { get; set; }
        public string airsTime { get; set; }
        public List<string> aliases { get; set; }
        public string banner { get; set; }
        public string firstAired { get; set; }
        public List<string> genre { get; set; }
        public Int32 id { get; set; }
        public string imdbId { get; set; }
        public Int32 lastUpdated { get; set; }
        public string network { get; set; }
        public string networkId { get; set; }
        public string overview { get; set; }
        public string rating { get; set; }
        public string runtime { get; set; }
        public Int32? seriesId { get; set; }
        public string seriesName { get; set; }
        public Double? siteRating { get; set; }
        public Int32? siteRatingCount { get; set; }
        public string status { get; set; }
        public string zap2itId { get; set; }
    }
}
