using Pixel.Automation.Editor.Controls.Arguments;
using Pixel.Automation.Editor.Controls.HotKeys;
using Serilog;
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
        private readonly ILogger logger = Log.ForContext<PropertyGridView>();

        public PropertyGridView()
        {
            InitializeComponent();
            this.propertyGrid.PreparePropertyItem += SetEditorForProperty;
        }

        private void SetEditorForProperty(object sender, PropertyItemEventArgs e)
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
                    targetPropertyItem.Editor = new InArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.Value.GetType().Name.StartsWith("OutArgument"))
                {
                    targetPropertyItem.Editor = new OutArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.Value.GetType().Name.StartsWith("PredicateArgument"))
                {
                    targetPropertyItem.Editor = new InArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                if (targetPropertyItem.DisplayName.Equals("Hot Key") || targetPropertyItem.DisplayName.Equals("Keys"))
                {
                    targetPropertyItem.Editor = new KeyEditor().ResolveEditor(targetPropertyItem);
                    return;
                }                
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
