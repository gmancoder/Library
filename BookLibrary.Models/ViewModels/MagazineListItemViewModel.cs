using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class MagazineListItemViewModel
    {
        public Magazine Magazine { get; set; }
        public string ImageFileName { get; set; }
        public List<List<Category>> CategoryStreams { get; set; }
    }
}
