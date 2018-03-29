using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class SearchViewModel
    {
        public Dictionary<string, List<LibraryObject>> Results { get; set; }
        public string Query { get; set; }
    }
}
