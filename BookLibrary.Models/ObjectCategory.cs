using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class ObjectCategory
    {
        public Guid Id { get; set; }
        public Int32 Id2 { get; set; }
        public Int64 BrowseNodeId { get; set; }
        public string ObjectType { get; set; }
        public Guid ObjectId { get; set; }
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
