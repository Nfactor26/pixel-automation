using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Arguments.Editor
{
    public class ArgumentUserControl : UserControl
    {

        public static readonly DependencyProperty ArgumentProperty = DependencyProperty.Register("Argument", typeof(Argument), typeof(ArgumentUserControl),
                                                                                          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public Argument Argument
        {
            get { return (Argument)GetValue(ArgumentProperty); }
            set { SetValue(ArgumentProperty, value); }
        }


        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ArgumentUserControl),
                                                                                          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }


        public static readonly DependencyProperty AvailablePropertiesProperty = DependencyProperty.Register("AvailableProperties", typeof(ObservableCollection<string>), typeof(ArgumentUserControl),
                                                                                            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public ObservableCollection<string> AvailableProperties
        {
            get { return (ObservableCollection<string>)GetValue(AvailablePropertiesProperty); }
            set { SetValue(AvailablePropertiesProperty, value); }
        }
     
        public ArgumentUserControl()
        {
            
        }
      
        protected void LoadAvailableProperties()
        {
            if (Argument?.Mode != ArgumentMode.DataBound)
                return;

        
            if (OwnerComponent != null && Argument != null)
            {
                string currentValue = Argument.PropertyPath;
                var argumentType = Argument.GetType().GetGenericArguments()[0];  // Argument<string> will return string as the argumentType
                AvailableProperties.Clear();

                if (OwnerComponent.TryGetScopedParent(out IScopedEntity scopedEntity))
                {
                    string scopedArgumentName = scopedEntity.GetScopedArgumentName();
                    foreach (var item in scopedEntity.GetPropertiesOfType(argumentType))
                    {
                        AvailableProperties.Add($"{scopedArgumentName}.{item}");
                    }
                }

                foreach (var item in OwnerComponent.EntityManager.GetPropertiesOfType(argumentType))
                {
                    AvailableProperties.Add(item);
                }

                if (AvailableProperties.Contains(currentValue))
                {
                    Argument.PropertyPath = currentValue;
                }
            }

        }

        public async void ShowScriptEditor(object sender, RoutedEventArgs e)
        {
            if (OwnerComponent == null && Argument == null)
                return;

            EntityManager entityManager = this.OwnerComponent.EntityManager;

            IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
            IScriptEditorFactory scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();

            string initialContent = string.Empty;
            if (string.IsNullOrEmpty(Argument.ScriptFile))
            {
                var fileSystem = entityManager.GetServiceOfType<IFileSystem>();
                Argument.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid().ToString()}.csx"));              
                initialContent = Argument.GenerateInitialScript();
            }
            IScriptEditorScreen scriptEditor = scriptEditorFactory.CreateScriptEditor();
            scriptEditor.OpenDocument(Argument.ScriptFile, initialContent);
            await windowManager.ShowDialogAsync(scriptEditor);
        }

        public async void ChangeArgumentType(object sender, RoutedEventArgs e)
        {
            //Using outside automation process such as Application in ApplicationRepository
            if (this.OwnerComponent?.EntityManager == null)
                return;

            EntityManager entityManager = this.OwnerComponent.EntityManager;
            IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
            IArgumentTypeProvider typeProvider = entityManager.GetServiceOfType<IArgumentTypeProvider>();
            ArgumentTypeBrowserViewModel typeBrowserWindow = new ArgumentTypeBrowserViewModel(typeProvider);

            var result = await windowManager.ShowDialogAsync(typeBrowserWindow);

            if (result.HasValue && result.Value)
            {
                DeleteScriptFile();
               
                if (this.Argument.GetType().Name.StartsWith("OutArgument"))
                {
                    Argument typedArgumentInstance = typeBrowserWindow.CreateOutArgumentForSelectedType();
                    typedArgumentInstance.Mode = this.Argument.Mode;
                    typedArgumentInstance.CanChangeMode = this.Argument.CanChangeMode;
                    typedArgumentInstance.CanChangeType = this.Argument.CanChangeType;
                    this.Argument = typedArgumentInstance;
                }

                if (this.Argument.GetType().Name.StartsWith("InArgument"))
                {
                    Argument typedArgumentInstance = typeBrowserWindow.CreateInArgumentForSelectedType();
                    typedArgumentInstance.Mode = this.Argument.Mode;
                    typedArgumentInstance.CanChangeMode = this.Argument.CanChangeMode;
                    typedArgumentInstance.CanChangeType = this.Argument.CanChangeType;
                    this.Argument = typedArgumentInstance;
                }               
            }

        }

        protected void DeleteScriptFile()
        {
            //Delete the script file if any
            try
            {
                if (this.Argument.Mode == ArgumentMode.Scripted)
                {
                    if (!string.IsNullOrEmpty(this.Argument.ScriptFile))
                    {
                        string processDirectory = this.OwnerComponent.EntityManager.WorkingDirectory;
                        string fileToDelete = Path.Combine(processDirectory, "Scripts", this.Argument.ScriptFile);
                        if (File.Exists(fileToDelete))
                            File.Delete(fileToDelete);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (Argument?.Mode != ArgumentMode.DataBound)
                return;
            if (e.Property == ArgumentProperty)
            {
                this.AvailableProperties.Clear();
                LoadAvailableProperties();
            }
        }

    }
}
