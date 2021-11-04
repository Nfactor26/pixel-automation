using System.Windows;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// Interaction logic for AutomationEditorView.xaml
    /// </summary>
    public partial class AutomationEditorView : EditorView
    {       
        public AutomationEditorView()
        {
            InitializeComponent();              
        }

        protected override UIElement GetDesignerHost()
        {
            return this.DesignerRoot;
        }      
    }
}
