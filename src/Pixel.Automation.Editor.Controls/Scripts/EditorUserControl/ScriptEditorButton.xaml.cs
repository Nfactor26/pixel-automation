using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Scripts.EditorUserControl
{
    /// <summary>
    /// Interaction logic for ScriptEditorButton.xaml
    /// </summary>
    public partial class ScriptEditorButton : UserControl
    {
        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(ScriptEditorButton),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty ActorComponentProperty = DependencyProperty.Register("ActorComponent", typeof(Component), typeof(ScriptEditorButton),
                                                                                                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component ActorComponent
        {
            get { return (Component)GetValue(ActorComponentProperty); }
            set { SetValue(ActorComponentProperty, value); }
        }

        public ScriptEditorButton()
        {
            InitializeComponent();
        }

        private async void OpenScriptEditorWindow(object sender, RoutedEventArgs e)
        {
            if (ActorComponent != null && !string.IsNullOrEmpty(ScriptFile))
            {
                var entityManager = this.ActorComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                await editorFactory.CreateAndShowScriptEditorScreenAsync(windowManager, this.ActorComponent, this.ScriptFile, GetDefaultScript);
            }

        }

        private string GetDefaultScript(IComponent actorComponent)
        {
            if (actorComponent.Tag.Equals("ExecuteScript"))
            {
                return $"#r \"{typeof(IComponent).Assembly.GetName().Name}.dll\"{Environment.NewLine}#r \"{typeof(ApplicationEntity).Assembly.GetName().Name}.dll\"{Environment.NewLine}using {typeof(EntityManager).Namespace};{Environment.NewLine}using {typeof(IApplication).Namespace};{Environment.NewLine}" +
                    $"using {actorComponent.EntityManager.Arguments?.GetType().Namespace};{Environment.NewLine}{Environment.NewLine}" +
                    $"void Execute(IApplication application, IComponent current){Environment.NewLine}{{{Environment.NewLine}    //Do something{Environment.NewLine} }}" +
                    $"{Environment.NewLine}return ((Action<IApplication, IComponent>)Execute);";               
            }
            if(actorComponent.Tag.Equals("ExecuteAsyncScript"))
            {
                return $"#r \"{typeof(IComponent).Assembly.GetName().Name}.dll\"{Environment.NewLine}#r \"{typeof(ApplicationEntity).Assembly.GetName().Name}.dll\"{Environment.NewLine}using {typeof(EntityManager).Namespace};{Environment.NewLine}using {typeof(IApplication).Namespace};{Environment.NewLine}" +
                   $"using {actorComponent.EntityManager.Arguments?.GetType().Namespace};{Environment.NewLine}{Environment.NewLine}" +
                   $"async Task ExecuteAsync(IApplication application, IComponent current){Environment.NewLine}{{{Environment.NewLine}    //Do something{Environment.NewLine} await Task.CompletedTask;{Environment.NewLine}}}" +
                   $"{Environment.NewLine}return ((Func<IApplication, IComponent, Task>)ExecuteAsync);";
            }
            return string.Empty;
        }
    }
}
