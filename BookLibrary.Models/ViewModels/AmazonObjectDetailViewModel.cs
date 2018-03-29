using BookLibrary.Models.ServiceModels.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Models.ViewModels
{
    public class AmazonObjectDetailViewModel<T>
    {
        public T Object { get; set; }
        public List<EditorialReview> Reviews { get; set; }
        public List<T> SimilarProducts { get; set; }
        public Offers Offers { get; set; }
        public List<List<Category>> CategoryStreams { get; set; }
    }
}
