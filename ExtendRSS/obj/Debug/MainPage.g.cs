﻿#pragma checksum "E:\project\WP8\ExtendRSS\ExtendRSS\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EF9CFC46EECA1D1AF3CA010B11AD276D"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18051
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


namespace ExtendRSS {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBlock output;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Primitives.Popup Login_Popup;
        
        internal System.Windows.Controls.Grid Login_Content;
        
        internal System.Windows.Controls.TextBox Txt_Username;
        
        internal System.Windows.Controls.TextBox Txt_Password;
        
        internal System.Windows.Controls.Button Btn_Login;
        
        internal System.Windows.Controls.Button Btn_Cancel;
        
        internal System.Windows.Controls.ListBox BookmarkListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/ExtendRSS;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.output = ((System.Windows.Controls.TextBlock)(this.FindName("output")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.Login_Popup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("Login_Popup")));
            this.Login_Content = ((System.Windows.Controls.Grid)(this.FindName("Login_Content")));
            this.Txt_Username = ((System.Windows.Controls.TextBox)(this.FindName("Txt_Username")));
            this.Txt_Password = ((System.Windows.Controls.TextBox)(this.FindName("Txt_Password")));
            this.Btn_Login = ((System.Windows.Controls.Button)(this.FindName("Btn_Login")));
            this.Btn_Cancel = ((System.Windows.Controls.Button)(this.FindName("Btn_Cancel")));
            this.BookmarkListBox = ((System.Windows.Controls.ListBox)(this.FindName("BookmarkListBox")));
        }
    }
}

