using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class CelebrityDetail<T>
    {
        public T Item { get; set; }
        public Celebrity Celebrity { get; set; }
    }
}
