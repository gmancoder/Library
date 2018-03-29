using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class TVDbTokenRequest
    {
        public string apikey { get; set; }
        public string username { get; set; }
        public string userkey { get; set; }
    }
}
