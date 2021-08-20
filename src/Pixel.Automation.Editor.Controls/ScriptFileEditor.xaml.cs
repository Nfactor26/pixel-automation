using Caliburn.Micro;
using Microsoft.Win32;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ScriptFileEditor.xaml
    /// </summary>
    public partial class ScriptFileEditor : UserControl, ITypeEditor
    {
        private readonly ILogger logger = Log.ForContext<ScriptFileEditor>();

        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(ScriptFileEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ScriptFileEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }     

        /// <summary>
        /// Display name for the current model property for which editor is resolved
        /// </summary>
        private string propertyDisplayName;

        /// <summary>
        /// constructor
        /// </summary>
        public ScriptFileEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Resolve the editor to be displayed on property grid for specified property
        /// </summary>
        /// <param name="propertyItem">Property on the model object which is being displayed on property grid</param>
        /// <returns></returns>
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.propertyDisplayName = propertyItem.DisplayName;
            this.OwnerComponent = propertyItem.Instance as Component;
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ScriptFileEditor.ScriptFileProperty, binding);
            return this;
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
                if (OwnerComponent == null || string.IsNullOrEmpty(ScriptFile))
                {
                    return;
                }

                var entityManager = this.OwnerComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                await editorFactory.CreateAndShowDialogAsync(windowManager, OwnerComponent, this.ScriptFile, (a) => { return string.Empty; });
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
                string existingScriptFile = this.ScriptFile;

                var entityManager = this.OwnerComponent.EntityManager;
                var fileSystem = entityManager.GetServiceOfType<IProjectFileSystem>();

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSX File (*.csx)|*.csx";
                openFileDialog.InitialDirectory = fileSystem.ScriptsDirectory;
                              
                if (openFileDialog.ShowDialog() == true)
                {
                    this.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, openFileDialog.FileName);
                    logger.Information("ScriptFile for {Property} belonging to {Component} was updated to {ScriptFile} ",this.propertyDisplayName, OwnerComponent, this.ScriptFile);
                    return;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
    }
}
