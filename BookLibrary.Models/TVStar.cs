using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class TVStar
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public virtual Person Person { get; set; }

        public virtual List<TVShowToTVStar> TVShows { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
