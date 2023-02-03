﻿#pragma checksum "..\..\..\Menus\EditorMenuTab.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "202404A364D45B3670216D095658A6D823A54F950BBC7DF4D89A37D98FC8C346"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Dom5Edit;
using Dom5Editor;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Dom5Editor {
    
    
    /// <summary>
    /// ModView
    /// </summary>
    public partial class ModView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl ModEditorTabs;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabItem ModEditorMain;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button LoadModButton;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SaveModButton;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EditorModName;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EditorDescription;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EditorModVersion;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EditorModDomVersion;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabItem ModEditorMonsters;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid MonsterVMPanel;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\Menus\EditorMenuTab.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox EditorMonsterList;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Dom5Editor;component/menus/editormenutab.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Menus\EditorMenuTab.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ModEditorTabs = ((System.Windows.Controls.TabControl)(target));
            return;
            case 2:
            this.ModEditorMain = ((System.Windows.Controls.TabItem)(target));
            return;
            case 3:
            this.LoadModButton = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\Menus\EditorMenuTab.xaml"
            this.LoadModButton.Click += new System.Windows.RoutedEventHandler(this.LoadModButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.SaveModButton = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\..\Menus\EditorMenuTab.xaml"
            this.SaveModButton.Click += new System.Windows.RoutedEventHandler(this.SaveModButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.EditorModName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.EditorDescription = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.EditorModVersion = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.EditorModDomVersion = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.ModEditorMonsters = ((System.Windows.Controls.TabItem)(target));
            return;
            case 10:
            this.MonsterVMPanel = ((System.Windows.Controls.Grid)(target));
            return;
            case 11:
            this.EditorMonsterList = ((System.Windows.Controls.ListBox)(target));
            
            #line 32 "..\..\..\Menus\EditorMenuTab.xaml"
            this.EditorMonsterList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.EditorMonsterList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

