using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SyncLinks.Models
{
    public class LocalFileCache
    {
        private DataContractJsonSerializer bookmarkItem_ser;
        private ItemListIndex itemListIndex;
        private Dictionary<string, BookmarkItem> bookmarkItemPool;

        public delegate void ListIndexChangedEventHandler(Views.IndexPage.PageStatus pageStatus);
        public event ListIndexChangedEventHandler ListIndexChanged;

        public LocalFileCache()
        {
            bookmarkItem_ser = new DataContractJsonSerializer(typeof(BookmarkItem));
            itemListIndex = new ItemListIndex();
            bookmarkItemPool = new Dictionary<string, BookmarkItem>();
        }

        #region Creation of Bookmark
        public BookmarkItem GetBookmarkItem(string url)
        {
            url = url.Trim().ToLower();
            if (bookmarkItemPool.ContainsKey(url)) return bookmarkItemPool[url];
            var item = LoadLinkItemRecord(url);
            if (item != null) {
                bookmarkItemPool.Add(url, item);
            }
            return item;
        }

        /// <returns>返回是否添加了新元素到列表</returns>
        public bool AddBookmarkItem(ref BookmarkItem item, int position = 0, bool isBunchAdd = false)
        {
            string url = item.href.Trim().ToLower();
            if (bookmarkItemPool.ContainsKey(url))
            {
                item.Dispose();
                item = bookmarkItemPool[url];
            }
            else {
                bookmarkItemPool.Add(url, item);
                SaveLinkItemRecord(item);
            }
            return UpdateIndex(item, position, "", isBunchAdd);
        }

        /// <return>返回添加的新元素</return>
        public BookmarkItem AddBookmarkItem(string url, string description, 
                                            string cacheHtml = "", int position = 0)
        {
            url = url.Trim().ToLower();
            if (bookmarkItemPool.ContainsKey(url)) return bookmarkItemPool[url];
            var item = new BookmarkItem(
                url,
                description,
                "",
                true,
                false,
                cacheHtml
            );
            bookmarkItemPool.Add(url, item);
            SaveLinkItemRecord(item);
            UpdateIndex(item, position, "", false);
            return item;
        }

        /// <return>返回添加的新元素的个数</return>
        public int AddBunchItemRecords(List<BookmarkItem> itemList, int position = 0)
        {
            if (itemList.Count == 0) return 0;
            int count = itemList.Count;
            int add = 0;
            for (int i = 0; i < count; i++)
            {
                BookmarkItem item = itemList[i];
                add += AddBookmarkItem(ref item, position + i, true) ? 1 : 0;
                itemList[i] = item;
            }
            if (add > 0 && ListIndexChanged != null)
            {
                if (itemList[0].isUnReaded) ListIndexChanged(Views.IndexPage.PageStatus.UNREAD);
                else ListIndexChanged(Views.IndexPage.PageStatus.READ);
                if (itemList[0].isStar) ListIndexChanged(Views.IndexPage.PageStatus.STAR);
            }
            return add;
        }

        #endregion Creation of Bookmark

        public void DeleteBookmark(BookmarkItem item)
        {
            //delete in item pool
            if (bookmarkItemPool.ContainsKey(item.href))
            {
                bookmarkItemPool.Remove(item.href);
            }
            //delete in storage
            string path = item.href.GetHashCode().ToString();
            if (App.store.FileExists(path + "/BookmarkItemRecord.json"))
                App.store.DeleteFile(path + "/BookmarkItemRecord.json");
            //delete in index
            if (item.isUnReaded)
            {
                if (itemListIndex.UNReadIndex.Remove(item.href) && ListIndexChanged != null)
                    ListIndexChanged(Views.IndexPage.PageStatus.UNREAD);
            }
            else
            {
                if (itemListIndex.ReadIndex.Remove(item.href) && ListIndexChanged != null)
                    ListIndexChanged(Views.IndexPage.PageStatus.READ);
            }
            if (item.isStar)
            {
                if (itemListIndex.StarIndex.Remove(item.href) && ListIndexChanged != null)
                    ListIndexChanged(Views.IndexPage.PageStatus.STAR);
            }
        }

        public List<BookmarkItem> LoadBunchItemRecords(Views.IndexPage.PageStatus pageStatus)
        {
            List<string> index = null;
            if (pageStatus == Views.IndexPage.PageStatus.UNREAD) index = itemListIndex.UNReadIndex;
            else if (pageStatus == Views.IndexPage.PageStatus.READ) index = itemListIndex.ReadIndex;
            else if (pageStatus == Views.IndexPage.PageStatus.RECENT) index = itemListIndex.RecentIndex;
            else if (pageStatus == Views.IndexPage.PageStatus.STAR) index = itemListIndex.StarIndex;
            else index = new List<string>();
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            foreach (var url in index)
            {
                var item = GetBookmarkItem(url);
                if (item != null) itemList.Add(item);
            }
            return itemList;
        }

        public void SaveIndexes(Views.IndexPage.PageStatus pageStatus, List<BookmarkItem> itemList = null)
        {
            itemListIndex.SaveIndex(pageStatus, itemList);
        }

        public void SaveAllIndex()
        {
            itemListIndex.SaveIndex(Views.IndexPage.PageStatus.READ);
            itemListIndex.SaveIndex(Views.IndexPage.PageStatus.UNREAD);
            itemListIndex.SaveIndex(Views.IndexPage.PageStatus.RECENT);
            itemListIndex.SaveIndex(Views.IndexPage.PageStatus.STAR);
        }

        public void BookmarkItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var item = sender as BookmarkItem;
            item.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveLinkItemRecord(item);
            if (e.PropertyName == "isUnReaded" || e.PropertyName == "isStar")
                UpdateIndex(item, 0, e.PropertyName, false);
        }

        public void UpdateRecentIndex(BookmarkItem item)
        {
            itemListIndex.RecentIndex.Insert(0, item.href);
            if (ListIndexChanged != null)
                ListIndexChanged(Views.IndexPage.PageStatus.RECENT);
        }

        private bool UpdateIndex(BookmarkItem item, int position, string property, bool isBunchUpdate = false)
        {
            bool ret = false;
            if (property == "" || property == "isUnReaded")
            {
                if (item.isUnReaded)
                {
                    if (itemListIndex.ReadIndex.Remove(item.href))
                    {
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.READ);
                        }
                        ret = true;
                    }
                    if (itemListIndex.UNReadIndex.Count <= position || itemListIndex.UNReadIndex[position] != item.href)
                    {
                        if (position < itemListIndex.UNReadIndex.Count)
                            itemListIndex.UNReadIndex.Insert(position, item.href);
                        else itemListIndex.UNReadIndex.Add(item.href); 
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.READ);
                        }
                        ret = true;
                    }
                }
                else
                {
                    if (itemListIndex.UNReadIndex.Remove(item.href))
                    {
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.UNREAD);
                        }
                        ret = true;
                    }
                    if (itemListIndex.ReadIndex.Count <= position || itemListIndex.ReadIndex[position] != item.href)
                    {
                        if (position < itemListIndex.ReadIndex.Count)
                            itemListIndex.ReadIndex.Insert(position, item.href);
                        else itemListIndex.ReadIndex.Add(item.href);
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.READ);
                        }
                        ret = true;
                    }
                }
            }
            if (property == "" || property == "isStar")
            {
                if (item.isStar)
                {
                    if (itemListIndex.StarIndex.Count <= position || itemListIndex.StarIndex[position] != item.href)
                    {
                        if (position < itemListIndex.StarIndex.Count)
                            itemListIndex.StarIndex.Insert(position, item.href);
                        else itemListIndex.StarIndex.Add(item.href);
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.STAR);
                        }
                        ret = true;
                    }
                }
                else
                {
                    if (itemListIndex.StarIndex.Remove(item.href))
                    {
                        if (!isBunchUpdate && ListIndexChanged != null)
                        {
                            ListIndexChanged(Views.IndexPage.PageStatus.STAR);
                        }
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private BookmarkItem LoadLinkItemRecord(string url)
        {
            string path = url.GetHashCode().ToString();
            if (App.store.FileExists(path + "/BookmarkItemRecord.json"))
            {
                using (var s = App.store.OpenFile(path + "/BookmarkItemRecord.json", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var it = (BookmarkItem)bookmarkItem_ser.ReadObject(s);
                    return it;
                }
            }
            else return null;
        }

        private void SaveLinkItemRecord(BookmarkItem item)
        {
            string path = item.href.GetHashCode().ToString();
            if (App.store.DirectoryExists(path) == false)
                App.store.CreateDirectory(path);
            using (var s = App.store.CreateFile(path + "/BookmarkItemRecord.json"))
                bookmarkItem_ser.WriteObject(s, item);
        }

        public static bool IsExits(string url)
        {
            return App.store.FileExists(url.GetHashCode().ToString() + "/BookmarkItemRecord.json");
        }

        /// <summary>
        /// 从本地加载笔记内容
        /// </summary>
        public static string LoadNote(string url)
        {
            string path = url.GetHashCode().ToString();
            if (App.store.FileExists(path + "/Note.xml"))
            {
                using (var s = new StreamReader(App.store.OpenFile(path + "/Note.xml", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    return s.ReadToEnd();
            }
            return null;
        }

        /// <summary>
        /// 保存笔记内容到本地
        /// </summary>
        public static void SaveNote(string url, string content)
        {
            string path = url.GetHashCode().ToString();
            if (App.store.DirectoryExists(path) == false)
                App.store.CreateDirectory(path);
            using (var s = new StreamWriter(App.store.CreateFile(path + "/Note.xml")))
                s.Write(content);
        }

        /// <summary>
        /// 清除存储的BookmarkItem项,保留笔记
        /// </summary>
        public void ClearStorage()
        {
            foreach (var i in App.store.GetDirectoryNames("*"))
                if (App.store.FileExists(i + "/BookmarkItemRecord.json"))
                {
                    App.store.DeleteFile(i + "/BookmarkItemRecord.json");
                }
            //            store.DeleteFile(store.GetFileNames("*/BookmarkItemRecord.xml"));            
            itemListIndex.ClearAllIndexes();
            ListIndexChanged(Views.IndexPage.PageStatus.READ);
            ListIndexChanged(Views.IndexPage.PageStatus.UNREAD);
            ListIndexChanged(Views.IndexPage.PageStatus.RECENT);
            ListIndexChanged(Views.IndexPage.PageStatus.STAR);
        }

        #region Utility Function
        public static string ContentEncoder(string content)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in content)
            {
                sb.Append(string.Format("%{0:X}", Convert.ToInt32(c)));
            }
            return sb.ToString();
        }

        public static string ContentDecoder(string content)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in content.Split('%'))
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    int p = Convert.ToInt32(s, 16);
                    sb.Append(char.ConvertFromUtf32(p));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error in ContentDecoder: {0}\n{1}", e.Message, e.StackTrace);
                return content;
            }
        }
        #endregion Utility Function
    }

    public class ItemListIndex
    {
        DataContractJsonSerializer list_ser = new DataContractJsonSerializer(typeof(List<string>));

        public List<string> UNReadIndex { get; set; }
        public List<string> ReadIndex { get; set; }
        public List<string> RecentIndex { get; set; }
        public List<string> StarIndex { get; set; }

        public ItemListIndex()
        {
            LoadIndexes();
        }

        public void LoadIndexes()
        {
            string filename = "unread_index.dat";
            if (App.store.FileExists(filename))
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Open))
                {
                    UNReadIndex = list_ser.ReadObject(stream) as List<string>;
                }
            }
            else UNReadIndex = new List<string>();
            filename = "read_index.dat";
            if (App.store.FileExists(filename))
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Open))
                {
                    ReadIndex = list_ser.ReadObject(stream) as List<string>;
                }
            }
            else ReadIndex = new List<string>();
            filename = "recent_index.dat";
            if (App.store.FileExists(filename))
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Open))
                {
                    RecentIndex = list_ser.ReadObject(stream) as List<string>;
                }
            }
            else RecentIndex = new List<string>();
            filename = "star_index.dat";
            if (App.store.FileExists(filename))
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Open))
                {
                    StarIndex = list_ser.ReadObject(stream) as List<string>;
                }
            }
            else StarIndex = new List<string>();
        }

        public void SaveIndex(Views.IndexPage.PageStatus pageStatus, List<BookmarkItem> itemList = null)
        {
            if (pageStatus == Views.IndexPage.PageStatus.UNREAD)
            {
                if (itemList != null) UNReadIndex = itemList.Select(t => t.href).ToList();
                string filename = "unread_index.dat";
                using (var stream = App.store.OpenFile(filename, FileMode.Create))
                {
                    list_ser.WriteObject(stream, UNReadIndex);
                }
            }else
            if (pageStatus == Views.IndexPage.PageStatus.READ)
            {
                if (itemList != null) ReadIndex = itemList.Select(t => t.href).ToList();
                string filename = "read_index.dat";
                using (var stream = App.store.OpenFile(filename, FileMode.Create))
                {
                    list_ser.WriteObject(stream, ReadIndex);
                }
            }else
            if (pageStatus == Views.IndexPage.PageStatus.RECENT)
            {
                if (itemList != null) RecentIndex = itemList.Select(t => t.href).ToList();
                string filename = "recent_index.dat";
                using (var stream = App.store.OpenFile(filename, FileMode.Create))
                {
                    list_ser.WriteObject(stream, RecentIndex);
                }
            }else
            if (pageStatus == Views.IndexPage.PageStatus.STAR)
            {
                if (itemList != null) StarIndex = itemList.Select(t => t.href).ToList();
                string filename = "star_index.dat";
                using (var stream = App.store.OpenFile(filename, FileMode.Create))
                {
                    list_ser.WriteObject(stream, StarIndex);
                }
            }
        }

        public void ClearAllIndexes()
        {
            UNReadIndex.Clear();
            ReadIndex.Clear();
            RecentIndex.Clear();
            StarIndex.Clear();

            SaveIndex(Views.IndexPage.PageStatus.UNREAD);
            SaveIndex(Views.IndexPage.PageStatus.READ);
            SaveIndex(Views.IndexPage.PageStatus.RECENT);
            SaveIndex(Views.IndexPage.PageStatus.STAR);
        }
    }

}
