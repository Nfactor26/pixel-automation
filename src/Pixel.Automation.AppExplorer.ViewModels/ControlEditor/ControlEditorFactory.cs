using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public class ControlEditorFactory : IControlEditorFactory
    {
        private readonly IApplicationDataManager applicationDataManager;
        private readonly INotificationManager notificationManager;
       
        public ControlEditorFactory(IApplicationDataManager applicationDataManager, INotificationManager notificationManager)
        {
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
        }

        public IControlEditor CreateControlEditor(IControlIdentity controlIdentity)
        {
            if(controlIdentity is IImageControlIdentity)
            {
                return new ImageControlEditorViewModel(this.applicationDataManager, notificationManager);
            }
            return new ControlEditorViewModel();
        }
    }
}
