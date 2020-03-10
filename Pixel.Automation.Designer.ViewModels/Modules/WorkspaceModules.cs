using Ninject.Modules;
using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Native.Windows;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.DataReader;
using Pixel.Automation.TestData.Repository.ViewModels;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class WorkspaceModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<AutomationProjectManager>().ToSelf();
            Kernel.Bind<PrefabProjectManager>().ToSelf();
            Kernel.Bind<TestDataRepository>().ToSelf();

            Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>().InSingletonScope();
            Kernel.Bind<IDataReader>().To<CsvDataReader>().InSingletonScope();
            Kernel.Bind<ITestDataLoader>().To<TestDataLoader>().InSingletonScope();
            Kernel.Bind<IArgumentTypeProvider>().To<ArgumentTypeProvider>().InSingletonScope();

            //If the Highglight rectangle is created on some other thread, it doesn't work. Delegate passed to BeginInvoke of forms never get executed.
            HighlightRectangle highlightRectangle = new HighlightRectangle();
            Kernel.Bind<IHighlightRectangle>().ToConstant<HighlightRectangle>(highlightRectangle);
        }
    }
}
