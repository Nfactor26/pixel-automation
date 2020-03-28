using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Editor.Core.Editors;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// Interaction logic for PropertyGridView.xaml
    /// </summary>
    public partial class PropertyGridView : UserControl
    {
        public PropertyGridView()
        {
            InitializeComponent();
            this.propertyGrid.PreparePropertyItem += SetEditorForProperty;
        }

        private void SetEditorForProperty(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemEventArgs e)
        {
            try
            {
                PropertyItem targetPropertyItem = e.Item as PropertyItem;
                if(targetPropertyItem.Value == null)
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
                if (targetPropertyItem.Name.Equals("Keys"))
                {
                    targetPropertyItem.Editor = (new KeyEditor()).ResolveEditor(targetPropertyItem);
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
