﻿#pragma checksum "E:\project\WP8\SyncLinks\ExtendRSS\Views\BrowserPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8FEC19B5353CDC29315837A38C6A3203"
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
using Microsoft.Phone.Shell;
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


namespace SyncLinks {
    
    
    public partial class BrowserPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.WebBrowser webBrowser;
        
        internal System.Windows.Controls.Primitives.Popup AddBookmark_Popup;
        
        internal System.Windows.Controls.Grid TitleContent;
        
        internal System.Windows.Controls.TextBox Txt_Title;
        
        internal System.Windows.Controls.Button Btn_OK;
        
        internal System.Windows.Controls.Button Btn_Cancel;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton Btn_PrePage;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton Btn_Add;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton Btn_NextPage;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem AppMenu_GoogleTrans;
        
        internal Microsoft.Phone.Shell.ApplicationBarMenuItem AppMenu_IE10;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/SyncLinks;component/Views/BrowserPage.xaml", System.UriKind.Relative));
            this.webBrowser = ((Microsoft.Phone.Controls.WebBrowser)(this.FindName("webBrowser")));
            this.AddBookmark_Popup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("AddBookmark_Popup")));
            this.TitleContent = ((System.Windows.Controls.Grid)(this.FindName("TitleContent")));
            this.Txt_Title = ((System.Windows.Controls.TextBox)(this.FindName("Txt_Title")));
            this.Btn_OK = ((System.Windows.Controls.Button)(this.FindName("Btn_OK")));
            this.Btn_Cancel = ((System.Windows.Controls.Button)(this.FindName("Btn_Cancel")));
            this.Btn_PrePage = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("Btn_PrePage")));
            this.Btn_Add = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("Btn_Add")));
            this.Btn_NextPage = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("Btn_NextPage")));
            this.AppMenu_GoogleTrans = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("AppMenu_GoogleTrans")));
            this.AppMenu_IE10 = ((Microsoft.Phone.Shell.ApplicationBarMenuItem)(this.FindName("AppMenu_IE10")));
        }
    }
}

