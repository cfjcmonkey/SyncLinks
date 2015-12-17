using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics;
using System.Net;
using System.Windows.Threading;
using Microsoft.Phone.Shell;
using System.Windows;

namespace SyncLinks.Models
{
    /// <summary>
    /// 考虑将添加和更新的在线操作与离线操作异步执行。
    /// 在线操作放入同步队列中，类似onenote，异步地执行；还可以将失败的操作加入队列中，下一次执行。
    /// 
    /// 当前实现将抛出所有异常，由前端处理。
    /// </summary>
    public class PocketAPI
    {
        static readonly string consumer_key = "48664-97ee937a868f84d4c422db40";      
        static readonly string redirect_uri = "synclinks:authorizationFinished";
        static readonly string host = "https://getpocket.com";
        Preference preference;
        Regex tokenCodeReg = new Regex("\"code\":\"(.+?)\"", RegexOptions.Compiled);
        Regex accessTokenReg = new Regex("\"access_token\":\"(.+?)\",\"username\":\"(.+?)\"", RegexOptions.Compiled);
        Regex statusReg = new Regex("\"status\":(\\d+)", RegexOptions.Compiled);

        DataContractJsonSerializer pocketItem_ser = new DataContractJsonSerializer(typeof(PocketItem));
        DataContractJsonSerializer getResponsePackage_ser = 
            new DataContractJsonSerializer(typeof(GetResponsePackage),
                                            new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
        DataContractJsonSerializer tokenRequestPackage_ser = new DataContractJsonSerializer(typeof(TokenRequestPackage));

        public string RequestToken { get { return preference.RequestToken; } }
        public string UserName { get { return preference.Username; } }
        public string AccessToken { get { return preference.AccessToken; } }

        public bool IsSync { get; set; }

        public PocketAPI()
        {
            preference = new Preference();
            IsSync = true;
        }

        #region Authorization
        /// <summary>
        /// 重设用户名和密码.
        /// 由Preference管理本地的存储，因此不用调用SavePreference来保存.
        /// </summary>
        void SetAccount(string user, string token)
        {
            preference.Username = user;
            preference.AccessToken = token;
        }
        /// <summary>
        /// 获取RequestToken，并跳转到系统浏览器，引导用户授权
        /// </summary>
        public void AssignAuthority()
        {
            GetRequestToken().ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    preference.RequestToken = t.Result;
                    RedirectUser2Authorization();
                }
                else
                {
                    Debug.WriteLine("Error: wrong t in GetRequestToken {0}", t.Exception.InnerException.Message);
                }
            });
        } 

        async Task<string> GetRequestToken()
        {
            string url = host + "/v3/oauth/request";
            string data = "{\"consumer_key\":\"" + consumer_key + "\",\"redirect_uri\":\"" + redirect_uri + "\"}";
            string result = await HttpTool.PostStringAsync(url, data);
            var match = tokenCodeReg.Match(result);
            if (match.Success) return match.Groups[1].Value;
            else return "";
        }

        void RedirectUser2Authorization()
        {
            string url = String.Format("{0}/auth/authorize?request_token={1}&redirect_uri={2}&mobile=1",
                host, RequestToken, redirect_uri);
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(url);
            webBrowserTask.Show();
        }

        /// <summary>
        /// 获取AccessToken，需要有RequestToken
        /// </summary>
        public async Task<bool> GetAccessToken()
        {
            string url = host + "/v3/oauth/authorize";
            string data = "{\"consumer_key\":\"" + consumer_key + "\",\"code\":\"" + RequestToken + "\"}";
            string result = await HttpTool.PostStringAsync(url, data);
            var match = accessTokenReg.Match(result);
            if (match.Success)
            {
                SetAccount(match.Groups[2].Value, match.Groups[1].Value);
                return true;
            }else return false;
        }
        #endregion Authorization

        async Task<List<PocketItem>> GetPocketList(int start, int count, bool isDetail = true, bool ? isRead = null, bool? isFavorite = null)
        {
            string url = host + "/v3/get";
            FakeJsonBuilder jsonSB = new FakeJsonBuilder();
            jsonSB.AppendItem("consumer_key", consumer_key);
            jsonSB.AppendItem("access_token", AccessToken);
            jsonSB.AppendItem("offset", start);
            jsonSB.AppendItem("count", count);
            jsonSB.AppendItem("detailType", isDetail ? "complete" : "simple");
            jsonSB.AppendItem("state", isRead == null ? "all" : (isRead == true ? "archive" : "unread"));
            if (isFavorite != null) jsonSB.AppendItem("favorite", isFavorite == true ? "1" : "0");

            string data = jsonSB.Output();
            //Stream result = await HttpTool.PostStreamAsync(url, data);
            //GetResponsePackage response = getResponsePackage_ser.ReadObject(result) as GetResponsePackage;
            //result.Close();
            string result = await HttpTool.PostStringAsync(url, data);
            GetResponsePackage response = null;
            var match = statusReg.Match(result);
            if (match.Success && match.Groups[1].Value == "1")
                response = Newtonsoft.Json.JsonConvert.DeserializeObject<GetResponsePackage>(result);
            else Debug.WriteLine("Warning in GetPocketList: not recognized json string = {0}", result);
            IsSync = true;
            return response == null ? new List<PocketItem>() : response.list.Values.ToList();
        }

        async void ModifyItem(string item_id, string action)
        {
            //return; // TEST

            string url = host + "/v3/send";
            FakeJsonBuilder jsonSB = new FakeJsonBuilder();
            jsonSB.AppendItem("action", action);
            jsonSB.AppendItem("item_id", item_id);
//            string time = "";
            string content = "[" + jsonSB.Output() + "]";
            content = HttpUtility.UrlEncode(content);
            url += String.Format("?actions={0}&access_token={1}&consumer_key={2}", content, AccessToken, consumer_key);
            string result = await HttpTool.PostStringAsync(url, "");
            var match = statusReg.Match(result);
            if (!(match.Success && match.Groups[1].Value == "1"))
                Debug.WriteLine("Error in ModifyItem, result = {0}", result);
            IsSync = true;
        }

        async void AddItem(string contentUrl)
        {
            string url = host + "/v3/add";
            FakeJsonBuilder jsonSB = new FakeJsonBuilder();
            jsonSB.AppendItem("consumer_key", consumer_key);
            jsonSB.AppendItem("access_token", AccessToken);
            jsonSB.AppendItem("url", contentUrl);
            string data = jsonSB.Output();
            string result = await HttpTool.PostStringAsync(url, data);
            var match = statusReg.Match(result);
            if (!(match.Success && match.Groups[1].Value == "1"))
                Debug.WriteLine("Error in AddItem, result = {0}", result);
            IsSync = true;
        }

        /// <summary>返回添加的新元素的个数</summary>
        public async Task<int> GetUnRead(int start, int count)
        {
            var result = await GetPocketList(start, count, true, false);
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            foreach (var item in result)
            {
                var newItem = PocketItem2BookmarkItem(item, true);
                if (newItem != null) itemList.Add(newItem);
            }
            return App.localFileCache.AddBunchItemRecords(itemList, start);
        }

        /// <summary>返回添加的新元素的个数</summary>
        public async Task<int> GetArchive(int start, int count)
        {
            var result = await GetPocketList(start, count, true, true);
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            foreach (var item in result)
            {
                var newItem = PocketItem2BookmarkItem(item, false);
                if (newItem != null) itemList.Add(newItem);
            }
            return App.localFileCache.AddBunchItemRecords(itemList, start);
        }

        /// <summary>返回添加的新元素的个数</summary>
        public async Task<int> GetStar(int start, int count)
        {
            var result = await GetPocketList(start, count, true, null, true);
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            foreach (var item in result)
            {
                var newItem = PocketItem2BookmarkItem(item, null, true);
                if (newItem != null) itemList.Add(newItem);
            }
            return App.localFileCache.AddBunchItemRecords(itemList, start);
        }

        public void AddNewItem (BookmarkItem item)
        {
            if (ItemStatusChanged != null) ItemStatusChanged(this, item, Views.IndexPage.PageStatus.UNREAD);
            AddItem(item.href);
        }

        public void DeleteItem(BookmarkItem item)
        {
            if (ItemStatusChanged != null) ItemStatusChanged(this, item, Views.IndexPage.PageStatus.OTHER);
            ModifyItem(item.hash, "delete");
        }

        public void SetUnRead(BookmarkItem item)
        {
            if (ItemStatusChanged != null) ItemStatusChanged(this, item, Views.IndexPage.PageStatus.UNREAD);
            ModifyItem(item.hash, "readd");
        }

        public void SetArchive(BookmarkItem item)
        {
            if (ItemStatusChanged != null) ItemStatusChanged(this, item, Views.IndexPage.PageStatus.READ);
            ModifyItem(item.hash, "archive");
        } 

        public void SetUnFavorite(BookmarkItem item)
        {
            ModifyItem(item.hash, "unfavorite");
        }

        public void SetFavorite(BookmarkItem item)
        {
            if (ItemStatusChanged != null) ItemStatusChanged(this, item, Views.IndexPage.PageStatus.STAR);
            ModifyItem(item.hash, "favorite");
        }

        public BookmarkItem PocketItem2BookmarkItem(PocketItem item, bool? isUnReaded = null, bool? isStar = null)
        {
            BookmarkItem newItem = new BookmarkItem(
                String.IsNullOrEmpty(item.resolved_url) ? item.given_url : item.resolved_url,
                String.IsNullOrEmpty(item.given_title) ? item.resolved_title : item.given_title,
                item.item_id,
                (isUnReaded == null) ? (item.status == "0") : (isUnReaded == true),
                (isStar == null) ? (item.favorite == "1") : (isStar == true)
            );
            if (String.IsNullOrEmpty(newItem.href))
            {
                newItem.Dispose();
                return null;
            }
            return newItem;
        }

        public void BookmarkItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var item = sender as BookmarkItem;
            if (e.PropertyName == "isUnReaded")
            {
                if (item.isUnReaded) SetUnRead(item);
                else SetArchive(item);
            }else if (e.PropertyName == "isStar")
            {
                if (item.isStar) SetFavorite(item);
                else SetUnFavorite(item);
            }
        }

        public delegate void ItemStatusChangedEventHandler(object sender, BookmarkItem item, Views.IndexPage.PageStatus pageStatus);
        public event ItemStatusChangedEventHandler ItemStatusChanged;

        #region Test
        public List<BookmarkItem> TestData()
        {
            var result = App.GetResourceStream(new Uri("Models/test.txt", UriKind.Relative)).Stream;
            StreamReader reader = new StreamReader(result);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<GetResponsePackage>(reader.ReadToEnd());            
            //GetResponsePackage response = getResponsePackage_ser.ReadObject(result) as GetResponsePackage;
            result.Close();
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            foreach (var item in response.list.Values)
            {
                var newItem = PocketItem2BookmarkItem(item);
                newItem.PropertyChanged += BookmarkItem_PropertyChanged;
                itemList.Add(newItem);
            }
            return itemList;
        }

        public List<BookmarkItem> TestDatav2()
        {
            List<BookmarkItem> itemList = new List<BookmarkItem>();
            BookmarkItem item = new BookmarkItem(
                "http://www.baidu.com",
                "Baidu",
                "123456",
                true,
                false
            );
            itemList.Add(item);

            item = new BookmarkItem(
                "http://www.google.com",
                "Google",
                "654321",
                true,
                false
            );
            itemList.Add(item);

            return itemList;
        }
        #endregion Test
    }

    public class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;
        public override Uri MapUri(Uri uri)
        {
            tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());
            // URI association launch for contoso.
            if (tempUri.Contains("synclinks:authorizationFinished"))
            {
                return new Uri("/Views/ConfigPage.xaml?AuthorizationFinished=1", UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }

    public class FakeJsonBuilder
    {
        StringBuilder sb = new StringBuilder();

        public void AppendItem(string key, Object value)
        {
            if (sb.Length == 0) sb.Append("{");
            else sb.Append(",");
            if ((value is Int32) || (value is Double) || (value is Boolean))
                sb.Append("\"" + key + "\":" + value);
            else sb.Append("\"" + key + "\":\"" + value + "\""); 
        }

        public string Output()
        {
            if (sb.Length == 0) return "";
            else return sb.ToString() + "}";
        }

        public void Clear()
        {
            sb.Clear();
        }
    }
}
