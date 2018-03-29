using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class CategoryIndexViewModel
    {
        public Category Category { get; set; }
        public bool AllCategories { get; set; }
        public string CategoryHtml { get; set; }
    }
}
