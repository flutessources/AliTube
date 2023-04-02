using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliTube.Core.Scrapping.YTS
{
    public class YTSMessage<T> where T : class
    {
        public T data { get; set; }
    }
}
