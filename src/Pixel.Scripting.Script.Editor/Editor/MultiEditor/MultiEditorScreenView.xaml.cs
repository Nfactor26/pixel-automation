﻿using System;
using System.Diagnostics;

namespace Pixel.Scripting.Script.Editor.MultiEditor
{
    /// <summary>
    /// Interaction logic for MultiEditorScreenView.xaml
    /// </summary>
    public partial class MultiEditorScreenView
    {
        public MultiEditorScreenView()
        {
            InitializeComponent();
        }

        [DebuggerStepThrough]
        private void OnManagerLayoutUpdated(object sender, EventArgs e)
        {
            UpdateFloatingWindows();
        }


        [DebuggerStepThrough]
        public void UpdateFloatingWindows()
        {
            var mainWindow = System.Windows.Window.GetWindow(this);
            var mainWindowIcon = (mainWindow != null) ? mainWindow.Icon : null;
            foreach (var window in Manager.FloatingWindows)
            {
                window.Icon = mainWindowIcon;              
            }
        }
    }
}
