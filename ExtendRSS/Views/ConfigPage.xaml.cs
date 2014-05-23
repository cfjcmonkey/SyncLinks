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
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        private void SetAccount_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.SetAccount(Txt_Username.Text, Txt_Password.Password);
            MessageBox.Show("完成.");
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            App.deliciousApi.ClearStorage();
            MessageBox.Show("完成.");
        }
    }
}