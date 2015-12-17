using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace SyncLinks.Models
{
    [DataContract]
    public class BookmarkItem : INotifyPropertyChanged, IComparable, IDisposable
    {
        [DataMember]
        public string href { set; get; }
        [DataMember]
        public string description { set; get; } //title
        [DataMember]
        public string hash { set; get; } //id
        [DataMember (EmitDefaultValue = true)]
        public string meta { set; get; }
        [DataMember(EmitDefaultValue = true)]
        public string others{ set; get; }
        [DataMember(EmitDefaultValue = true)]
        public string tag { set; get; }
        [DataMember(EmitDefaultValue = true)]
        public string time { set; get; }

        private string _cacheHtml;
        [DataMember(EmitDefaultValue = true)]
        public string cacheHtml
        {
            get { return _cacheHtml; }
            set { _cacheHtml = value;  NotifyPropertyChanged("cacheHtml"); }
        }

        private string _extended; //note
        [DataMember(EmitDefaultValue = true)]
        public string extended
        {
            get { return _extended; }
            set { _extended = value; NotifyPropertyChanged("extended"); }
        }

        private bool _isUnReaded;
        [DataMember]
        public bool isUnReaded
        { 
            get { return _isUnReaded;  }
            set { _isUnReaded = value; NotifyPropertyChanged("isUnReaded"); } 
        }

        private bool _isStar;
        [DataMember]
        public bool isStar
        {
            get { return _isStar; }
            set { _isStar = value; NotifyPropertyChanged("isStar"); }
        }

        public BookmarkItem(string href, string description, string hash, bool isUnReaded, bool isStar, string cacheHtml = "")
        {
            this.href = href;
            this.description = description;
            this.hash = hash;
            this.isUnReaded = isUnReaded;
            this.isStar = isStar;
            this.cacheHtml = cacheHtml;

            this.PropertyChanged += App.pocketApi.BookmarkItem_PropertyChanged;
            this.PropertyChanged += App.localFileCache.BookmarkItem_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
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

        public override bool Equals(object obj)
        {
            if (!(obj is BookmarkItem)) return false;
            return this.hash == (obj as BookmarkItem).hash;
        }

        public override int GetHashCode()
        {
            return this.hash.GetHashCode();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.PropertyChanged -= App.pocketApi.BookmarkItem_PropertyChanged;
                this.PropertyChanged -= App.localFileCache.BookmarkItem_PropertyChanged;

                disposedValue = true;
            }
        }

         ~BookmarkItem()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
