﻿#pragma checksum "E:\project\WP8\SyncLinks\ExtendRSS\Views\LinkListControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4298FDF40B3588B3D2366EE7EE84BD0E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Hompus.PullDownToRefreshDemo;
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


namespace SyncLinks.Views {
    
    
    public partial class LinkListControl : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Hompus.PullDownToRefreshDemo.PullDownToRefreshPanel refreshPanel;
        
        internal System.Windows.Controls.ItemsControl ResultListControl;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/SyncLinks;component/Views/LinkListControl.xaml", System.UriKind.Relative));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.refreshPanel = ((Hompus.PullDownToRefreshDemo.PullDownToRefreshPanel)(this.FindName("refreshPanel")));
            this.ResultListControl = ((System.Windows.Controls.ItemsControl)(this.FindName("ResultListControl")));
        }
    }
}

