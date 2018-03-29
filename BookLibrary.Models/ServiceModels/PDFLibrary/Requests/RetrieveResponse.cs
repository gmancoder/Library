using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels.PDFLibrary.Requests
{
    public class RetrieveResponse<T> : PDFLibraryResponse
    {
        public T Results { get; set; }
    }
}
