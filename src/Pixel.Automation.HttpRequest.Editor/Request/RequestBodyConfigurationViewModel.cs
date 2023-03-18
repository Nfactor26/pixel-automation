using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor
{
    /// <summary>
    /// View model for configuring the <see cref="RequestBody"/>
    /// </summary>
    public class RequestBodyConfigurationViewModel : Screen, IConfigurationScreen
    {
        /// <summary>
        /// Actor component for executing the rest request
        /// </summary>
        public Component OwnerComponent { get; private set; }
       
        /// <summary>
        /// HttpRequest to configure
        /// </summary>
        public RestHttpRequest HttpRequest { get; private set; }
       
        /// <summary>
        /// Collection of tabs for configuration of request body
        /// </summary>
        public BindableCollection<Screen> BodyConfigurationScreens { get; set; } = new();

        private int selectedIndex;
        /// <summary>
        /// Index of the selected tab
        /// </summary>
        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                this.selectedIndex = value;
                this.BodyConfigurationScreens.ElementAt(value).ActivateAsync();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ownerComponent"></param>
        /// <param name="httpRequest"></param>
        public RequestBodyConfigurationViewModel(Component ownerComponent, RestHttpRequest httpRequest)
        {
            this.DisplayName = "Body";
            this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent)).NotNull();
            this.HttpRequest = Guard.Argument(httpRequest, nameof(httpRequest)).NotNull();
            this.BodyConfigurationScreens.Add(new EmptyDataBodyConfigurationViewModel(this.OwnerComponent, this.HttpRequest));
            this.BodyConfigurationScreens.Add(new FormDataBodyConfigurationViewModel(this.OwnerComponent, this.HttpRequest));
            this.BodyConfigurationScreens.Add(new BinaryDataBodyConfigurationViewModel(this.OwnerComponent, this.HttpRequest));
            this.BodyConfigurationScreens.Add(new RawDataBodyConfigurationViewModel(this.OwnerComponent, this.HttpRequest));
            switch(this.HttpRequest.RequestBody)
            {
                default:
                case EmptyBodyContent _:
                    this.SelectedIndex = 0;
                    break;
                case FormDataBodyContent _:
                    this.SelectedIndex = 1;
                    break;
                case BinaryBodyContent _:
                    this.SelectedIndex = 2;
                    break;
                case RawBodyContent _:
                    this.SelectedIndex = 3;
                    break;
            }
        }

        /// </inheritdoc>
        public void ApplyChanges(RestHttpRequest request)
        {
            if (this.BodyConfigurationScreens.ElementAt(this.SelectedIndex) is IConfigurationScreen cf)
            {
                cf.ApplyChanges(request);
            }
        }
    }
}
