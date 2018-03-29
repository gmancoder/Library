using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models
{
    public class TrackOfTheDay
    {
        public Guid Id { get; set; }
        public Guid TrackId { get; set; }
        public virtual Track Track { get; set; }
        public DateTime Date { get; set; }
    }
}
