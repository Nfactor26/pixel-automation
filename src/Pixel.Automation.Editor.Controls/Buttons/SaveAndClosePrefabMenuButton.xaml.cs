using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Editor.Notifications;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for SaveAndClosePrefabMenuButton.xaml
    /// </summary>
    public partial class SaveAndClosePrefabMenuButton : UserControl
    {
        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(SaveAndClosePrefabMenuButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        public SaveAndClosePrefabMenuButton()
        {
            InitializeComponent();
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {           
            if (sender is Button button)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        void SaveChanges(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent is PrefabHostEntity prefabHost)
            {
                var eventAggregator = prefabHost.EntityManager.GetServiceOfType<IEventAggregator>();
                eventAggregator.PublishOnUIThreadAsync(new SavePrefabRequestNotification(prefabHost.PrefabId, prefabHost));
            }
        }

        void ClosePrefab(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent is PrefabHostEntity prefabHost)
            {
                var eventAggregator = prefabHost.EntityManager.GetServiceOfType<IEventAggregator>();
                eventAggregator.PublishOnUIThreadAsync(new ClosePrefabRequestNotification(prefabHost.PrefabId, prefabHost, false));
            }
        }

        void SaveAndClosePrefab(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent is PrefabHostEntity prefabHost)
            {
                var eventAggregator = prefabHost.EntityManager.GetServiceOfType<IEventAggregator>();
                eventAggregator.PublishOnUIThreadAsync(new ClosePrefabRequestNotification(prefabHost.PrefabId, prefabHost, true));
            }
        }
    }
}
