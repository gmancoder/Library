using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.API
{
    public class NonQueryResponse<TObj, TReq, IdType>
    {
        public TReq Request { get; set; }
        public TObj Result { get; set; }

        public IdType Id { get; set; }
    }
}
