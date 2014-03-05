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
        ProgressIndicator proIndicator;
        int count; //已加载的链接数

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
            if (e.AddedItems[0] == HistoryViewer)
            {
                OfflineLoad();
            }
            else if (e.AddedItems[0] == AllViewer)
            {
                count = 0;
                ShowAllLinks();
            }
        }

        /// <summary>
        /// 设链接为已读.暂时只记录到本地,不同步到网络.
        /// </summary>
        private void SetReaded(BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?") == false || item.isUnReaded == "1")
            {
                string tag = "Readed";
                string[] sub = item.tag.Split(',');
                foreach (string s in sub)
                {
                    if (s.Trim().Equals("UnReaded") == false) tag += "," + s;
                }
                item.isUnReaded = "0";
                item.tag = tag;
                App.deliciousApi.SaveLinkItemRecord(item);
                /*
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
                 */
            }
        }

        /// <summary>
        /// 设链接为未读.暂时只记录到本地,不同步到网络
        /// </summary>
        private void SetUnReaded(BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "\\W?Readed\\W?") == true || item.isUnReaded == "0")
            {
                string tag = "UnReaded";
                string[] sub = item.tag.Split(',');
                foreach (string s in sub)
                {
                    if (s.Trim().Equals("Readed") == false) tag += "," + s;
                }
                item.isUnReaded = "1";
                item.tag = tag;
                App.deliciousApi.SaveLinkItemRecord(item);
                /*
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
                 */
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
        /// 加载本地的链接信息
        /// </summary>
        private void OfflineLoad()
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载书签...";

            HistoryLinksListBox.Items.Clear();
            foreach (BookmarkItem item in App.deliciousApi.LoadLinkItemsRecord())
            {
                HistoryLinksListBox.Items.Add(GenerateItemUI(item));
            }

            proIndicator.IsVisible = false;
        }

        /// <summary>
        /// 从网络加载全部链接
        /// </summary>
        private void ShowAllLinks()
        {
            proIndicator.IsVisible = true;
            proIndicator.Text = "正在加载书签...";

            App.deliciousApi.GetAll(count, 10).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Result == null)
                        {
                            //MessageBox.Show("请求失败！检查用户名或密码");
                            Login_Popup.IsOpen = true;
                        }
                        else if (t.Result.Count == 0)
                        {
                            MessageBox.Show("已加载所有的链接.");
                        }
                        else
                        {
                            AllLinksListBox.Items.Clear();
                            foreach (BookmarkItem item in t.Result)
                            {
                                AllLinksListBox.Items.Add(GenerateItemUI(item));
                            }
                            count += t.Result.Count;
                        }
                        proIndicator.IsVisible = false;
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("请求失败！检查网络或用户名密码");
                        proIndicator.IsVisible = false;
                    });
            });
        }

        /// <summary>
        /// 根据书签内容生成显示到界面的控件
        /// </summary>
        /// <param name="item"></param>
        /// <returns>由两个按钮组成的控件，外包一层StackPanel</returns>
        private UIElement GenerateItemUI(BookmarkItem item)
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
            return stack;
        }

        /// <summary>
        /// 点击设置按钮
        /// </summary>
        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Login_Popup.IsOpen = true;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (Login_Popup.IsOpen)
            {
                Login_Popup.IsOpen = false;
                e.Cancel = true;
            }
        }

    }
}