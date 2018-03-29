using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.AjaxModels
{
    public class RequestMethodStatus
    {
        public bool status { get; set; }
        public object results { get; set; }
    }
}
