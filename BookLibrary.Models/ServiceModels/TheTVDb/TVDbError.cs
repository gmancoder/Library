using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.TheTVDb
{
    public class TVDbError
    {
        public List<string> invalidFilters { get; set; }
        public string invalidLanguage { get; set; }
        public List<string> invalidQueryParams { get; set; }
    }
}
