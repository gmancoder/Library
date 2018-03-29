using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class LibraryObject
    {
        public string Type { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public string Image { get; set; }
        public List<LibraryObjectBy> ByObjects { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LibraryObjectBy
    {
        public string ByText { get; set; }
        public string ByValue { get; set; }
        public string ByType { get; set; }
        public Guid? ById { get; set; }
    }
}
