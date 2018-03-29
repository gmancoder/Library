using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<LibraryObject> LatestAdditions { get; set; }
        public Track TrackOfTheDay { get; set; }
        public LibraryObject Reading { get; set; }
    }
}
