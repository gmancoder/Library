using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.AjaxModels
{
    public class AjaxRequest
    {
        public string action { get; set; }
        public string method { get; set; }
        public IDictionary<string, object> data { get; set; }
    }
}
