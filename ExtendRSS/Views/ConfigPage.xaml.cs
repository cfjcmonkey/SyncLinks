using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ExtendRSS.Views
{
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();
            if (App.deliciousApi.IsSycn()) Sycn.Content = "同步状态:开启";
            else Sycn.Content = "同步状态:关闭";
        }

        private void SetAccount_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SetAccount(Txt_Username.Text, Txt_Password.Password);
            MessageBox.Show("完成.");
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.ClearStorage();
            MessageBox.Show("完成.");
        }

        /// <summary>
        /// 同步的内容有标签状态和笔记.
        /// 关闭情况下，依旧会从网络抓取最新的标签.
        /// 即，同步关闭时只读取;开启时可以更改网络信息.
        /// </summary>
        private void Sycn_Click(object sender, RoutedEventArgs e)
        {
            if (App.deliciousApi.IsSycn())
            {
                Sycn.Content = "同步状态:关闭";
                App.deliciousApi.SetSycn(false);
            }
            else
            {
                Sycn.Content = "同步状态:开启";
                App.deliciousApi.SetSycn(true);
            }
        }
    }
}