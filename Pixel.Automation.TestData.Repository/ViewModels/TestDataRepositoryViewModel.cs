using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Notfications;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    public class TestDataRepositoryViewModel : ToolBox , IHandle<ShowTestDataSourceNotification>
    {
        public TestDataRepository ActiveInstance { get; set; }

        public override PaneLocation PreferredLocation => PaneLocation.Bottom;

        public override bool IsActiveItem
        {
            get => this.isActiveItem;
            set
            {
                this.isActiveItem = value;
                NotifyOfPropertyChange(() => IsActiveItem);
                //whenever this is no more ActiveItem, set selected test data source to null.
                if (value == false && this.ActiveInstance != null)
                {
                    this.ActiveInstance.SelectedTestDataSource = null;
                }
            }
        }

        public TestDataRepositoryViewModel(IEventAggregator eventAggregator)
        {
            this.DisplayName = "Test Data Repository";
            eventAggregator.SubscribeOnPublishedThread(this);
        }

        public bool IsTestProcessOpen
        {
            get => this.ActiveInstance != null;
        }

        public void SetActiveInstance(object instance)
        {
            if (instance is TestDataRepository)
            {
                this.ActiveInstance = instance as TestDataRepository;
                NotifyOfPropertyChange(nameof(ActiveInstance));
                NotifyOfPropertyChange(nameof(IsTestProcessOpen));
            }
        }

        public void CloseActiveInstance()
        {
            this.ActiveInstance = null;
            NotifyOfPropertyChange(nameof(ActiveInstance));
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        }
       

        public Task HandleAsync(ShowTestDataSourceNotification message, CancellationToken cancellationToken)
        {
            if (message != null && this.ActiveInstance != null)
            {
                this.IsSelected = true;
                this.ActiveInstance.FilterText = message.TestDataId ?? string.Empty;

            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// One of the list box item can be directly selected without actually activating the view.
        /// Make sure that whenever an item is selected, we set IsActiveItem to true.
        /// </summary>
        public void Activate()
        {
            this.IsActiveItem = true;
        }
    }
}
