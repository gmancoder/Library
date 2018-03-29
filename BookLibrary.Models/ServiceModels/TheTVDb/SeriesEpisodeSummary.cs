using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesEpisodeSummary
    {
        public string airedEpisodes { get; set; }
        public List<string> airedSeasons { get; set; }
        public string dvdEpisodes { get; set; }
        public List<string> dvdSeasons { get; set; }
    }
}
