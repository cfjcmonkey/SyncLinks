﻿#pragma checksum "E:\project\WP8\SyncLinks\ExtendRSS\Views\IndexPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2B9FC15AAFA25B9F1CC323797D1C2832"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18449
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
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
    
    
    public partial class IndexPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot PivotControl;
        
        internal Microsoft.Phone.Controls.PivotItem UnReadedViewer;
        
        internal Microsoft.Phone.Controls.PivotItem StarViewer;
        
        internal Microsoft.Phone.Controls.PivotItem RecentViewer;
        
        internal Microsoft.Phone.Controls.PivotItem ReadedViewer;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/ExtendRSS;component/Views/IndexPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.PivotControl = ((Microsoft.Phone.Controls.Pivot)(this.FindName("PivotControl")));
            this.UnReadedViewer = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("UnReadedViewer")));
            this.StarViewer = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("StarViewer")));
            this.RecentViewer = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("RecentViewer")));
            this.ReadedViewer = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("ReadedViewer")));
        }
    }
}

