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

namespace ExtendRSS
{
    public partial class BrowserPage : PhoneApplicationPage
    {
        private string url;
        string googleUrl = "http://google.com/gwt/x?noimg=1&u=";
        string baiduUrl = "http://gate.baidu.com/tc?from=opentc&src=";
        ProgressIndicator proIndicator;
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
            Btn_NextPage = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            url = NavigationContext.QueryString["url"].ToString();
            url = googleUrl + url;
            webBrowser.Navigate(new Uri(url, UriKind.Absolute));
            Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
            Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
        }

        private void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            proIndicator.IsVisible = false;
            Btn_PrePage.IsEnabled = webBrowser.CanGoBack;
            Btn_NextPage.IsEnabled = webBrowser.CanGoForward;
        }

        private void webBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载网页";
        }

        private void Btn_GoogleEncode_Click(object sender, EventArgs e)
        {
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

        private void Btn_AddBookmark_Click(object sender, EventArgs e)
        {

        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity < 0)
            {
                string str = url;
                if (str.StartsWith(googleUrl))str = str.Remove(0, googleUrl.Length);
                if (str.StartsWith(baiduUrl)) str = str.Remove(0, baiduUrl.Length);
                NavigationService.Navigate(new Uri("/Views/NotePage.xaml?url=" + str, UriKind.Relative));
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