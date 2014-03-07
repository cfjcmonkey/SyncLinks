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
            string text = App.deliciousApi.LoadNote(url);
            if (text == null) text = url + "\n\n";
            Txt_NoteContent.Text = text;
            Txt_NoteContent.Focus();
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalVelocity > 0)
            {
                NavigationService.Navigate(new Uri("/Views/BrowserPage.xaml?url=" + url, UriKind.Relative));
            }
        }

        private void NavigationOutTransition_EndTransition(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SaveNote(url, Txt_NoteContent.Text);
        }

    }
}