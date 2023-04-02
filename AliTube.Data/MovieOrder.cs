using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliTube.Data
{
    public class MovieOrder
    {
        public EMovieOrder OrderType { get; set; }
        public bool Descendent { get; set; } = true;

        public static MovieOrder Default
        {
            get
            {
                return new MovieOrder()
                {
                    OrderType = EMovieOrder.Title,
                    Descendent = true
                };
            }
        }
    }
}
