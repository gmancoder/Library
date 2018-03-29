using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class SeriesResponse
    {
        public SeriesData data { get; set; }
        public TVDbError errors { get; set; }
    }
}
