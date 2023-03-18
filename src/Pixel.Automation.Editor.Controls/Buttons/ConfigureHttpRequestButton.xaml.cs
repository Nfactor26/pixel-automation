using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for ConfigureHttpRequestButton.xaml
    /// </summary>
    public partial class ConfigureHttpRequestButton : UserControl
    {
        private readonly ILogger logger = Log.ForContext<ConfigurePrefabMenuButton>();

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ConfigureHttpRequestButton),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// Actor component that will execute the HttpRequest
        /// </summary>
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        public static readonly DependencyProperty HttpRequestProperty = DependencyProperty.Register("HttpRequest", typeof(object), typeof(ConfigureHttpRequestButton),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// HttpRequest configuration
        /// </summary>
        public object HttpRequest
        {
            get { return (object)GetValue(HttpRequestProperty); }
            set { SetValue(HttpRequestProperty, value); }
        }

        public static readonly DependencyProperty ResponseContentHandlingProperty = DependencyProperty.Register("ResponseContentHandling", typeof(object), typeof(ConfigureHttpRequestButton),
                                                                                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// HttpResponse content configuration
        /// </summary>
        public object ResponseContentHandling
        {
            get { return (object)GetValue(ResponseContentHandlingProperty); }
            set { SetValue(ResponseContentHandlingProperty, value); }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public ConfigureHttpRequestButton()
        {
            InitializeComponent();
        }                  

        /// <summary>
        /// Show the configuration window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfigureHttpRequest(object sender, RoutedEventArgs e)
        {
            try
            {
                var entityManager = this.OwnerComponent.EntityManager;
                IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
                IHttpRequestEditor httpRequestEditor = entityManager.GetServiceOfType<IHttpRequestEditor>();
                httpRequestEditor.Initialize(this.HttpRequest, this.ResponseContentHandling, this.OwnerComponent);
                await windowManager.ShowDialogAsync(httpRequestEditor);
                
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

    }
}
