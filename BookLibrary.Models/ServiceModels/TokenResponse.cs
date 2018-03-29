using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ServiceModels
{
    public class TokenResponse
    {
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public string access_token { get; set; }
    }
}
