using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for ConfigurePrefabMenuButton.xaml
    /// </summary>
    public partial class ConfigurePrefabMenuButton : UserControl
    {
        private readonly ILogger logger = Log.ForContext<ConfigurePrefabMenuButton>();

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ConfigurePrefabMenuButton),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        public static readonly DependencyProperty InputMappingScriptFileProperty = DependencyProperty.Register("InputMappingScriptFile", typeof(string), typeof(ConfigurePrefabMenuButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public string InputMappingScriptFile
        {
            get { return (string)GetValue(InputMappingScriptFileProperty); }
            set { SetValue(InputMappingScriptFileProperty, value); }
        }

        public static readonly DependencyProperty OutputMappingScriptFileProperty = DependencyProperty.Register("OutputMappingScriptFile", typeof(string), typeof(ConfigurePrefabMenuButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public string OutputMappingScriptFile
        {
            get { return (string)GetValue(OutputMappingScriptFileProperty); }
            set { SetValue(OutputMappingScriptFileProperty, value); }
        }

        public ConfigurePrefabMenuButton()
        {
            InitializeComponent();
        }

        private void ConfigureInputMapping(object sender, RoutedEventArgs e)
        {
            try
            {
                var entityManager = this.OwnerComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                _ = editorFactory.CreateAndShowScriptEditorScreenAsync(windowManager, this.OwnerComponent, this.InputMappingScriptFile, (a) => { return string.Empty; });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        private void ConfigureOutputMapping(object sender, RoutedEventArgs e)
        {
            try
            {
                var entityManager = this.OwnerComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                _ = editorFactory.CreateAndShowScriptEditorScreenAsync(windowManager, this.OwnerComponent, this.OutputMappingScriptFile, (a) => { return string.Empty; });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            //We need to ensure that the prefab is loaded. If Prefab is not loaded, ScriptEditorFactory will have
            //missing references for Prefab data model assembly
            if(this.OwnerComponent is PrefabEntity prefabEntity)
            {
                prefabEntity.LoadPrefab();
            }
            if(sender is Button button)
            {
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
