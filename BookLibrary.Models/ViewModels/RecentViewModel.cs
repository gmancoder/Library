using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class RecentViewModel<T>
    {
        public List<T> Today { get; set; }
        public List<T> Yesterday { get; set; }
        public List<T> ThisWeek { get; set; }
        public List<T> LastWeek { get; set; }
        public Dictionary<int, List<T>> Older { get; set; }
        public int Count { get; set; }
    }
}
