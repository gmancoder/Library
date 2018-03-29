using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesEpisodeSummaryResponse
    {
        public SeriesEpisodeSummary data { get; set; }
        public TVDbError errors { get; set; }
    }
}
