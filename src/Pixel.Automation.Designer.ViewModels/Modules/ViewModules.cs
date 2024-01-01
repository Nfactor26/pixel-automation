using Caliburn.Micro;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.ControlEditor;
using Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Designer.ViewModels.DragDropHandlers;
using Pixel.Automation.Designer.ViewModels.Factory;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.HttpRequest.Editor;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ViewModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            Kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            Kernel.Bind<INotificationManager>().To<NotificationManager>().InSingletonScope();
            Kernel.Bind<IPlatformProvider>().To<XamlPlatformProvider>().InSingletonScope();

            Kernel.Bind<IShell>().To<DesignerWindowViewModel>();
          
            Kernel.Bind<IHome>().To<HomeViewModel>().InSingletonScope();
            Kernel.Bind<INewProject>().To<NewProjectViewModel>();
            Kernel.Bind<IEditorFactory>().To<EditorFactory>();        
          
            Kernel.Bind<IControlEditorFactory>().To<ControlEditorFactory>();
           
            //Kernel.Bind<IPrefabBuilder>().To<PrefabBuilderViewModel>();
            //Kernel.Bind<IPrefabBuilderFactory>().ToFactory();

            Kernel.Bind<IArgumentExtractor>().To<ArgumentExtractor>().InSingletonScope();
            Kernel.Bind<IScriptExtactor>().To<ScriptExtractor>().InSingletonScope();

            Kernel.Bind<IDropTarget>().To<ComponentDropHandler>().InTransientScope();

            Kernel.Bind<IVersionManagerFactory>().To<VersionManagerFactory>();

            Kernel.Bind<IHttpRequestEditor>().To<HttpRequestConfigurationViewModel>();
        }
    }
}
