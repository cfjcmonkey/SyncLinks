using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ExtendRSS.Resources;
using ExtendRSS.Models;

namespace ExtendRSS
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator proIndicator;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            proIndicator = new ProgressIndicator(){
                IsIndeterminate = true,
                IsVisible = false
            };
            SystemTray.SetProgressIndicator(this, proIndicator);

            BookmarkListBox.Width = Application.Current.Host.Content.ActualWidth;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Login_Popup.IsOpen = true;
//            ShowRecent();

            List<BookmarkItem> listbox = new List<BookmarkItem>();
            BookmarkItem item = new BookmarkItem();
            item.href = "/Views/BrowserPage.xaml?url=http://www.baidu.com";
            item.description = "baidu";
            item.extended = "000000000000000001111111111111111111111111111111111222222222222222222222";
            item.tag = "Read";
            item.isUnReaded = "0";
            listbox.Add(item);
            item.tag = "UnRead";
            item.isUnReaded = "0";
            listbox.Add(item);
            BookmarkListBox.ItemsSource = listbox;

        }

        /// <summary>
        /// 登陆按钮的响应，登陆成功后显示最近的书签
        /// </summary>
        private void Btn_Login_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SetUsername(Txt_Username.Text);
            App.deliciousApi.SetPassword(Txt_Password.Text);
            Login_Popup.IsOpen = false;

            output.Text = "Get Recent Bookmarks...";
            ShowRecent();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Login_Popup.IsOpen = false;
        }

        private void ShowRecent()
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载书签...";

            output.Text = "Loading recent bookmarks";
            App.deliciousApi.GetRecent().ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Result == null) Login_Popup.IsOpen = true;
                        else BookmarkListBox.ItemsSource = t.Result;
                        output.Text = "Recent Bookmarks";
                        proIndicator.IsVisible = false;
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                    Dispatcher.BeginInvoke(() =>
                    {
                        output.Text = "Task Failed";
                        proIndicator.IsVisible = false;
                    });
            });
        }

        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}