using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Editor.Notifications.Notfications;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Buttons;

/// <summary>
/// Interaction logic for OpenPrefabMenuButton.xaml
/// </summary>
public partial class OpenPrefabMenuButton : UserControl
{  
    public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(OpenPrefabMenuButton),
                                                                                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
    public Component OwnerComponent
    {
        get { return (Component)GetValue(OwnerComponentProperty); }
        set { SetValue(OwnerComponentProperty, value); }
    }

    public OpenPrefabMenuButton()
    {
        InitializeComponent();
    }

    void OpenForEdit(object sender, RoutedEventArgs e)
    {
        if (this.OwnerComponent is PrefabHostEntity prefabHost)
        {
            var eventAggregator = prefabHost.EntityManager.GetServiceOfType<IEventAggregator>();
            eventAggregator.PublishOnUIThreadAsync(new OpenPrefabRequestNotification(default, default)
            {
                PrefabId = prefabHost.PrefabId,
                PrefabHost = prefabHost
            });
        }       
    }
}
