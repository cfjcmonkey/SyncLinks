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
using System.Text.RegularExpressions;

namespace ExtendRSS.Models
{
    public class DeliciousApi
    {
        private HttpClient client = new HttpClient();

        private string username = "cmonkey";
        private string password = "20092426";
        private string host = "https://api.del.icio.us";

        public void SetUsername(string user){
            username = user;
        }

        public void SetPassword(string pass){
            password = pass;
        }
//        Encoding encoding = Encoding.GetEncoding("gb2312");

        /// <summary>
        /// Check to see when a user last posted an item.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetUpdates(){
            return GetAsync(host + "/v1/posts/update");    
        }
        
        /// <summary>
        /// Fetch recent bookmarks.
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
                        if (Regex.IsMatch(item.tag, "\\W?Read\\W?")) item.isUnReaded = "0";
                        else item.isUnReaded = "1";
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
        /// Fetch all bookmarks by date or index range.
        /// </summary>
        /// <returns></returns>
        public Task< List<BookmarkItem> > GetAll(){
            return GetAsync(host + "/v1/posts/all").ContinueWith<List<BookmarkItem>>(t =>
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
                        if (Regex.IsMatch(item.tag, "\\W?UnRead\\W?")) item.isUnReaded = "0";
                        else item.isUnReaded = "1";
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
    /// Add a new bookmark.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Task<string> AddBookmark(string url, string description, string tag){
            return GetAsync(host + "/v1/posts/add?url=" + url + "&description=" + description + "&tags=" + tag + "&replace=yes").ContinueWith<string>(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    if (t.Result.StartsWith("Exception")) return null;
                    XDocument doc = XDocument.Parse(t.Result);
                    List<BookmarkItem> result = new List<BookmarkItem>();
                    XElement node = doc.Element("result");
                    return node.Attribute("code").Value;
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
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的字符串内容
        /// 字符串为空，表明请求失败。
        /// 字符串以Exception开头，表明请求出现异常，冒号后跟异常信息</returns>
        public Task<string> GetAsync(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(username, password);
                HttpClient client = new HttpClient(handler);
                return client.GetStringAsync(uri).ContinueWith<string>(t =>
                {
                    string result = "";
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        result = t.Result;
                    }
                    return result;
                });
            }
            catch (Exception e)
            {
                return new Task<string>(() => { return "Exception :" + e.Message; });
            }
        }

    }
}
