using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Core.Editors
{
    /// <summary>
    /// Interaction logic for ControlEditorButton.xaml
    /// </summary>
    public partial class ControlEditorButton : UserControl
    {
        public static readonly DependencyProperty TargetControlProperty = DependencyProperty.Register("TargetControl", typeof(IControlIdentity), typeof(ControlEditorButton),
                                                                                     new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public IControlIdentity TargetControl
        {
            get { return (IControlIdentity)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        public ControlEditorButton()
        {
            InitializeComponent();
        }

        private async void OpenControlEditorWindow(object sender, RoutedEventArgs e)
        {
            if(TargetControl != null)
            {
                IWindowManager windowManager = IoC.Get<IWindowManager>();
                IControlEditor controlEditor = IoC.Get<IControlEditor>();
                controlEditor.Initialize(TargetControl);
                await windowManager.ShowDialogAsync (controlEditor);
            }         
        }
    }
}
