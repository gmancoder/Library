using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class MovieStar
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public virtual Person Person { get; set; }
        //public Guid? CelebrityId { get; set; }
        //public string Name { get; set; }
        //public string Image { get; set; }
        //public bool ManuallyAdded { get; set; }
        public virtual List<MovieToMovieStar> Movies { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
