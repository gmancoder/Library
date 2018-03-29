using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class MovieStarDetailViewModel : MovieStarListViewModel
    {
        public List<Movie> Movies { get; set; }
        public Guid? CelebrityId { get; set; }
    }
}
