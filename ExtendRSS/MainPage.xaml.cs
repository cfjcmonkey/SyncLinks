using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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

            output.Text = "最近的书签";
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Login_Popup.IsOpen = true;
            ShowRecent();
/*
//            ObservableCollection<BookmarkItem> listbox = new ObservableCollection<BookmarkItem>();
            BookmarkItem item = new BookmarkItem();
            item.href = "/Views/BrowserPage.xaml?url=http://www.baidu.com";
            item.description = "baidu";
            item.extended = "000000000000000001111111111111111111111111111111111222222222222222222222";
            item.tag = "Read";
            item.isUnReaded = "0";
               
//            listbox.Add(item);
//            BookmarkListBox.ItemsSource = listbox;

            BookmarkListBox.Items.Clear();

            StackPanel stack = new StackPanel(){
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            Button b = new Button{
                Style = (Style)Resources["BookmarkItemButtonStyle"]
            };

            b.Tap += (param_sender, param_e) =>
            {
                NavigationService.Navigate(new Uri(item.href, UriKind.Relative));
                output.Text = "tap textblock";
            };
 
            Button a = new Button{
                Style = (Style)Resources["IsReadItemButtonStyle"]
            };
            a.Tap += (param_sender, param_e) =>
            {
                BookmarkItem it = a.DataContext as BookmarkItem;
//                Button c = (b.Parent as StackPanel).Children[0] as Button;
                if (it.isUnReaded.Equals("0"))
                {
                    SetUnReaded(it);
                    it.isUnReaded = "1";
//                    (a.DataContext as BookmarkItem).isUnReaded = "1";
                }
                else
                {
                    SetReaded(it);
                    it.isUnReaded = "0";
//                    (a.DataContext as BookmarkItem).isUnReaded = "0";
                }
//                a.UpdateLayout();
                output.Text = "tap rectangle";
            };
            stack.Children.Add(a);
            stack.Children.Add(b);
            stack.DataContext = item;
            BookmarkListBox.Items.Add(stack);
//            Grid grid = new Grid();
//            grid.Children.Add(a);
//            grid.Children.Add(b);
//            BookmarkListBox.Items.Add(grid);
 */
        }

        /// <summary>
        /// 登陆按钮的响应，登陆成功后显示最近的书签
        /// </summary>
        private void Btn_Login_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SetUsername(Txt_Username.Text);
            App.deliciousApi.SetPassword(Txt_Password.Text);
            Login_Popup.IsOpen = false;

            //output.Text = "Loading Recent Bookmarks...";
            ShowRecent();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Login_Popup.IsOpen = false;
        }

        private void SetReaded(BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?") == false)
            {
                string tag = "Readed";
                string[] sub = item.tag.Split(',');
                foreach (string s in sub)
                {
                    if (s.Trim().Equals("UnReaded") == false) tag += "," + s;
                }
                item.tag = tag;

                App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("成功"); });
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                    }
                });
            }
        }

        private void SetUnReaded(BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?") == true)
            {
                string tag = "UnReaded";
                string[] sub = item.tag.Split(',');
                foreach (string s in sub)
                {
                    if (s.Trim().Equals("Readed") == false) tag += "," + s;
                }
                item.tag = tag;

                App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("成功"); });
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                    }

                });
            }
        }

        private void ShowRecent()
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载书签...";

            //output.Text = "Loading recent bookmarks";
            App.deliciousApi.GetRecent().ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Result == null) Login_Popup.IsOpen = true;
                        else
                        {
                            BookmarkListBox.Items.Clear();
                            foreach (BookmarkItem item in t.Result)
                            {
                                StackPanel stack = new StackPanel()
                                {
                                    Orientation = System.Windows.Controls.Orientation.Horizontal
                                };
                                Button b = new Button
                                {
                                    Style = (Style)Resources["BookmarkItemButtonStyle"]
                                };
                                b.Tap += (param_sender, param_e) => //点击进入链接
                                {
                                    BookmarkItem it = b.DataContext as BookmarkItem;
                                    SetReaded(it);
                                    NavigationService.Navigate(new Uri(item.href, UriKind.Relative));

                                    if (it.isUnReaded.Equals("1")) it.isUnReaded = "0";
                                };
                                b.Hold += (param_sender, param_e) => //长按改变阅读状态，已读/未读
                                {
                                    BookmarkItem it = b.DataContext as BookmarkItem;
                                    if (it.isUnReaded.Equals("0"))
                                    {
                                        SetUnReaded(it);
                                        it.isUnReaded = "1";
                                    }
                                    else
                                    {
                                        SetReaded(it);
                                        it.isUnReaded = "0";
                                    }
                                };
                                Button a = new Button
                                {
                                    Style = (Style)Resources["IsReadItemButtonStyle"]
                                };
                                stack.Children.Add(a);
                                stack.Children.Add(b);
                                stack.DataContext = item;
                                BookmarkListBox.Items.Add(stack);
                            }
//                            BookmarkListBox.ItemsSource = t.Result;
                        }
                        //output.Text = "Recent Bookmarks";
                        proIndicator.IsVisible = false;
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("请求失败！检查网络或用户名和密码");
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