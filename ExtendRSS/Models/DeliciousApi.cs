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

namespace SyncLinks.Models
{
    public class DeliciousAPI
    {
        private HttpClient client = new HttpClient();
        public static IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
        private Preference preference;
        private const string host = "https://api.del.icio.us";

        public const string AUTHORITYERROR = "authority error";
        public const string NETWORKERROR = "network error";

        public DeliciousAPI() 
        {
            preference = LoadPreference();
            if (preference == null) preference = new Preference() { IsSycn = false };
        }

        public bool IsSycn()
        {
            return preference.IsSycn;
        }

        public void SetSycn(bool isSycn)
        {
            preference.IsSycn = isSycn;
            SavePreference();
        }

        /// <summary>
        /// 重设用户名和密码.
        /// 由Preference管理本地的存储，因此不用调用SavePreference来保存.
        /// </summary>
        public void SetAccount(string user, string pass)
        {
            preference.Username = user;
            preference.Password = pass;
            SavePreference();
        }

        /// <summary>
        /// 从本地加载Preference
        /// </summary>
        /// <returns>返回Preference. 若没有存储，则返回null</returns>
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

        /// <summary>
        /// 保存Preference到本地
        /// </summary>
        public void SavePreference()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Preference));
            using (var s = store.CreateFile("Config.xml"))
                ser.Serialize(s, preference);
        }

        ///// <summary>
        ///// Check to see when a user last posted an item.
        ///// </summary>
        ///// <returns></returns>
        //public Task<string> GetUpdates(){
        //    return GetAsync(host + "/v1/posts/update");    
        //}

        /// <summary>
        /// Fetch all bookmarks by date or index range.
        /// </summary>
        /// <param name="start">从第start个链接开始显示,最低为0</param>
        /// <param name="count">显示连续的count个链接，最高为100000</param>
        /// <returns>返回链接列表;请求异常则抛出,其中特殊处理了权限异常,通常为错误的用户名或密码</returns>
        public Task< List<BookmarkItem> > GetAll(int start = 0, int count = 300, string tag = null){
            return GetAsync(host + "/v1/posts/all?start=" + start + "&results=" + count + "&tag=" + tag).ContinueWith<List<BookmarkItem>>(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    XDocument doc = XDocument.Parse(t.Result);
                    List<BookmarkItem> result = new List<BookmarkItem>();
                    if (doc.Root.Name == "result") return result;
                    if (doc.Root.Name == "posts")
                    {
                        foreach (XElement node in doc.Descendants("post"))
                        {
                            BookmarkItem item = new BookmarkItem();
                            item.href = node.Attribute("href").Value;
                            item.description = node.Attribute("description").Value;
                            item.extended = ContentDecoder(node.Attribute("extended").Value);
                            item.tag = node.Attribute("tag").Value;
                            item.time = node.Attribute("time").Value;
                            item.time = item.time.Replace('T', ' ').Replace('Z', ' ').Trim();                    //may slow down the deal speed, better to use regex
                            if (IsExits(item.href))
                            {
                                BookmarkItem pItem = LoadLinkItemRecord(item.href);
                                item.isUnReaded = pItem.isUnReaded;
                                if (item.time.CompareTo(pItem.time) > 0)
                                {
                                    SaveLinkItemRecord(item);
                                }
                            }
                            else
                            {
                                if (Regex.IsMatch(item.tag, "(^|\\W)Readed($|\\W)")) item.isUnReaded = "0";
                                else item.isUnReaded = "1";
                                SaveLinkItemRecord(item);
                            }
                            result.Add(item);
                        }
                    }
                    return result;
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    throw t.Exception.InnerException;
                }
                return new List<BookmarkItem>();
            });
        }

    /// <summary>
    /// Add a new bookmark.
    /// </summary>
    /// <returns>添加成功，返回"done";否则返回错误信息</returns>
    public Task<string> AddBookmark(BookmarkItem it){
        string extended = ContentEncoder(it.extended);
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"url", it.href},
            {"description", it.description},
            {"tags", it.tag},
            {"extended", extended},
            {"replace", "yes"}
        };
        return PostAsync(host + "/v1/posts/add", data).ContinueWith<string>(t =>
        {
            if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
            {
                XDocument doc = XDocument.Parse(t.Result);
                return doc.Root.Attribute("code").Value;
            }
            else if (t.Status == TaskStatus.Faulted)
            {
                throw t.Exception.InnerException;
            }
            return "error";
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
        /// 开始一个异步任务来获取GET请求内容。
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的字符串内容</returns>
        public Task<string> GetAsync(string url)
        {
            Uri uri = new Uri(url);
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
            HttpClient client = new HttpClient(handler);
            return client.GetStringAsync(uri).ContinueWith<string>(t =>
            {
                string result = "";
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    result = t.Result;
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    //"Response status code does not indicate success: 401 (Unauthorized)."
                    if (t.Exception.InnerException.Message.Contains("401 (Unauthorized)"))
                        throw new Exception(AUTHORITYERROR);
                    else throw new Exception(NETWORKERROR);
                }
                return result;
            });
        }

        /// <summary>
        /// 开始一个异步任务来获取POST请求内容
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="data">发送内容</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的字符串内容</returns>
        public async Task<string> PostAsync(string url, Dictionary<string, string> data)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
            using (HttpClient client = new HttpClient(handler))
            {
                HttpContent content = new FormUrlEncodedContent(data);
                var result = await client.PostAsync(url, content);
                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 从本地加载链接记录.
        /// </summary>
        /// <returns>目录名为链接的哈希码,所以返回的记录列表为乱序</returns>
        public SortedSet<BookmarkItem> LoadLinkItemsRecord(){
            SortedSet<BookmarkItem> result = new SortedSet<BookmarkItem>();
            XmlSerializer ser = new XmlSerializer(typeof(BookmarkItem));
            foreach (var i in store.GetDirectoryNames("*"))
                if (store.FileExists(i + "/BookmarkItemRecord.xml"))
                    using (var s = store.OpenFile(i + "/BookmarkItemRecord.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        BookmarkItem it = (BookmarkItem)ser.Deserialize(s);
                        result.Add(it);
                    }
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
            return store.FileExists(url.GetHashCode().ToString() + "/BookmarkItemRecord.xml");
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

        public string ContentEncoder(string content)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in content)
            {
                sb.Append(string.Format("%{0:X}", Convert.ToInt32(c)));
            }
            return sb.ToString();
        }

        public string ContentDecoder(string content)
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
            }catch(Exception e){
                return content;
            }
        }

        /// <summary>
        /// 从本地加载笔记内容
        /// </summary>
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

        /// <summary>
        /// 保存笔记内容到本地
        /// </summary>
        public void SaveNote(string url, string content)
        {
            string path = url.GetHashCode().ToString();
            if (store.DirectoryExists(path) == false)
                store.CreateDirectory(path);
            using (var s = new StreamWriter(store.CreateFile(path + "/Note.xml")))
                s.Write(content);
        }

        /// <summary>
        /// 清除存储的BookmarkItem项,保留笔记
        /// </summary>
        public void ClearStorage()
        {
            foreach (var i in store.GetDirectoryNames("*"))
                if (store.FileExists(i + "/BookmarkItemRecord.xml"))
                {
                    store.DeleteFile(i + "/BookmarkItemRecord.xml");
                }
            //            store.DeleteFile(store.GetFileNames("*/BookmarkItemRecord.xml"));
        }

        /// <summary>
        /// 当前只实现删除本地信息
        /// </summary>
        public void DeleteBookmark(string url)
        {
            string path = url.GetHashCode().ToString();
            if (store.FileExists(path + "/BookmarkItemRecord.xml"))
                store.DeleteFile(path + "/BookmarkItemRecord.xml");
        }
    }
}
