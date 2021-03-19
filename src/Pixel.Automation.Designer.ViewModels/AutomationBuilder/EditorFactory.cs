using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class EditorFactory : IEditorFactory
    {
        private IServiceResolver serviceResolver;

        public EditorFactory(IServiceResolver serviceResolver)
        {
            this.serviceResolver = serviceResolver;
            this.serviceResolver.RegisterDefault<IServiceResolver>(serviceResolver);
        }

        public IAutomationEditor CreateAutomationEditor()
        {
           return serviceResolver.Get<IAutomationEditor>();
        }

        public IPrefabEditor CreatePrefabEditor()
        {
            return serviceResolver.Get<IPrefabEditor>();
        }
    }
}
