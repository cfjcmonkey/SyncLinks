﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using ExtendRSS.Models;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace ExtendRSS
{
    public partial class BrowserPage : PhoneApplicationPage
    {
        private string url, orgUrl;
        string googleUrl = "http://google.com/gwt/x?noimg=1&u=";
        string goolehost = "http://google.com";
        string baiduUrl = "http://gate.baidu.com/tc?from=opentc&src=";
        string baiduhost = "http://gate.baidu.com";
        ProgressIndicator proIndicator;
        BookmarkItem item;
        bool IsRefresh;

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
            Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
            Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
            Btn_Add.IsEnabled = true;
            AddBookmark_Popup.IsOpen = false;
            IsRefresh = false;
            
            orgUrl = NavigationContext.QueryString["url"].ToString();
            orgUrl = App.deliciousApi.ContentDecoder(orgUrl);
            item = App.deliciousApi.LoadLinkItemRecord(orgUrl);
            if (item.cacheHtml != null)
            {
                url = orgUrl;
                webBrowser.NavigateToString(item.cacheHtml);
            }
            else
            {
                url = googleUrl + orgUrl;
                webBrowser.Navigate(new Uri(url, UriKind.Absolute));
            }
        }

        private void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            proIndicator.IsVisible = false;
            Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
            Btn_NextPage.IsEnabled = webBrowser.CanGoForward;

            if (e.Uri != null && e.Uri.OriginalString.Length > 0)
            {
                url = e.Uri.OriginalString;
                if (url.StartsWith("about:"))
                {
                    url = url.Remove(0, "about:".Length);
                    if ((goolehost + url).StartsWith(googleUrl)) url = goolehost + url;
                    else if ((baiduhost + url).StartsWith(baiduUrl)) url = baiduhost + url;
                    webBrowser.Navigate(new Uri(url, UriKind.Absolute));
                }
            }

            string curUrl = url;
            if (curUrl.StartsWith(googleUrl)) curUrl = curUrl.Remove(0, googleUrl.Length);
            if (curUrl.StartsWith(baiduUrl)) curUrl = curUrl.Remove(0, baiduUrl.Length);
            Btn_Add.IsEnabled = !App.deliciousApi.IsExits(curUrl);

            if (item.cacheHtml == null || ( url.EndsWith(orgUrl) && IsRefresh))
            {
                item.cacheHtml = webBrowser.SaveToString();
                App.deliciousApi.SaveLinkItemRecord(item);
            }
            IsRefresh = false;
        }

        private void webBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载网页";
            Btn_Add.IsEnabled = false;
        }

        private void Btn_GoogleEncode_Click(object sender, EventArgs e)
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
                url = googleUrl + url;
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
                Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
                Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
            }
        }

        private void Btn_GoForward_Click(object sender, EventArgs e)
        {
            if (webBrowser.CanGoForward)
            {
                webBrowser.GoForward();
                Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
                Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
            }
        }

        /// <summary>
        /// 添加标签.
        /// 当前只添加标签到本地,不同步到网上.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            BookmarkItem item = App.deliciousApi.LoadLinkItemRecord(curUrl);
            if (item == null)
            {
                item = new BookmarkItem();
                item.extended = "";
            }
            item.cacheHtml = webBrowser.SaveToString();
            item.href = curUrl;
            item.isUnReaded = "1";
            item.tag = BookmarkItem.UNREAD;
            item.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            item.description = Txt_Title.Text;

            App.deliciousApi.SaveLinkItemRecord(item);
            MessageBox.Show("添加完成.");
            AddBookmark_Popup.IsOpen = false;

            if (App.deliciousApi.IsSycn())
            {
                //App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                //{
                //    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                //    {
                //        Dispatcher.BeginInvoke(() =>
                //        {
                //            //if done, do nothing
                //            if (t.Result != "done") MessageBox.Show(t.Result);
                //            else MessageBox.Show("添加完成.");
                //        });
                //    }
                //    else if (t.Status == TaskStatus.Faulted)
                //    {
                //        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                //    }
                //});
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            AddBookmark_Popup.IsOpen = false;
        }

        private void Btn_AddBookmark_Click(object sender, EventArgs e)
        {
            AddBookmark_Popup.IsOpen = true;
            try
            {
                XDocument doc = XDocument.Parse(webBrowser.SaveToString());
                string space = (doc.FirstNode as XElement).Name.NamespaceName;
                XElement ele = doc.Descendants("{" + space + "}" + "title").FirstOrDefault();
                Txt_Title.Text = ele.Value;
            }
            catch
            {
                Txt_Title.Text = "";
            }
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity < 0)
            {
                string tmp = App.deliciousApi.ContentEncoder(orgUrl);
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
                Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
                Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
                e.Cancel = true;
            }
        }
    }
}