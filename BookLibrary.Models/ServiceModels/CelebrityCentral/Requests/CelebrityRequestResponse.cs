using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.CelebrityCentral
{
    public class CelebrityRequestResponse<T>
    {
        public CelebrityRetrieveRequest Request { get; set; }
        public List<T> Results { get; set; }
        public string RawSql { get; set; }
    }
}
