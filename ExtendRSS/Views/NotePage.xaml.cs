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
using ExtendRSS.Models;

namespace ExtendRSS.Views
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
//            string text = App.deliciousApi.LoadNote(url);
            BookmarkItem item = App.deliciousApi.LoadLinkItemRecord(url);
            string text;
            if (item == null || item.extended.Length == 0) text = url + "\n\n";
            else text = item.extended;
            Txt_NoteContent.Text = text;
            Txt_NoteContent.Focus();
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity > 0)
            {
                if (App.RootFrame.CanGoBack) App.RootFrame.GoBack();
                else NavigationService.Navigate(new Uri("/Views/BrowserPage.xaml?url=" + url, UriKind.Relative));
            }
        }

        /// <summary>
        /// BeginTransaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationOutTransition_BeginTransition(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SaveNote(url, Txt_NoteContent.Text);
            BookmarkItem item = App.deliciousApi.LoadLinkItemRecord(url);
            item.extended = Txt_NoteContent.Text;
            App.deliciousApi.SaveLinkItemRecord(item);
            App.deliciousApi.AddBookmark(item).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        //if done, do nothing
                        if (t.Result != "done") MessageBox.Show("同步失败...检查网络或用户名和密码");
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    Dispatcher.BeginInvoke(() => { MessageBox.Show("同步失败...检查网络或用户名和密码"); });
                }
            });
        }

    }
}