using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
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
            if(ActorComponent != null && !string.IsNullOrEmpty(ScriptFile))
            {
                var entityManager = this.ActorComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                IScriptEditorScreen scriptEditor = editorFactory.CreateScriptEditor();
                if (ActorComponent.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
                {
                    //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                    editorFactory.AddProject(ActorComponent.Id, new string[] { testCaseEntity.Tag }, ActorComponent.EntityManager.Arguments.GetType());
                }
                else
                {
                    editorFactory.AddProject(ActorComponent.Id, Array.Empty<string>(), ActorComponent.EntityManager.Arguments.GetType());
                }
                string initialContent = GetDefaultScript(this.ActorComponent);
                editorFactory.AddDocument(ScriptFile, ActorComponent.Id, initialContent);
                scriptEditor.OpenDocument(this.ScriptFile, ActorComponent.Id, initialContent);
                await windowManager.ShowDialogAsync(scriptEditor);
            }
          
        }

        private string GetDefaultScript(Component actorComponent)
        {          
            if (actorComponent.Tag.Equals("ScriptedAction"))
            {
                return $"#r \"{typeof(IComponent).Assembly.GetName().Name}.dll\"{Environment.NewLine}#r \"{typeof(ApplicationEntity).Assembly.GetName().Name}.dll\"{Environment.NewLine}using {typeof(EntityManager).Namespace};{Environment.NewLine}using {typeof(IApplication).Namespace};{Environment.NewLine}" +
                    $"using {actorComponent.EntityManager.Arguments?.GetType().Namespace};{Environment.NewLine}{Environment.NewLine}" +
                    $"void Execute(IApplication application, IComponent current){Environment.NewLine}{{{Environment.NewLine}    //Do something{Environment.NewLine}}}" +
                    $"{Environment.NewLine}return ((Action<IApplication,IComponent>)Execute);";
            }
            return string.Empty;
        }
    }
}
