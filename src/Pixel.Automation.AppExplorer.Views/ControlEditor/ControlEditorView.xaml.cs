﻿using Pixel.Automation.Editor.Controls.Arguments;
using Pixel.Automation.Editor.Controls.Browsers;
using Pixel.Automation.Editor.Controls.HotKeys;
using Pixel.Automation.Editor.Controls.Mobile;
using System;
using System.Diagnostics;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Pixel.Automation.AppExplorer.Views.ControlEditor
{
    /// <summary>
    /// Interaction logic for ControlEditorView.xaml
    /// </summary>
    public partial class ControlEditorView
    {

        private double lastScrollPosition = 0.0D;

        public ControlEditorView()
        {
            InitializeComponent();
            this.propertyGrid.PreparePropertyItem += SetEditorForProperty;
            //this.propertyGrid.PropertyValueChanged += ResetScroll;
            //this.propertyGrid.MouseWheel += GetCurrentScrollPosition;
            //this.propertyGrid.SelectedObjectChanged += ResetSrollToTop;
        }

        private void ResetSrollToTop(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            lastScrollPosition = 0.0D;
        }

        private void GetCurrentScrollPosition(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            lastScrollPosition = this.propertyGrid.GetScrollPosition();
        }

        private void ResetScroll(object sender, EventArgs e)
        {
            this.propertyGrid.ScrollToPosition(lastScrollPosition);
        }

       

        private void SetEditorForProperty(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemEventArgs e)
        {
            //Note : Any change here should also be made in PropertyGridView.xaml.cs used on main editor
            try
            {             
                PropertyItem targetPropertyItem = e.Item as PropertyItem;

                if (targetPropertyItem.DisplayName.Equals("Hot Key") || targetPropertyItem.DisplayName.Equals("Keys"))
                {
                    targetPropertyItem.Editor = (new KeyEditor()).ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.DisplayName.Equals("Find By"))
                {
                    targetPropertyItem.Editor = (new WebFindBy()).ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.DisplayName.Equals("(Android) Find By"))
                {
                    targetPropertyItem.Editor = (new AndroidFindBy()).ResolveEditor(targetPropertyItem);
                    return;
                }

                if (targetPropertyItem.Value == null)
                {
                    return;
                }
                if (targetPropertyItem.Value.GetType().Name.StartsWith("InArgument"))
                {
                    targetPropertyItem.Editor = (new InArgumentEditor()).ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.Value.GetType().Name.StartsWith("OutArgument"))
                {
                    targetPropertyItem.Editor = (new OutArgumentEditor()).ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.Value.GetType().Name.StartsWith("PredicateArgument"))
                {
                    targetPropertyItem.Editor = (new InArgumentEditor()).ResolveEditor(targetPropertyItem);
                    return;
                }               
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
