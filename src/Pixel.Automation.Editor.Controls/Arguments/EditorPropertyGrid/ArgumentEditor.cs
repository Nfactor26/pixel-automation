using Caliburn.Micro;
using Microsoft.Win32;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    public class ArgumentEditorBase : UserControl
    {
        private readonly ILogger logger = Log.ForContext<ArgumentEditorBase>();
        protected PropertyItem propertyItem;

        public static readonly DependencyProperty ArgumentProperty = DependencyProperty.Register("Argument", typeof(Argument), typeof(ArgumentEditorBase),
                                                                                         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public Argument Argument
        {
            get { return (Argument)GetValue(ArgumentProperty); }
            set { SetValue(ArgumentProperty, value); }
        }

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ArgumentEditorBase),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        public static readonly DependencyProperty AvailablePropertiesProperty = DependencyProperty.Register("AvailableProperties", typeof(ObservableCollection<string>), typeof(ArgumentEditorBase),
                                                                                           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public ObservableCollection<string> AvailableProperties
        {
            get { return (ObservableCollection<string>)GetValue(AvailablePropertiesProperty); }
            set { SetValue(AvailablePropertiesProperty, value); }
        }

        public ArgumentEditorBase()
        {
            AvailableProperties = new ObservableCollection<string>();
        }


        protected void LoadAvailableProperties()
        {
            if (Argument?.Mode != ArgumentMode.DataBound)
            {
                return;
            }

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

        protected void InitializeScriptName()
        {
            var entityManager = this.OwnerComponent.EntityManager;
            if (string.IsNullOrEmpty(Argument.ScriptFile))
            {
                var fileSystem = entityManager.GetCurrentFileSystem();
                Argument.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid()}.csx"));
            }
        }

        /// <summary>
        /// Open the script editor to edit the script file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async void ShowScriptEditor(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OwnerComponent == null && Argument == null)
                {
                    return;
                }
                InitializeScriptName();

                var entityManager = this.OwnerComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                Func<IComponent, string> scriptDefaultValueGetter = (a) =>
                {
                    return Argument.GenerateInitialScript();
                };
                await editorFactory.CreateAndShowDialogAsync(windowManager, OwnerComponent, Argument.ScriptFile, scriptDefaultValueGetter);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Open the file browser to browse for a script file.       
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BrowseScript(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OwnerComponent == null)
                {
                    return;
                }
                string existingScriptFile = Argument.ScriptFile;

                var entityManager = this.OwnerComponent.EntityManager;
                var fileSystem = entityManager.GetServiceOfType<IProjectFileSystem>();

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSX File (*.csx)|*.csx";
                openFileDialog.InitialDirectory = fileSystem.ScriptsDirectory;

                if (openFileDialog.ShowDialog() == true)
                {
                    Argument.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, openFileDialog.FileName);
                    logger.Information("ScriptFile for argument belonging to {Component} was updated to {ScriptFile} ", OwnerComponent, Argument.ScriptFile);
                    return;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async void ChangeArgumentType(object sender, RoutedEventArgs e)
        {
            //Using outside automation process such as Application in ApplicationRepository
            if (this.OwnerComponent?.EntityManager == null)
            {
                return;
            }

            var entityManager = this.OwnerComponent.EntityManager;
            IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
            var argumentBrowserFactory = entityManager.GetServiceOfType<IArgumentTypeBrowserFactory>();
            var typeBrowserWindow = argumentBrowserFactory.CreateArgumentTypeBrowser();

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

                this.OwnerComponent.GetType().GetProperty(propertyItem.PropertyName).SetValue(propertyItem.Instance, this.Argument);

            }

        }

        /// <summary>
        /// Delete script file if any
        /// </summary>
        protected void DeleteScriptFile()
        {
            try
            {
                if (this.Argument.Mode == ArgumentMode.Scripted)
                {
                    if (!string.IsNullOrEmpty(this.Argument.ScriptFile))
                    {
                        string processDirectory = this.OwnerComponent.EntityManager.GetCurrentFileSystem().WorkingDirectory; ;
                        string fileToDelete = Path.Combine(processDirectory, "Scripts", this.Argument.ScriptFile);
                        if (File.Exists(fileToDelete))
                        {
                            File.Delete(fileToDelete);
                            logger.Information($"Deleted script file : {fileToDelete}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (Argument?.Mode != ArgumentMode.DataBound)
            {
                return;
            }
            if (e.Property == ArgumentProperty)
            {
                this.AvailableProperties.Clear();
                LoadAvailableProperties();
            }
        }

    }
}
