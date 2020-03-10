using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Core.Editors
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

        public static readonly DependencyProperty ActorComponentProperty = DependencyProperty.Register("ActorComponent", typeof(ActorComponent), typeof(ScriptEditorButton),
                                                                                                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public ActorComponent ActorComponent
        {
            get { return (ActorComponent)GetValue(ActorComponentProperty); }
            set { SetValue(ActorComponentProperty, value); }
        }

        public ScriptEditorButton()
        {
            InitializeComponent();
        }

        private async void OpenScriptEditorWindow(object sender, RoutedEventArgs e)
        {
            if(ActorComponent != null && !string.IsNullOrEmpty(ScriptFile))
            {
                var entityManager = this.ActorComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                IScriptEditorScreen scriptEditor = scriptEditorFactory.CreateScriptEditor();
                scriptEditor.OpenDocument(this.ScriptFile, GetDefaultScript(this.ActorComponent));
                await windowManager.ShowDialogAsync(scriptEditor);
            }
          
        }

        private string GetDefaultScript(ActorComponent actorComponent)
        {
            if(actorComponent.Tag.Equals("ScriptedAction"))
            {
                return $"#r \"Pixel.Automation.Core.dll\"{Environment.NewLine}#r \"Pixel.Automation.CoreComponent.dll\"{Environment.NewLine}using {typeof(EntityManager).Namespace};{Environment.NewLine}using {typeof(IApplication).Namespace};{Environment.NewLine}" +
                    $"using {actorComponent.EntityManager.Arguments?.GetType().Namespace};{Environment.NewLine}{Environment.NewLine}bool TryExecute(EntityManager entityManager, IApplication application, Entity control, {actorComponent.EntityManager.Arguments?.GetType().GetDisplayName()} dataModel){Environment.NewLine}{{{Environment.NewLine}    return true;{Environment.NewLine}}}";
            }
            return string.Empty;
        }
    }
}
