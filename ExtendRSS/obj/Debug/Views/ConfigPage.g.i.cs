﻿#pragma checksum "E:\project\WP8\SyncLinks\ExtendRSS\Views\ConfigPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0CBA04A475343D987564B888E6B3D5FF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace ExtendRSS.Views {
    
    
    public partial class ConfigPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel ContentPanel;
        
        internal System.Windows.Controls.TextBox Txt_Username;
        
        internal System.Windows.Controls.PasswordBox Txt_Password;
        
        internal System.Windows.Controls.Button SetAccount;
        
        internal System.Windows.Controls.Button ClearCache;
        
        internal System.Windows.Controls.Button Sycn;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/ExtendRSS;component/Views/ConfigPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.StackPanel)(this.FindName("ContentPanel")));
            this.Txt_Username = ((System.Windows.Controls.TextBox)(this.FindName("Txt_Username")));
            this.Txt_Password = ((System.Windows.Controls.PasswordBox)(this.FindName("Txt_Password")));
            this.SetAccount = ((System.Windows.Controls.Button)(this.FindName("SetAccount")));
            this.ClearCache = ((System.Windows.Controls.Button)(this.FindName("ClearCache")));
            this.Sycn = ((System.Windows.Controls.Button)(this.FindName("Sycn")));
        }
    }
}

