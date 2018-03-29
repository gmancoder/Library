using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels
{
    public class RequestFilter
    {
        public string field { get; set; }
        public string @operator { get; set; }
        public List<object> values { get; set; }
    }
}
