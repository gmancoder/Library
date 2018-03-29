using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PDFLibrary
{
    public class Category
    {
        public Int32 Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Private { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }
}
