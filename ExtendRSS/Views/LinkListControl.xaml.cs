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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SyncLinks.Views
{
    public partial class LinkListControl : UserControl
    {
        IndexPage parent; //用systemstray...代替
        ObservableCollection<BookmarkItem> itemList;
        int total
        {
            get
            {
                return itemList.Count;
            }
        }
        public IndexPage.PageStatus StatusTag { get; set; }
        bool IsUpdateOnline { get; set; }


        public LinkListControl(IndexPage page)
        {
            InitializeComponent();

            parent = page;
            IsUpdateOnline = true;
            //App.localFileCache.ListIndexChanged += LocalFileCache_ListIndexChanged;
            //App.pocketApi.ItemStatusChanged += PocketApi_ItemStatusChanged;
            
            ////////////////////////TEST
            //if (StatusTag == IndexPage.PageStatus.UNREAD)
            //{
            //    App.localFileCache.AddBunchItemRecords(App.pocketApi.TestData(), 0);
            //}
            ///////////////////////
        }

        ~LinkListControl()
        {
            //App.localFileCache.ListIndexChanged -= LocalFileCache_ListIndexChanged;
            //App.pocketApi.ItemStatusChanged -= PocketApi_ItemStatusChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            itemList = App.localFileCache.LoadItemList(StatusTag);
            ResultListControl.ItemsSource = itemList;

            UpdateData();
        }

        public void UpdateData(bool isForce = false)
        {
            IsUpdateOnline |= isForce;
            //OfflineLoad(true);
            if (IsUpdateOnline)
            {
                if (total == 0) OnlineLoadUI(0, 10);
                else OnlineLoadUI(0, -1); //TEST
            }
        }

        public void LocalRefresh()
        {
            OfflineLoad(true);
        }

        //private void UpdateUI()
        //{
        //    ResultListControl.ItemsSource = null;
        //    ResultListControl.ItemsSource = itemList;
        //}

        //private void LocalFileCache_ListIndexChanged(IndexPage.PageStatus pageStatus)
        //{
        //    Dispatcher.BeginInvoke(() => {
        //        //判定当前是否显示在页面上
        //        if (parent.IsSelected(StatusTag))
        //        {
        //            if (pageStatus == StatusTag) OfflineLoad(true);
        //        }
        //    });
        //}

        //private void PocketApi_ItemStatusChanged(object sender, BookmarkItem item, IndexPage.PageStatus pageStatus)
        //{
        //    //Update itemList, do not need to update UI.
        //    if (StatusTag == pageStatus) AddItem2UI(item);
        //    else RemoveItem4UI(item);
        //}

        /// <summary>
        /// 加载本地的链接信息
        /// 绑定数据和界面，并更新界面
        /// </summary>
        /// <param name="tag">加载包含指定tag的链接;若tag不可识别,则加载所有链接</param>
        private void OfflineLoad(bool isUpdateUI = true)
        {
            parent.proIndicator.IsVisible = true;
            parent.proIndicator.Text = "正在加载书签...";

            App.localFileCache.ReLoadItemList(StatusTag);

            //if (isUpdateUI) UpdateUI();
            parent.proIndicator.IsVisible = false;
        }


        /// <summary>
        /// 加载服务器的链接信息
        /// 绑定数据和界面，并更新界面
        /// </summary>
        private void OnlineLoadUI(int offset, int count = -1)
        {
            if (StatusTag == IndexPage.PageStatus.RECENT) return;

            parent.proIndicator.IsVisible = true;
            parent.proIndicator.Text = "正在加载书签...";

            OnlineLoad(offset, count);
        }

        private void OnlineLoad(int offset, int count = -1)
        {
            int defaultCount = 10;
            Task<int> task = null;
            if (StatusTag == IndexPage.PageStatus.UNREAD) task = App.pocketApi.GetUnRead(offset, count < 0 ? defaultCount : count);
            else if (StatusTag == IndexPage.PageStatus.READ) task = App.pocketApi.GetArchive(offset, count < 0 ? defaultCount : count);
            else if (StatusTag == IndexPage.PageStatus.STAR) task = App.pocketApi.GetStar(offset, count < 0 ? defaultCount : count);
            else return;

            task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    if (count == -1 && t.Result == defaultCount)
                    {
                        OnlineLoad(offset + defaultCount, -1);
                    }
                    else
                    {
                        IsUpdateOnline = false;
                        Dispatcher.BeginInvoke(() =>
                        {
                            parent.proIndicator.IsVisible = false;
                        });
                    }
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    IsUpdateOnline = false;
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (App.pocketApi.IsSync)
                        {
                            if (t.Exception.InnerException.Message.Contains("401 (Unauthorized)"))
                            {
                                var boxresult = MessageBox.Show("未登录或登录过期，现在去登录？", "", MessageBoxButton.OKCancel);
                                if (boxresult == MessageBoxResult.OK)
                                {
                                    parent.NavigationService.Navigate(new Uri("/Views/ConfigPage.xaml", UriKind.Relative));
                                }
                            }
                            else
                            {
                                MessageBox.Show("请求错误: " + t.Exception.InnerException.Message + "\n" + t.Exception.InnerException.StackTrace);
                            }
                            App.pocketApi.IsSync = false;
                        }
                        parent.proIndicator.IsVisible = false;
                    });
                }
            });
        }

        #region triggered events
        /// <summary>
        /// 点击按钮，进入链接阅读
        /// </summary>
        private void ItemBtn_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BookmarkItem it = (sender as Button).DataContext as BookmarkItem;
            if (it.isUnReaded) it.isUnReaded = false;
            string url = LocalFileCache.ContentEncoder(it.href);
            parent.NavigationService.Navigate(new Uri("/Views/BrowserPage.xaml?url=" + url, UriKind.Relative));
        }
        /// <summary>
        /// 点击右键菜单项，标为已读或未读
        /// </summary>
        private void ReadMenuItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BookmarkItem mitem = (sender as MenuItem).DataContext as BookmarkItem;
            if (mitem.isUnReaded) mitem.isUnReaded = false;
            else mitem.isUnReaded = true;
        }
        /// <summary>
        /// 点击右键菜单项，标记收藏
        /// </summary>
        private void StarMenuItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BookmarkItem sitem = (sender as MenuItem).DataContext as BookmarkItem;
            if (sitem.isStar == false) sitem.isStar = true;
        }
        /// <summary>
        /// 点击右键菜单项，删除链接项目
        /// </summary>
        private void DeleteMenuItem_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BookmarkItem sitem = (sender as MenuItem).DataContext as BookmarkItem;
            App.localFileCache.DeleteBookmark(sitem);
            App.pocketApi.DeleteItem(sitem);
            //UpdateUI();
        }

        /// <summary>
        /// 拉动更新
        /// </summary>
        private void refreshPanel_RefreshRequested(object sender, EventArgs e)
        {
            OnlineLoadUI(total, 10);
        }
        #endregion triggered events
    }
}
