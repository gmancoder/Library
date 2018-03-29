using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class PersonViewModel
    {
        public Person Person { get; set; }
        public string Details { get; set; }
        public LibraryObject AssociatedWith { get; set; }
        public List<LibraryObject> Books { get; set; }
        public List<LibraryObject> Albums { get; set; }
        public List<LibraryObject> Tracks { get; set; }
        public List<LibraryObject> Movies { get; set; }
        public List<LibraryObject> TVShows { get; set; }
    }
}
