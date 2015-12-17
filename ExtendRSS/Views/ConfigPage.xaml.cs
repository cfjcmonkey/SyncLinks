using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SyncLinks.Models;

namespace SyncLinks.Views
{
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();

            ProgressIndicator indicator = new ProgressIndicator()
            {
                IsVisible = false,
                IsIndeterminate = true,
                Text = "正在联系服务器"
            };
            SystemTray.SetProgressIndicator(this, indicator);

            this.Loaded += ConfigPage_Loaded;
        }

        private void ConfigPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("AuthorizationFinished"))
            {
                App.pocketApi.GetAccessToken().ContinueWith(t =>
                {
                    if (t != null && t.Result == true)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            SystemTray.ProgressIndicator.IsVisible = false;
                            App.pocketApi.IsSync = true;
                            MessageBox.Show("完成.");
                            NavigationService.Navigate(new Uri("/Views/IndexPage.xaml", UriKind.Relative));
                        });
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            SystemTray.ProgressIndicator.IsVisible = false;
                            MessageBox.Show("授权失败.");
                        });
                    }
                });
            }
        }

        private void SetAccount_Click(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator.IsVisible = true;
            App.pocketApi.AssignAuthority();
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            App.localFileCache.ClearStorage();
            MessageBox.Show("完成.");
        }

        /// <summary>
        /// 同步的内容有标签状态和笔记.
        /// 关闭情况下，依旧会从网络抓取最新的标签.
        /// 即，同步关闭时只读取;开启时可以更改网络信息.
        /// </summary>
        private void Sycn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}