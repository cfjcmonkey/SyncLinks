using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ExtendRSS.Models;

namespace ExtendRSS.Views
{
    public partial class IndexPage : PhoneApplicationPage
    {
        public ProgressIndicator proIndicator;
        public IndexPage()
        {
            InitializeComponent();

            proIndicator = new ProgressIndicator()
            {
                IsIndeterminate = true,
                IsVisible = false
            };
            SystemTray.SetProgressIndicator(this, proIndicator);

            Login_Content.Width = Application.Current.Host.Content.ActualWidth;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] == UnReadedViewer)
            {
                if (UnReadedViewer.Content != null) (UnReadedViewer.Content as LinkListControl).LocalRefresh();
                UnReadedViewer.Content = new LinkListControl(this) { StatusTag = BookmarkItem.UNREAD };
            }
            else if (e.AddedItems[0] == ReadedViewer)
            {
                if (ReadedViewer.Content != null) (ReadedViewer.Content as LinkListControl).LocalRefresh();
                ReadedViewer.Content = new LinkListControl(this) { StatusTag = BookmarkItem.READ };
            }
            else if (e.AddedItems[0] == RecentViewer)
            {
                if (RecentViewer.Content != null) (RecentViewer.Content as LinkListControl).LocalRefresh();
                RecentViewer.Content = new LinkListControl(this);
            }
            else if (e.AddedItems[0] == StarViewer)
            {
                if (StarViewer.Content != null) (StarViewer.Content as LinkListControl).LocalRefresh();
                StarViewer.Content = new LinkListControl(this) { StatusTag = BookmarkItem.STAR };
            }
        }

        /// <summary>
        /// 登陆按钮的响应，登陆成功后显示最近的书签
        /// </summary>
        private void Btn_Login_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SetAccount(Txt_Username.Text, Txt_Password.Password);
            Login_Popup.IsOpen = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Login_Popup.IsOpen = false;
        }

        /// <summary>
        /// 设置按钮的响应,弹出登录框
        /// </summary>
        private void AppBarIconButton_Set_Click(object sender, EventArgs e)
        {
            Login_Popup.IsOpen = true;
        }

        /// <summary>
        /// 重载回退键,若登陆框打开，优先关闭登陆框.
        /// </summary>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Login_Popup.IsOpen)
            {
                Login_Popup.IsOpen = false;
                e.Cancel = true;
            }
        }

        private void AppBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            if (PivotControl.SelectedItem == UnReadedViewer)
            {
                UnReadedViewer.Content = new LinkListControl(this) { StatusTag = BookmarkItem.UNREAD };
            }
            else if (PivotControl.SelectedItem == ReadedViewer)
            {
                ReadedViewer.Content = new LinkListControl(this) { StatusTag = BookmarkItem.READ };
            }
            else if (PivotControl.SelectedItem == RecentViewer)
            {
                RecentViewer.Content = new LinkListControl(this);
            }
        }

    }
}