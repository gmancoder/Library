using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesImage
    {
        public string fileName { get; set; }
        public Int32 id { get; set; }
        public string keyType { get; set; }
        public Int32? languageId { get; set; }
        public SeriesImageRatingInfo ratingsInfo { get; set; }
        public string resolution { get; set; }
        public string subKey { get; set; }
        public string thumbnail { get; set; }
    }

    public class SeriesImageRatingInfo
    {
        public Double average { get; set; }
        public Int32 count { get; set; }
    }
}
