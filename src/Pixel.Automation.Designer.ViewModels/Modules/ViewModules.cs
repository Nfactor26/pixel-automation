using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.ControlEditor;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Designer.ViewModels.DragDropHandlers;
using Pixel.Automation.Designer.ViewModels.Factory;
using Pixel.Automation.Designer.ViewModels.Shell;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.OpenId.Authenticator;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ViewModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            Kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            Kernel.Bind<IPlatformProvider>().To<XamlPlatformProvider>().InSingletonScope();

            Kernel.Bind<IShell>().To<DesignerWindowViewModel>();
            Kernel.Bind<IShell>().To<LoginWindowViewModel>();
            Kernel.Bind<ISignInManager>().To<AuthorizationCodeSignInManager>().InSingletonScope();
           
            Kernel.Bind<IHome>().To<HomeViewModel>().InSingletonScope();
            Kernel.Bind<INewProject>().To<NewProjectViewModel>();
            Kernel.Bind<IEditorFactory>().To<EditorFactory>();        
          
            Kernel.Bind<IControlEditorFactory>().To<ControlEditorFactory>();
           
            Kernel.Bind<IPrefabBuilder>().To<PrefabBuilderViewModel>();
            Kernel.Bind<IPrefabBuilderFactory>().ToFactory();

            Kernel.Bind<IArgumentExtractor>().To<ArgumentExtractor>().InSingletonScope();
            Kernel.Bind<IScriptExtactor>().To<ScriptExtractor>().InSingletonScope();

            Kernel.Bind<IDropTarget>().To<ComponentDropHandler>().InTransientScope();

            Kernel.Bind<IVersionManagerFactory>().To<VersionManagerFactory>();
        }
    }
}
