using Pixel.Automation.Editor.Controls.Arguments;
using Pixel.Automation.Editor.Controls.HotKeys;
using Pixel.Automation.Editor.Controls.Scripts.EditorPropertyGrid;
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
                else if (targetPropertyItem.Value.GetType().Name.StartsWith("InArgument"))
                {
                    targetPropertyItem.Editor = new InArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                else if (targetPropertyItem.Value.GetType().Name.StartsWith("OutArgument"))
                {
                    targetPropertyItem.Editor = new OutArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                else if (targetPropertyItem.Value.GetType().Name.StartsWith("FuncArgument"))
                {
                    targetPropertyItem.Editor = new InArgumentEditor().ResolveEditor(targetPropertyItem);
                    return;
                }
                else if (targetPropertyItem.DisplayName.Equals("Hot Key") || targetPropertyItem.DisplayName.Equals("Keys"))
                {
                    targetPropertyItem.Editor = new KeyEditor().ResolveEditor(targetPropertyItem);
                    return;
                }    
                else if(targetPropertyItem.DisplayName.Equals("Input Mapping Script") || targetPropertyItem.DisplayName.Equals("Output Mapping Script"))
                {
                    //we don't want to show edit script button on property grid for script files of a prefab entity as they need to be loaded first which is not handled
                    //by this control.
                    var browserScriptEditor = new BrowseScriptEditor() { ShowEditButton = false };
                    targetPropertyItem.Editor = browserScriptEditor.ResolveEditor(targetPropertyItem);
                }
                else if(targetPropertyItem.DisplayName.Equals("Script File") && !targetPropertyItem.IsReadOnly)
                {
                    //for script files of execute script actor
                    targetPropertyItem.Editor = new BrowseScriptEditor().ResolveEditor(targetPropertyItem);
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
