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
    public partial class LinkListControl : UserControl
    {
        IndexPage parent;
        int count; //已加载的链接数
        public string StatusTag { get; set; }

        public LinkListControl(IndexPage page)
        {
            InitializeComponent();

            parent = page;
            count = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OfflineLoad(StatusTag);
            if (StatusTag == BookmarkItem.UNREAD)
                LoadUnReadedLinks();
            else LoadRecentLinks(StatusTag);
        }

        public void LocalRefresh()
        {
            OfflineLoad(StatusTag);
        }

        /// <summary>
        /// 加载本地的链接信息
        /// </summary>
        /// <param name="tag">加载包含指定tag的链接;若tag为null,则加载所有链接</param>
        private void OfflineLoad(string tag)
        {
            parent.proIndicator.IsVisible = true;
            parent.proIndicator.Text = "正在加载书签...";

            ContentPanel.Children.Clear();
            if (tag == null)
            {
                foreach (BookmarkItem item in App.deliciousApi.LoadLinkItemsRecord())
                {
                        ContentPanel.Children.Add(GenerateItemUI(item));
                }
            }
            if (tag == BookmarkItem.UNREAD)
            {   //只要不包含已读tag的都是未读
                foreach (BookmarkItem item in App.deliciousApi.LoadLinkItemsRecord())
                {
                    if (!Regex.IsMatch(item.tag, "(^|\\W)" + BookmarkItem.READ + "($|\\W)"))
                        ContentPanel.Children.Add(GenerateItemUI(item));
                }
            }
            else
            {
                foreach (BookmarkItem item in App.deliciousApi.LoadLinkItemsRecord())
                {
                    if (Regex.IsMatch(item.tag, "(^|\\W)" + tag + "($|\\W)"))
                        ContentPanel.Children.Add(GenerateItemUI(item));
                }
            }

            parent.proIndicator.IsVisible = false;
        }

        /// <summary>
        /// 加载最近的链接
        /// 每次加载10个链接，随着向下拉动逐渐增多.
        /// 用全局变量count记录已加载的链接数
        /// </summary>
        /// <param name="tag">加载包含指定tag的链接;若tag为null,则加载所有链接</param>
        /// <remarks>当前未读项并不都带有未读tag,因此会漏选未读项</remarks>
        private void LoadRecentLinks(string tag = null)
        {
            parent.proIndicator.IsVisible = true;
            parent.proIndicator.Text = "正在加载书签...";

            App.deliciousApi.GetAll(count, 10, tag).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Result.Count > 0)
                        {
                            OfflineLoad(tag);
                            count += t.Result.Count;
                        }//若返回零条链接,则不做处理.
                        parent.proIndicator.IsVisible = false;
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Exception.InnerException.Message.Contains("401"))
                        {
                            MessageBox.Show("请求失败！检查用户名和密码");
                            (parent as IndexPage).Login_Popup.IsOpen = true;
                        }
                        else MessageBox.Show("请求失败！检查网络");
                        parent.proIndicator.IsVisible = false;
                    });
            });
        }

        /// <summary>
        /// 加载未读的链接.
        /// 先加载所有的链接,根据DeliciousApi的处理,凡是不包含已读tag的均为未读.
        /// </summary>
        private void LoadUnReadedLinks()
        {
            parent.proIndicator.IsVisible = true;
            parent.proIndicator.Text = "正在加载书签...";

            App.deliciousApi.GetAll(count, 10, null).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Result.Count > 0)
                        {
                            OfflineLoad(BookmarkItem.UNREAD);
                            count += t.Result.Count;
                        }//若返回零条链接,则不做处理.
                        parent.proIndicator.IsVisible = false;
                    });
                }
                else if (t.Status == TaskStatus.Faulted)
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (t.Exception.InnerException.Message.Contains("401"))
                        {
                            MessageBox.Show("请求失败！检查用户名和密码");
                            (parent as IndexPage).Login_Popup.IsOpen = true;
                        }
                        else MessageBox.Show("请求失败！检查网络");
                        parent.proIndicator.IsVisible = false;
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
                SetReaded(ref it);
                parent.NavigationService.Navigate(new Uri("/Views/BrowserPage.xaml?url=" + item.href, UriKind.Relative));

//                if (it.isUnReaded.Equals("1")) it.isUnReaded = "0";
            };
            ContextMenu contextmenu = new ContextMenu();
            MenuItem mark = new MenuItem(){ Header = "标记为已读/未读" };
            mark.Tap += (s, e) =>
            {
                BookmarkItem mitem = (s as MenuItem).DataContext as BookmarkItem;
                if (mitem.isUnReaded == "1") SetReaded(ref mitem);
                else SetUnReaded(ref mitem);
            };

            MenuItem star = new MenuItem(){ Header = "收藏" };
            star.Tap += (s, e) =>
            {
                BookmarkItem sitem = (s as MenuItem).DataContext as BookmarkItem;
                SetStared(ref sitem);
            };

            contextmenu.Items.Add(mark);
            contextmenu.Items.Add(star);
            ContextMenuService.SetContextMenu(stack, contextmenu);
/*
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
 */ 
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
        /// 设链接为已读.记录到本地,同步到网络.
        /// </summary>
        private void SetReaded(ref BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "(^|\\W)" + BookmarkItem.READ + "($|\\W)") == false || item.isUnReaded == "1")
            {
                string tag = BookmarkItem.READ;
                char[] sp = { ',', ' ' };
                string[] sub = item.tag.Split(sp, 2);
                foreach (string s in sub)
                {
                    string st = s.Trim();
                    if (st.Equals(BookmarkItem.UNREAD) == false && st.Equals(BookmarkItem.READ) == false && st.Length > 0) tag += "," + st;
                }
                item.isUnReaded = "0";
                item.tag = tag;
                App.deliciousApi.SaveLinkItemRecord(item);

                App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            //if done, do nothing
                            if (t.Result != "done") MessageBox.Show("请求失败！检查网络或用户名和密码");
                        });
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                    }
                });
            }
        }

        /// <summary>
        /// 设链接为未读.记录到本地,同步到网络
        /// </summary>
        private void SetUnReaded(ref BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "(^|\\W)" + BookmarkItem.READ + "($|\\W)") == true || item.isUnReaded == "0")
            {
                string tag = BookmarkItem.UNREAD;
                char[] sp = {',', ' '};
                string[] sub = item.tag.Split(sp,2);
                foreach (string s in sub)
                {
                    string st = s.Trim();
                    if (st.Equals(BookmarkItem.UNREAD) == false && st.Equals(BookmarkItem.READ) == false && st.Length > 0) tag += "," + st;
                }
                item.isUnReaded = "1";
                item.tag = tag;
                App.deliciousApi.SaveLinkItemRecord(item);

                App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            //if done, do nothing
                            if (t.Result != "done") MessageBox.Show("请求失败！检查网络或用户名和密码");
                        });
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                    }
                });

            }
        }

        private void SetStared(ref BookmarkItem item)
        {
            if (Regex.IsMatch(item.tag, "(^|\\W)" + BookmarkItem.STAR + "($|\\W)") == false)
            {
                item.tag += "," + BookmarkItem.STAR;
                App.deliciousApi.SaveLinkItemRecord(item);

                App.deliciousApi.AddBookmark(item).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            //if done, do nothing
                            if (t.Result != "done") MessageBox.Show("请求失败！检查网络或用户名和密码");
                        });
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("请求失败！检查网络或用户名和密码"); });
                    }
                });

            }
        }

        /// <summary>
        /// 拉动更新
        /// </summary>
        private void ContentScroll_GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == Orientation.Vertical && e.VerticalVelocity < 0 &&
                LinkListScroller.VerticalOffset + LinkListScroller.ViewportHeight >= LinkListScroller.ExtentHeight - 600)
                LoadRecentLinks(StatusTag);
        }

    }
}
