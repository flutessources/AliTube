using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliTube.Core.Scrapping.YTS
{
    public class YTSListMovies
    {
        public int movie_count { get; set; }
        public List<YTSMovie> movies { get; set; }
    }
}
