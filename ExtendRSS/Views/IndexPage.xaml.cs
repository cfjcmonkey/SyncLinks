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
using SyncLinks.Models;

namespace SyncLinks.Views
{
    public partial class IndexPage : PhoneApplicationPage
    {
        public enum PageStatus { UNREAD, READ, STAR, RECENT, OTHER}

        public ProgressIndicator proIndicator;
        public IndexPage()
        {
            InitializeComponent();
            //this.Loaded += IndexPage_Loaded;
            proIndicator = new ProgressIndicator()
            {
                IsIndeterminate = true,
                IsVisible = false
            };
            SystemTray.SetProgressIndicator(this, proIndicator);
            AddBookmark_Popup.IsOpen = false;
//            Login_Content.Width = Application.Current.Host.Content.ActualWidth;
        }

        //private void IndexPage_Loaded(object sender, RoutedEventArgs e)
        //{
            
        //}

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = e.RemovedItems[0] as PivotItem;
            //if (pivot != null && pivot.Content != null) (pivot.Content as LinkListControl).SaveCurrentIndex();
            UpdatePage(e.AddedItems[0] as PivotItem);
        }

        public bool IsSelected(PageStatus pageStatus)
        {
            var item = PivotControl.SelectedItem as PivotItem;
            if (item == UnReadedViewer) return pageStatus == PageStatus.UNREAD;
            else if (item == ReadedViewer) return pageStatus == PageStatus.READ;
            else if (item == RecentViewer) return pageStatus == PageStatus.RECENT;
            else if (item == StarViewer) return pageStatus == PageStatus.STAR;
            else return false;
        }

        void UpdatePage(PivotItem item, bool isForce = false)
        {
            if (item == UnReadedViewer)
            {
                if (UnReadedViewer.Content != null) (UnReadedViewer.Content as LinkListControl).UpdateData(isForce);
                else UnReadedViewer.Content = new LinkListControl(this) { StatusTag = PageStatus.UNREAD };
            }
            else if (item == ReadedViewer)
            {
                if (ReadedViewer.Content != null) (ReadedViewer.Content as LinkListControl).UpdateData(isForce);
                else ReadedViewer.Content = new LinkListControl(this) { StatusTag = PageStatus.READ };
            }
            else if (item == RecentViewer)
            {
                if (RecentViewer.Content != null) (RecentViewer.Content as LinkListControl).LocalRefresh();
                else RecentViewer.Content = new LinkListControl(this) { StatusTag = PageStatus.RECENT };
            }
            else if (item == StarViewer)
            {
                if (StarViewer.Content != null) (StarViewer.Content as LinkListControl).UpdateData(isForce);
                else StarViewer.Content = new LinkListControl(this) { StatusTag = PageStatus.STAR };
            }
        }

        /// <summary>
        /// 点击设置按钮，转到设置页面
        /// </summary>
        private void AppBarIconButton_Set_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ConfigPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 点击刷新按钮，更新当前页面，有网络连接
        /// why not use refresh instead of new object?
        /// </summary>
        private void AppBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            UpdatePage(PivotControl.SelectedItem as PivotItem, true);
        }

        private void AppBarIconButton_Add_Click(object sender, EventArgs e)
        {
            AddBookmark_Popup.IsOpen = true;
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            var item = App.localFileCache.AddBookmarkItem(Txt_URL.Text, Txt_URL.Text);
            App.pocketApi.AddNewItem(item);
            MessageBox.Show("添加完成.");
            AddBookmark_Popup.IsOpen = false;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            AddBookmark_Popup.IsOpen = false;
        }

        ///// <summary>
        ///// 登陆按钮的响应，登陆成功后显示最近的书签
        ///// </summary>
        //private void Btn_Login_Click(object sender, RoutedEventArgs e)
        //{
        //    App.deliciousApi.SetAccount(Txt_Username.Text, Txt_Password.Password);
        //    Login_Popup.IsOpen = false;
        //}

        //private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    Login_Popup.IsOpen = false;
        //}

        ///// <summary>
        ///// 重载回退键,若登陆框打开，优先关闭登陆框.
        ///// </summary>
        //protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        //{
        //    //if (Login_Popup.IsOpen)
        //    //{
        //    //    Login_Popup.IsOpen = false;
        //    //    e.Cancel = true;
        //    //}
        //}

    }
}