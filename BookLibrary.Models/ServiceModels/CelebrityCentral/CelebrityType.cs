using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class CelebrityType
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
    }
}
