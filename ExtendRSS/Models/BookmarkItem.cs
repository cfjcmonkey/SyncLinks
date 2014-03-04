using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendRSS.Models
{
    public class BookmarkItem
    {
        public string href { set; get; }
        public string description { set; get; }
        public string extended { set; get; }
        public string hash { set; get; }
        public string meta { set; get; }
        public string others{ set; get; }
        public string tag { set; get; }
        public string time { set; get; }
        public string isUnReaded { set; get; }
    }
}
