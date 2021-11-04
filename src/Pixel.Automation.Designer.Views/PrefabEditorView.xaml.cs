using System.Windows;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// Interaction logic for PrefabEditorView.xaml
    /// </summary>
    public partial class PrefabEditorView : EditorView
    {      
        public PrefabEditorView()
        {
            InitializeComponent();            
        }

        protected override UIElement GetDesignerHost()
        {
            return this.DesignerRoot;
        }

    }
}
