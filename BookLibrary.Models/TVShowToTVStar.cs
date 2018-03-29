using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class TVShowToTVStar
    {
        public Guid Id { get; set; }
        public Guid TVShowId { get; set; }
        public virtual TVShow TVShow { get; set; }
        public Guid TVStarId { get; set; }
        public virtual TVStar TVStar { get; set; }
        public bool ManuallyAdded { get; set; }
    }
}
