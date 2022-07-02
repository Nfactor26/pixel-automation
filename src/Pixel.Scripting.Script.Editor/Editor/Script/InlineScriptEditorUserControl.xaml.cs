using Pixel.Automation.Editor.Controls;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Component = Pixel.Automation.Core.Component;

namespace Pixel.Scripting.Script.Editor.Script
{
    /// <summary>
    /// Interaction logic for InlineScriptEditorUserControl.xaml
    /// </summary>
    public partial class InlineScriptEditorUserControl : UserControl, INotifyPropertyChanged
    {
        private readonly ILogger logger = Log.ForContext<InlineScriptEditorUserControl>();

        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(InlineScriptEditorUserControl),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(InlineScriptEditorUserControl),
                                                                                          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }
     
        public IInlineScriptEditor Editor { get; private set; }

        public InlineScriptEditorUserControl()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;          
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OwnerComponent != null && !string.IsNullOrEmpty(ScriptFile) && this.Editor == null)
                {
                    var editorFactory = OwnerComponent.EntityManager.GetServiceOfType<IScriptEditorFactory>();
                    this.Editor = editorFactory.CreateAndInitializeInilineScriptEditor(this.OwnerComponent, this.ScriptFile,
                        (a) => { return string.Empty; });
                    OnPropertyChanged(nameof(this.Editor));
                    logger.Debug($"Created and initialized inline script editor for component with Id : {this.OwnerComponent.Id}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        #region INotifyPropertyChanged

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion INotifyPropertyChanged
    }
}
