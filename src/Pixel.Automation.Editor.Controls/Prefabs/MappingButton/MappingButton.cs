using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Prefabs
{
    public abstract class MappingButton : UserControl
    {       

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(MappingButton),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(MappingButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }


        public static readonly DependencyProperty AssignFromProperty = DependencyProperty.Register("AssignFrom", typeof(Type), typeof(MappingButton),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Type AssignFrom
        {
            get { return (Type)GetValue(AssignFromProperty); }
            set { SetValue(AssignFromProperty, value); }
        }

        public static readonly DependencyProperty AssignToProperty = DependencyProperty.Register("AssignTo", typeof(Type), typeof(MappingButton),
                                                                                         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Type AssignTo
        {
            get { return (Type)GetValue(AssignToProperty); }
            set { SetValue(AssignToProperty, value); }
        }

        public static readonly DependencyProperty EntityManagerProperty = DependencyProperty.Register("EntityManager", typeof(EntityManager), typeof(MappingButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public EntityManager EntityManager
        {
            get { return (EntityManager)GetValue(EntityManagerProperty); }
            set { SetValue(EntityManagerProperty, value); }
        }

        protected async void OpenMappingWindow(object sender, RoutedEventArgs e)
        {
            var prefabArgumentMapper = GetArgumentMapper();
            var propertyMappings = prefabArgumentMapper.GenerateMapping(this.EntityManager.GetScriptEngine(), this.AssignFrom, this.AssignTo).ToList();
            string generatedCode = prefabArgumentMapper.GeneratedMappingCode(propertyMappings, this.AssignFrom, this.AssignTo);

            IWindowManager windowManager = IoC.Get<IWindowManager>();
            IScriptEditorFactory scriptEditorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            using (IScriptEditorScreen scriptEditor = scriptEditorFactory.CreateScriptEditor())
            {
                if (OwnerComponent.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
                {
                    //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                    AddProject(new string[] { testCaseEntity.Tag });
                }
                else if (OwnerComponent.TryGetAnsecstorOfType<TestFixtureEntity>(out TestFixtureEntity testFixtureEntity))
                {
                    //Test fixture have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project                
                    AddProject(new string[] { testFixtureEntity.Tag });
                }
                else
                {
                    AddProject(Array.Empty<string>());
                }
                scriptEditorFactory.AddDocument(this.ScriptFile, GetProjectName(), generatedCode);
                scriptEditor.OpenDocument(this.ScriptFile, GetProjectName(), generatedCode);
                await windowManager.ShowDialogAsync(scriptEditor);
                scriptEditorFactory.RemoveProject(GetProjectName());
            }              

            void AddProject(string[] projectReferences)
            {
                scriptEditorFactory.AddProject(GetProjectName(), projectReferences, EntityManager.Arguments.GetType());
            }
        }

        protected abstract IPrefabArgumentMapper GetArgumentMapper();
      
        protected string GetProjectName()
        {
            return $"{OwnerComponent.Id}";
        }
    }
}
