using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.API
{
    public class RetrieveResponse<T>
    {
        public RetrieveRequest Request { get; set; }
        public List<T> Results { get; set; }

        public string RawSql { get; set; }
    }
}
