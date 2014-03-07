using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO.IsolatedStorage;

namespace ExtendRSS.Models
{
    public class DeliciousApi
    {
        private HttpClient client = new HttpClient();
        public static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
        private Preference preference;
        private const string host = "https://api.del.icio.us";
        int total; //total links

        public DeliciousApi() 
        {
            preference = LoadPreference();
            if (preference == null) preference = new Preference();
            total = -1;
        }

        public void SetAccount(string user, string pass)
        {
            preference.username = user;
            preference.password = pass;
            SavePreference();
        }

        public Preference LoadPreference()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Preference));
            if (store.FileExists("Config.xml"))
            {
                using (var s = store.OpenFile("Config.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    return ((Preference)ser.Deserialize(s));
            }
            else return null;
        }

        public void SavePreference()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Preference));
            using (var s = store.CreateFile("Config.xml"))
                ser.Serialize(s, preference);
        }

        public string LoadNote(string url)
        {
            string path = url.GetHashCode().ToString();
            if (store.FileExists(path + "/Note.xml"))
            {
                using (var s = new StreamReader(store.OpenFile(path + "/Note.xml", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    return s.ReadToEnd();
            }
            return null;
        }

        public void SaveNote(string url, string content)
        {
            string path = url.GetHashCode().ToString();
            if (store.DirectoryExists(path) == false)
                store.CreateDirectory(path);
            using (var s = new StreamWriter(store.CreateFile(path + "/Note.xml")))
                    s.Write(content);
        }
        /// <summary>
        /// Check to see when a user last posted an item.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetUpdates(){
            return GetAsync(host + "/v1/posts/update");    
        }
        
        /// <summary>
        /// Fetch recent bookmarks.
        /// *old method!
        /// </summary>
        /// <returns>返回书签列表，如果任务失败则返回空列表，如果发生异常则返回null</returns>
        public Task< List<BookmarkItem> > GetRecent(){
            return GetAsync(host + "/v1/posts/recent").ContinueWith<List<BookmarkItem>>(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    if (t.Result.StartsWith("Exception")) return null;
                    XDocument doc = XDocument.Parse(t.Result);
                    List<BookmarkItem> result = new List<BookmarkItem>();
                    foreach (XElement node in doc.Descendants("post"))
                    {
                        BookmarkItem item = new BookmarkItem();
                        item.href = "/Views/BrowserPage.xaml?url=" + node.Attribute("href").Value;
                        item.description = node.Attribute("description").Value;
                        item.extended = node.Attribute("extended").Value;
                        item.tag = node.Attribute("tag").Value;
                        item.time = node.Attribute("time").Value;
                        if (IsExits(item.href))
                        {
                            BookmarkItem pItem = LoadLinkItemRecord(item.href);
                            item.isUnReaded = pItem.isUnReaded;
                            if (item.time.Equals(pItem.time) == false)
                            {
                                SaveLinkItemRecord(item);
                            }
                        }
                        else
                        {
                            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?")) item.isUnReaded = "0";
                            else item.isUnReaded = "1";
                            SaveLinkItemRecord(item);
                        }
                        result.Add(item);
                    }
                    return result;
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    //to do
                }
                return new List<BookmarkItem>();
            });
        }

        /// <summary>
        /// 判断是否还能获取更多的链接
        /// </summary>
        /// <param name="start">已获取的链接数</param>
        /// <returns>初始时,total=-1,经过一次GetAll请求后置为实际的总链接数</returns>
        public bool HasMoreLinks(int start)
        {
            return total < 0 || start < total;
        }

        /// <summary>
        /// Fetch all bookmarks by date or index range.
        /// </summary>
        /// <param name="start">从第start个链接开始显示,最低为0</param>
        /// <param name="count">显示连续的count个链接，最高为100000</param>
        /// <returns>注意未处理"no bookmarks" 需提前判断HasMoreLinks; 如果链接总数是0,则会抛异常</returns>
        public Task< List<BookmarkItem> > GetAll(int start = 0, int count = 300){
            return GetAsync(host + "/v1/posts/all?start=" + start + "&results=" + count).ContinueWith<List<BookmarkItem>>(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    if (t.Result.StartsWith("Exception")) return null;
                    XDocument doc = XDocument.Parse(t.Result);
                    total = Convert.ToInt32(doc.Element("posts").Attribute("total").Value);
                    List<BookmarkItem> result = new List<BookmarkItem>();
                    foreach (XElement node in doc.Descendants("post"))
                    {
                        BookmarkItem item = new BookmarkItem();
                        item.href = "/Views/BrowserPage.xaml?url=" + node.Attribute("href").Value;
                        item.description = node.Attribute("description").Value;
                        item.extended = node.Attribute("extended").Value;
                        item.tag = node.Attribute("tag").Value;
                        item.time = node.Attribute("time").Value;
                        item.time = item.time.Replace('T', ' ').Replace('Z',' ');                    //may slow down the deal speed, better to use regex
                        if (IsExits(item.href))
                        {
                            BookmarkItem pItem = LoadLinkItemRecord(item.href); 
                            item.isUnReaded = pItem.isUnReaded;
                            if (item.time.Equals(pItem.time) == false)
                            {
                                SaveLinkItemRecord(item);
                            }
                        }
                        else
                        {
                            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?")) item.isUnReaded = "0";
                            else item.isUnReaded = "1";
                            SaveLinkItemRecord(item);
                        }
                        result.Add(item);
                    }
                    return result;
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    if (t.Exception.InnerException.Message.Contains("401"))
                        throw new Exception("401 Unauthority error");
                    else throw t.Exception;
                }
                return new List<BookmarkItem>();
            });
        }

    /// <summary>
    /// Add a new bookmark.
    /// *not available! the interface provided is wrong.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Task<string> AddBookmark(BookmarkItem item){
            return GetAsync(host + "/v1/posts/add?url=" + item.href + 
                "&description=" + item.description + 
                "&tags=" + item.tag + 
                "&extended=" + item.extended +
                "&replace=yes").ContinueWith<string>(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    if (t.Result.StartsWith("Exception")) return null;
                    return t.Result;
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    //to do
                }
                return "";
            });
      }

/* 
/v1/posts/delete? — Delete an existing bookmark.
/v1/posts/get? — Get bookmark for a single date, or fetch specific items.
/v1/posts/dates? — List dates on which bookmarks were posted.
/v1/posts/all?hashes — Fetch a change detection manifest of all items.
/v1/posts/suggest — Fetch popular, recommended and network tags for a specific url.
*/
        /// <summary>
        /// 开始一个异步任务来获取请求内容。
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的字符串内容</returns>
        public Task<string> GetAsync(string url)
        {
            Uri uri = new Uri(url);
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(preference.username, preference.password);
            HttpClient client = new HttpClient(handler);
            return client.GetStringAsync(uri).ContinueWith<string>(t =>
            {
                string result = "";
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    result = t.Result;
                }
                else
                {
                    if (t.Exception.InnerException.Message.Contains("401"))
                        throw new Exception("401 Unauthority error");
                    else throw t.Exception;
                }
                return result;
            });
        }

        /// <summary>
        /// 从本地加载链接记录.
        /// </summary>
        /// <returns>目录名为链接的哈希码,所以返回的记录列表为乱序</returns>
        public List<BookmarkItem> LoadLinkItemsRecord(){
            List<BookmarkItem> result = new List<BookmarkItem>();
            XmlSerializer ser = new XmlSerializer(typeof(BookmarkItem));
            foreach (var i in store.GetDirectoryNames("*"))
                if (store.FileExists(i + "/BookmarkItemRecord.xml"))
                    using (var s = store.OpenFile(i + "/BookmarkItemRecord.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                        result.Add((BookmarkItem)ser.Deserialize(s));
            return result;
        }

        public void SaveLinkItemRecord(BookmarkItem item)
        {
            string path = item.href.GetHashCode().ToString();
            if (store.DirectoryExists(path) == false)
                store.CreateDirectory(path);
            XmlSerializer ser = new XmlSerializer(typeof(BookmarkItem));
            using (var s = store.CreateFile(path + "/BookmarkItemRecord.xml"))
                ser.Serialize(s, item);
        }

        public bool IsExits(string url)
        {
            return store.DirectoryExists(url.GetHashCode().ToString());
        }

        public BookmarkItem LoadLinkItemRecord(string url)
        {
            string path = url.GetHashCode().ToString();
            XmlSerializer ser = new XmlSerializer(typeof(BookmarkItem));
            if (store.FileExists(path + "/BookmarkItemRecord.xml"))
            {
                using (var s = store.OpenFile(path + "/BookmarkItemRecord.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    return (BookmarkItem)ser.Deserialize(s);
            }
            else return null;
        }
    }
}
