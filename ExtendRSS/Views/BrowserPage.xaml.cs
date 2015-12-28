using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SyncLinks.Models;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SyncLinks
{
    public partial class BrowserPage : PhoneApplicationPage
    {
        static string googleUrl = "http://google.com/gwt/x?noimg=1&u=";
        static string googlehost = "http://google.com";
        static string baiduUrl = "http://gate.baidu.com/tc?from=opentc&src=";
        static string baiduhost = "http://gate.baidu.com";
        static Regex regUrlHead = new Regex(@"https?://(m|www)\.", RegexOptions.Compiled);

        ProgressIndicator proIndicator;
        BookmarkItem item;
        bool IsRefresh;
        string url;
        string orgUrl { get { return item == null ? "" : item.href; } }

        public bool CanGoBack { get { return webBrowser.CanGoBack; } }
        public bool CanGoForward { get { return webBrowser.CanGoForward; } }
        public bool IsNotAdded { get { return item != null && !LocalFileCache.IsExits(item.href); } }

        public BrowserPage()
        {
            InitializeComponent();
            proIndicator = new ProgressIndicator()
            {
                IsIndeterminate = true,
                IsVisible = false
            };
            SystemTray.SetProgressIndicator(this, proIndicator);

            Btn_PrePage = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            Btn_NextPage = ApplicationBar.Buttons[3] as ApplicationBarIconButton;
            Btn_Add = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Btn_PrePage.IsEnabled = CanGoBack;
            Btn_NextPage.IsEnabled = CanGoForward;
            Btn_Add.IsEnabled = IsNotAdded;
            AddBookmark_Popup.IsOpen = false;
            IsRefresh = false;
            
            url = NavigationContext.QueryString["url"].ToString();
            url = LocalFileCache.ContentDecoder(url);
            item = App.localFileCache.GetBookmarkItem(url);
            if (!String.IsNullOrEmpty(item.cacheHtml))
            {
                webBrowser.NavigateToString(item.cacheHtml);
            }
            else
            {
                //url = baiduUrl + url;
                webBrowser.Navigate(new Uri(url, UriKind.Absolute));
            }
            App.localFileCache.UpdateRecentIndex(item);
        }

        private void webBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "加载" + e.Uri == null ? url : e.Uri.OriginalString;
        }

        private void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            proIndicator.IsVisible = false;

            if (e.Uri != null && e.Uri.OriginalString.Length > 0)
            {
                url = e.Uri.OriginalString;
                if (url.StartsWith("about:"))
                {
                    url = url.Remove(0, "about:".Length);
                    if ((googlehost + url).StartsWith(googleUrl)) url = googlehost + url;
                    else if ((baiduhost + url).StartsWith(baiduUrl)) url = baiduhost + url;
                    webBrowser.Navigate(new Uri(url, UriKind.Absolute));
                    return;
                }
            }

            if (UrlEqual(url, item.href) && (String.IsNullOrEmpty(item.cacheHtml) || IsRefresh))
            {
                item.cacheHtml = webBrowser.SaveToString();
            }
            IsRefresh = false;
            Btn_PrePage.IsEnabled = CanGoBack;
            Btn_NextPage.IsEnabled = CanGoForward;
            Btn_Add.IsEnabled = IsNotAdded;
        }


        bool CleanUrl(ref string paramUrl)
        {
            bool isChange = false;
            if (paramUrl.StartsWith(googleUrl))
            {
                paramUrl = paramUrl.Remove(0, googleUrl.Length);
                isChange = true;
            }
            if (paramUrl.StartsWith(baiduUrl))
            {
                paramUrl = paramUrl.Remove(0, baiduUrl.Length);
                isChange = true;
            }
            return isChange;
        }

        bool UrlEqual(string url1, string url2)
        {
            CleanUrl(ref url1);
            CleanUrl(ref url2);
            url1 = regUrlHead.Replace(url1, "").Trim().ToLower();
            url2 = regUrlHead.Replace(url2, "").Trim().ToLower();
            return url1 == url2;
        }

        private void Btn_PageEncode_Click(object sender, EventArgs e)
        {
            IsRefresh = true;
            if (url.StartsWith(googleUrl) == true)
            {
                url = url.Remove(0, googleUrl.Length);
                url = baiduUrl + url;
//                AppMenu_GoogleTrans.Text = "加载原网页";
            }
            else if (url.StartsWith(baiduUrl) == true)
            {
                url = url.Remove(0, baiduUrl.Length);
//                AppMenu_GoogleTrans.Text = "谷歌转码"; //引用为空的报错
            }
            else
            {
                url = baiduUrl + url;
            }
            webBrowser.Navigate(new Uri(url));
        }

        private void Btn_Fresh_Click(object sender, EventArgs e)
        {
            IsRefresh = true;
            webBrowser.Navigate(new Uri(url, UriKind.Absolute));
        }

        private void Btn_GoBack_Click(object sender, EventArgs e)
        {
            if (webBrowser.CanGoBack)
            {
                webBrowser.GoBack();
            }
        }

        private void Btn_GoForward_Click(object sender, EventArgs e)
        {
            if (webBrowser.CanGoForward)
            {
                webBrowser.GoForward();
            }
        }

        /// <summary>
        /// 添加链接.
        /// </summary>
        private void Btn_OK_Click(object sender, EventArgs e)
        {
            string curUrl = url;
            if (curUrl.StartsWith(googleUrl))
            {
                curUrl = curUrl.Remove(0, googleUrl.Length);
                int i = curUrl.IndexOf("&");
                if (i != -1) curUrl = curUrl.Remove(i);
            }
            if (curUrl.StartsWith(baiduUrl))
            {
                curUrl = curUrl.Remove(0, baiduUrl.Length);
                int i = curUrl.IndexOf("&");
                if (i != -1) curUrl = curUrl.Remove(i);
            }

            var item = App.localFileCache.AddBookmarkItem(curUrl, Txt_Title.Text, webBrowser.SaveToString());
            App.pocketApi.AddNewItem(item);
            MessageBox.Show("添加完成.");
            AddBookmark_Popup.IsOpen = false;
            Btn_Add.IsEnabled = IsNotAdded;
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            AddBookmark_Popup.IsOpen = false;
        }

        private void Btn_AddBookmark_Click(object sender, EventArgs e)
        {
            AddBookmark_Popup.IsOpen = true;
            XDocument doc = XDocument.Parse(webBrowser.SaveToString());
            string space = (doc.FirstNode as XElement).Name.NamespaceName;
            var results = doc.Descendants("{" + space + "}" + "title");
            if (results != null && results.Count<XElement>() > 0)
                Txt_Title.Text = results.First<XElement>().Value;
            else Txt_Title.Text = "";
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity < 0)
            {
                string tmp = LocalFileCache.ContentEncoder(orgUrl);
                NavigationService.Navigate(new Uri("/Views/NotePage.xaml?url=" + tmp, UriKind.Relative));
            }
        }

        private void AppMenu_IE10_Click(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();            
            webBrowserTask.Uri = new Uri(url, UriKind.Absolute);
            webBrowserTask.Show();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (webBrowser.CanGoBack)
            {
                webBrowser.GoBack();
                e.Cancel = true;
            }
        }
    }
}