using Caliburn.Micro;
using Ninject.Modules;
using Pixel.Automation.AppExplorer.ViewModels.ControlEditor;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ViewModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            Kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();

            Kernel.Bind<IShell>().To<ShellViewModel>().InSingletonScope();
            Kernel.Bind<IHome>().To<HomeViewModel>().InSingletonScope();
            Kernel.Bind<INewProject>().To<NewProjectViewModel>();
            Kernel.Bind<IAutomationBuilder>().To<AutomationBuilderViewModel>();
            Kernel.Bind<IPrefabEditor>().To<PrefabEditorViewModel>();
            Kernel.Bind<IControlEditor>().To<ControlEditorViewModel>();

            Kernel.Bind<IArgumentExtractor>().To<ArgumentExtractor>().InSingletonScope();
            Kernel.Bind<IScriptExtactor>().To<ScriptExtractor>().InSingletonScope();
        }
    }
}
