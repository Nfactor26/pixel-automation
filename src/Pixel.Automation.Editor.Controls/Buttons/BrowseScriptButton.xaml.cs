using Microsoft.Win32;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls
{
    /// <summary>
    /// Interaction logic for BrowseScriptButton.xaml
    /// </summary>
    public partial class BrowseScriptButton : UserControl
    {
        private readonly ILogger logger = Log.ForContext<BrowseScriptButton>();


        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(BrowseScriptButton),
                                                                                     new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty ActorComponentProperty = DependencyProperty.Register("ActorComponent", typeof(Component), typeof(BrowseScriptButton),
                                                                                                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component ActorComponent
        {
            get { return (Component)GetValue(ActorComponentProperty); }
            set { SetValue(ActorComponentProperty, value); }
        }

        public BrowseScriptButton()
        {
            InitializeComponent();
        }

        public void BrowseForScript(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ActorComponent == null)
                {
                    return;
                }
                string existingScriptFile = this.ScriptFile;

                var entityManager = this.ActorComponent.EntityManager;
                var fileSystem = entityManager.GetServiceOfType<IProjectFileSystem>();

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSX File (*.csx)|*.csx";
                openFileDialog.InitialDirectory = fileSystem.ScriptsDirectory;

                if (openFileDialog.ShowDialog() == true)
                {
                    this.ScriptFile = System.IO.Path.GetRelativePath(fileSystem.WorkingDirectory, openFileDialog.FileName);
                    logger.Information("ScriptFile {existingScriptFile} belonging to {Component} was updated to {ScriptFile} ", existingScriptFile, ActorComponent, this.ScriptFile);
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
