using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SyncLinks.Models;

namespace SyncLinks.Views
{
    public partial class NotePage : PhoneApplicationPage
    {
        private string url;

        public NotePage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            url = NavigationContext.QueryString["url"].ToString();
            url = LocalFileCache.ContentDecoder(url);
            string text = LocalFileCache.LoadNote(url);
//            BookmarkItem item = App.deliciousApi.LoadLinkItemRecord(url);
//            string text = item.extended;
            if (text == null || text.Length == 0) text = url + "\n\n";
            Txt_NoteContent.Text = text;
            Txt_NoteContent.Focus();
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity > 0)
            {
                if (App.RootFrame.CanGoBack) App.RootFrame.GoBack();
                else
                {
                    string tmp = LocalFileCache.ContentEncoder(url);
                    NavigationService.Navigate(new Uri("/Views/BrowserPage.xaml?url=" + tmp, UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// BeginTransaction
        /// 保存笔记.
        /// 当前只保存笔记到本地.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationOutTransition_BeginTransition(object sender, RoutedEventArgs e)
        {
            LocalFileCache.SaveNote(url, Txt_NoteContent.Text);

            BookmarkItem item = App.localFileCache.GetBookmarkItem(url);
            item.extended = Txt_NoteContent.Text;
                //App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                //{
                //    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                //    {
                //        Dispatcher.BeginInvoke(() =>
                //        {
                //            //if done, do nothing
                //            if (t.Result != "done") MessageBox.Show("同步失败...检查网络或用户名和密码");
                //        });
                //    }
                //    else if (t.Status == TaskStatus.Faulted)
                //    {
                //        Dispatcher.BeginInvoke(() => { MessageBox.Show("同步失败...检查网络或用户名和密码"); });
                //    }
                //});
        }

    }
}