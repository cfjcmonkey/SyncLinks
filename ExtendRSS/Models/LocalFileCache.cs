using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SyncLinks.Models
{
    public class LocalFileCache
    {
        private DataContractJsonSerializer bookmarkItem_ser;
        private Dictionary<string, BookmarkItem> bookmarkItemPool;
        private BookmarkItemList unreadList;
        private BookmarkItemList readList;
        private BookmarkItemList recentList;
        private BookmarkItemList starList;

        public delegate void ListIndexChangedEventHandler(Views.IndexPage.PageStatus pageStatus);
        public event ListIndexChangedEventHandler ListIndexChanged;

        public LocalFileCache()
        {
            bookmarkItem_ser = new DataContractJsonSerializer(typeof(BookmarkItem));
            bookmarkItemPool = new Dictionary<string, BookmarkItem>();
        }

        #region Creation of Bookmark
        public BookmarkItem GetBookmarkItem(string url)
        {
            url = url.Trim();;
            if (bookmarkItemPool.ContainsKey(url)) return bookmarkItemPool[url];
            var item = LoadLinkItemRecord(url);
            if (item != null) {
                bookmarkItemPool.Add(url, item);
                item.PropertyChanged += App.pocketApi.BookmarkItem_PropertyChanged;
                item.PropertyChanged += App.localFileCache.BookmarkItem_PropertyChanged;
            }
            return item;
        }

        /// <returns>返回是否添加了新元素到列表</returns>
        public bool AddBookmarkItem(ref BookmarkItem item, int position = 0, bool isBunchAdd = false)
        {
            string url = item.href.Trim();
            if (bookmarkItemPool.ContainsKey(url))
            {   //更新原链接元素
                var oldItem = bookmarkItemPool[url];
                oldItem.description = item.description;
                oldItem.hash = item.hash;
                oldItem.isUnReaded = item.isUnReaded;
                oldItem.isStar = item.isStar;
                if (!String.IsNullOrWhiteSpace(item.cacheHtml)) oldItem.cacheHtml = item.cacheHtml;

                item.Dispose();
                item = oldItem;
            }
            else {
                bookmarkItemPool.Add(url, item);
                item.PropertyChanged += App.pocketApi.BookmarkItem_PropertyChanged;
                item.PropertyChanged += App.localFileCache.BookmarkItem_PropertyChanged;
                SaveLinkItemRecord(item);
            }
            return UpdateIndex(item, position, "", isBunchAdd);
        }

        /// <return>返回添加的新元素</return>
        public BookmarkItem AddBookmarkItem(string url, string description, 
                                            string cacheHtml = "", int position = 0)
        {
            url = url.Trim();
            if (bookmarkItemPool.ContainsKey(url))
            {   //更新原链接元素
                UpdateItem(url, description, "", cacheHtml);
            }
            var item = new BookmarkItem(
                url,
                description,
                "",
                true,
                false,
                cacheHtml
            );
            bookmarkItemPool.Add(url, item);
            item.PropertyChanged += App.pocketApi.BookmarkItem_PropertyChanged;
            item.PropertyChanged += App.localFileCache.BookmarkItem_PropertyChanged;
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

        public bool UpdateItem(string url, string description, string hash = "", string cacheHtml = "")
        {
            url = url.Trim();
            if (bookmarkItemPool.ContainsKey(url))
            {   //更新原链接元素
                var oldItem = bookmarkItemPool[url];
                oldItem.description = description;
                if (!String.IsNullOrWhiteSpace(cacheHtml)) oldItem.cacheHtml = cacheHtml;
                if (!String.IsNullOrWhiteSpace(hash)) oldItem.hash = hash;
                return true;
            }else return false;
        }

        public void DeleteBookmark(BookmarkItem item)
        {
            //delete in item pool
            if (bookmarkItemPool.ContainsKey(item.href))
            {
                bookmarkItemPool.Remove(item.href);
            }
            //delete in storage
            string path = URL2Path(item.href);
            if (App.store.FileExists(path + "/BookmarkItemRecord.json"))
                App.store.DeleteFile(path + "/BookmarkItemRecord.json");
            //delete in index
            if (item.isUnReaded) GetItemListHolder(Views.IndexPage.PageStatus.UNREAD).Remove(item);
            else GetItemListHolder(Views.IndexPage.PageStatus.READ).Remove(item);
            if (item.isStar) GetItemListHolder(Views.IndexPage.PageStatus.STAR).Remove(item);
        }

        public ObservableCollection<BookmarkItem> LoadItemList(Views.IndexPage.PageStatus pageStatus)
        {
            var listHolder = GetItemListHolder(pageStatus);
            if (listHolder != null) return listHolder.itemList;
            else return null;
        }

        public ObservableCollection<BookmarkItem> ReLoadItemList(Views.IndexPage.PageStatus pageStatus)
        {
            var listHolder = GetItemListHolder(pageStatus);
            if (listHolder != null)
            {
                listHolder.LoadIndex();
                return listHolder.itemList;
            }
            else return null;
        }

        public void SaveAllIndex()
        {
            if (unreadList != null) unreadList.SaveIndex();
            if (readList != null) readList.SaveIndex();
            if (recentList != null) recentList.SaveIndex();
            if (starList != null) starList.SaveIndex();
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
            GetItemListHolder(Views.IndexPage.PageStatus.RECENT).Insert(item, 0);
        }

        private bool UpdateIndex(BookmarkItem item, int position, string property, bool isBunchUpdate = false)
        {
            if (item == null) return false;
            bool ret = false;
            if (property == "" || property == "isUnReaded")
            {
                if (item.isUnReaded)
                {
                    if (GetItemListHolder(Views.IndexPage.PageStatus.READ).Remove(item)) ret = true;
                    if (GetItemListHolder(Views.IndexPage.PageStatus.UNREAD).Insert(item, position)) ret = true;
                }
                else
                {
                    if (GetItemListHolder(Views.IndexPage.PageStatus.UNREAD).Remove(item)) ret = true;
                    if (GetItemListHolder(Views.IndexPage.PageStatus.READ).Insert(item, position)) ret = true;
                }
            }
            if (property == "" || property == "isStar")
            {
                if (item.isStar)
                {
                    if (GetItemListHolder(Views.IndexPage.PageStatus.STAR).Insert(item, position)) ret = true;
                }
                else
                {
                    if (GetItemListHolder(Views.IndexPage.PageStatus.STAR).Remove(item)) ret = true;
                }
            }
            return ret;
        }

        private BookmarkItemList GetItemListHolder(Views.IndexPage.PageStatus pageStatus)
        {
            if (pageStatus == Views.IndexPage.PageStatus.UNREAD)
            {
                if (unreadList == null) unreadList = new BookmarkItemList(Views.IndexPage.PageStatus.UNREAD);
                return unreadList;
            }
            else if (pageStatus == Views.IndexPage.PageStatus.READ)
            {
                if (readList == null) readList = new BookmarkItemList(Views.IndexPage.PageStatus.READ);
                return readList;
            }
            else if (pageStatus == Views.IndexPage.PageStatus.RECENT)
            {
                if (recentList == null) recentList = new BookmarkItemList(Views.IndexPage.PageStatus.RECENT);
                return recentList;
            }
            else if (pageStatus == Views.IndexPage.PageStatus.STAR)
            {
                if (starList == null) starList = new BookmarkItemList(Views.IndexPage.PageStatus.STAR);
                return starList;
            }else return null;
        }

        private BookmarkItem LoadLinkItemRecord(string url)
        {
            string path = URL2Path(url);
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
            string path = URL2Path(item.href);
            if (App.store.DirectoryExists(path) == false)
                App.store.CreateDirectory(path);
            using (var s = App.store.CreateFile(path + "/BookmarkItemRecord.json"))
                bookmarkItem_ser.WriteObject(s, item);
        }

        public static bool IsExits(string url)
        {
            return App.store.FileExists(URL2Path(url) + "/BookmarkItemRecord.json");
        }

        /// <summary>
        /// 从本地加载笔记内容
        /// </summary>
        public static string LoadNote(string url)
        {
            string path = URL2Path(url);
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
            string path = URL2Path(url);
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
                    //App.store.DeleteDirectory(i);
                }
            //            store.DeleteFile(store.GetFileNames("*/BookmarkItemRecord.xml"));            
            GetItemListHolder(Views.IndexPage.PageStatus.UNREAD).ClearIndex();
            GetItemListHolder(Views.IndexPage.PageStatus.READ).ClearIndex();
            GetItemListHolder(Views.IndexPage.PageStatus.RECENT).ClearIndex();
            GetItemListHolder(Views.IndexPage.PageStatus.STAR).ClearIndex();
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

        static Regex urlcharReg = new Regex(@"[\\/:\*\?<>\|]", RegexOptions.Compiled);
        public static string URL2Path(string url)
        {
            return urlcharReg.Replace(url, " ");
        }
        #endregion Utility Function
    }

    public class BookmarkItemList
    {
        static DataContractJsonSerializer list_ser = new DataContractJsonSerializer(typeof(List<string>));

        public ObservableCollection<BookmarkItem> itemList { get; set; }
        private HashSet<string> itemHashSet;
        public Views.IndexPage.PageStatus pageStatus { get; set; }

        public BookmarkItemList(Views.IndexPage.PageStatus status)
        {
            this.pageStatus = status;
            itemList = new ObservableCollection<BookmarkItem>();
            itemHashSet = new HashSet<string>();
            LoadIndex();
        }

        public void LoadIndex()
        {
            itemHashSet.Clear();
            itemList.Clear();
            List<string> itemListIndex = null;
            string filename = "";
            if (pageStatus == Views.IndexPage.PageStatus.UNREAD)
            {
                filename = "unread_index.dat";
            }
            else if (pageStatus == Views.IndexPage.PageStatus.READ)
            {
                filename = "read_index.dat";
            }
            else if (pageStatus == Views.IndexPage.PageStatus.RECENT)
            {
                filename = "recent_index.dat";
            }
            else if (pageStatus == Views.IndexPage.PageStatus.STAR)
            {
                filename = "star_index.dat";
            }
            if (App.store.FileExists(filename))
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Open))
                {
                    itemListIndex = list_ser.ReadObject(stream) as List<string>;
                }
            }
            else itemListIndex = new List<string>();
            if (itemListIndex != null)
            {
                for (int i = 0; i < itemListIndex.Count; i++)
                {
                    var item = App.localFileCache.GetBookmarkItem(itemListIndex[i]);
                    if (item != null) { itemList.Add(item); itemHashSet.Add(itemListIndex[i]); }
                    else Debug.WriteLine("Error in LoadIndex: item {0} from index {1} is null", itemListIndex[i], pageStatus);
                    
                }
            }
        }

        public void SaveIndex()
        {
            string filename = "";
            if (pageStatus == Views.IndexPage.PageStatus.UNREAD)
            {               
                filename = "unread_index.dat";
            }else if (pageStatus == Views.IndexPage.PageStatus.READ)
            {
                filename = "read_index.dat";
            }else if (pageStatus == Views.IndexPage.PageStatus.RECENT)
            {
                filename = "recent_index.dat";
            }else if (pageStatus == Views.IndexPage.PageStatus.STAR)
            {
                filename = "star_index.dat";
            }
            if (String.IsNullOrEmpty(filename) == false)
            {
                using (var stream = App.store.OpenFile(filename, FileMode.Create))
                {
                    list_ser.WriteObject(stream, itemList.Select(t => t.href).ToList());
                }
            }
        }

        public void ClearIndex()
        {
            itemList.Clear();
            itemHashSet.Clear();
            SaveIndex();
        }

        public bool Remove(BookmarkItem item)
        {
            if (item == null) return false;
            if (itemHashSet.Remove(item.href) == false) return false;
            itemList.Remove(item);
            SaveIndex();
            return true;
        }

        public bool Insert(BookmarkItem item, int position)
        {
            if (item == null) return false;
            if (itemHashSet.Add(item.href) == false) return false;
            if (position < itemList.Count) itemList.Insert(position, item);
            else itemList.Add(item);
            SaveIndex();
            return true;
        }
    }

}
