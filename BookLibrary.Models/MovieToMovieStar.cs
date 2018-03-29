using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class MovieToMovieStar
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public virtual Movie Movie { get; set; }
        public Guid MovieStarId { get; set; }
        public virtual MovieStar MovieStar { get; set; }
        public bool ManuallyAdded { get; set; }
    }
}
