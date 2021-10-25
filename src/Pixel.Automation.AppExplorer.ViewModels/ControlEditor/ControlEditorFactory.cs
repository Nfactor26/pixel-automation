using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public class ControlEditorFactory : IControlEditorFactory
    {
        private readonly IApplicationDataManager applicationDataManager;
       
        public ControlEditorFactory(IApplicationDataManager applicationDataManager)
        {
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
        }

        public IControlEditor CreateControlEditor(IControlIdentity controlIdentity)
        {
            if(controlIdentity is IImageControlIdentity)
            {
                return new ImageControlEditorViewModel(this.applicationDataManager);
            }
            return new ControlEditorViewModel();
        }
    }
}
