using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public Guid? CelebrityId { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public string DisplayImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
