using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections;

namespace ExtendRSS.Models
{
    public class BookmarkItem : INotifyPropertyChanged, IComparable
    {
        [XmlIgnore]
        public static string UNREAD = "UnReaded";
        [XmlIgnore]
        public static string READ = "Readed";
        [XmlIgnore]
        public static string BLOG = "Blog";
        [XmlIgnore]
        public static string STAR = "Star";

        public string cacheHtml { set; get; }
        public string href { set; get; }
        public string description { set; get; }
        public string extended { set; get; }
        public string hash { set; get; }
        public string meta { set; get; }
        public string others{ set; get; }
        public string tag { set; get; }
        public string time { set; get; }
        private string _isUnReaded;
        public string isUnReaded
        { 
            get { return _isUnReaded;  }
            set { _isUnReaded = value; NotifyPropertyChanged("isUnReaded"); } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int CompareTo(Object x)
        {
            return -1 * time.CompareTo((x as BookmarkItem).time);
        }
    }
}
