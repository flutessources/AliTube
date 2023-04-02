using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliTube.Data
{
    public class GetMoviesMessage
    {
        public MovieOrder? Order { get; set; }
        public List<MovieFilter>? Filters { get; set; }
        public int Limit { get; set; }
        public int Page { get; set; }
    }
}
