using Caliburn.Micro;
using Ninject;
using Pixel.Automation.AppExplorer.ViewModels.ControlEditor;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Utilities;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Native.Windows;
using Pixel.Automation.RunTime;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{
    public class AppBootstrapper : BootstrapperBase
    {

        private IKernel kernel;

        public AppBootstrapper()
        {
           

            ConsoleManager.Show();

            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .WriteTo.ColoredConsole()
             .WriteTo.RollingFile("logs\\Pixel-Automation-{Date}.txt")
             .CreateLogger();

            Initialize();

        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootViewFor<IShell>();
        }

        /// <summary>
        /// Return all the Assemblies which will be searched for View by Caliburn micro
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            List<Assembly> viewAssemblies = new List<Assembly>();
            viewAssemblies.Add(Assembly.GetEntryAssembly());

            //Since .net core doesn't support sub directories easily. Workaround at the moment.
            foreach(var item in Directory.GetFiles(".", "*.dll",SearchOption.TopDirectoryOnly))
            {
                switch(item)
                {
                    case ".\\Pixel.Automation.Arguments.Editor.dll":
                    case ".\\Pixel.Automation.Prefabs.Editor.dll":
                    case ".\\Pixel.Automation.TestData.Repository.dll":
                    case ".\\Pixel.Automation.TestExplorer.dll":
                    case ".\\Pixel.Scripting.Script.Editor.dll":
                    case ".\\Pixel.Automation.AppExplorer.Views.dll":
                    //case ".\\Pixel.Automation.Designer.Views.dll":
                        viewAssemblies.Add(Assembly.LoadFrom(System.IO.Path.Combine(Environment.CurrentDirectory, item)));
                        break;
                }
            }
            //if(Directory.Exists("Editors"))
            //{
            //    foreach (var item in Directory.GetFiles("Editors", "Pixel.*.dll"))
            //    {
            //        viewAssemblies.Add(Assembly.LoadFrom(System.IO.Path.Combine(Environment.CurrentDirectory, item)));
            //    }
            //}
            //if (Directory.Exists("Scripting"))
            //{
            //    foreach (var item in Directory.GetFiles("Scripting", "Pixel.*.dll"))
            //    {
            //        viewAssemblies.Add(Assembly.LoadFile(System.IO.Path.Combine(Environment.CurrentDirectory, item)));
            //    }
            //}
            return viewAssemblies;
        }



        protected override void Configure()
        {
            try
            {
                if (Execute.InDesignMode)
                {
                    return;
                }


                LogManager.GetLog = type => new DebugLog(type);

                kernel = new StandardKernel(new ToolBoxModule(), new ScrappersModule(), new ScriptingModule(), new CodeGeneratorModule());                
                kernel.Settings.Set("InjectAttribute", typeof(InjectedAttribute));
             

                kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
                kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
                //kernel.Bind<IKernel>().ToConstant(kernel);
                kernel.Bind<ISerializer>().To<JsonSerializer>().InSingletonScope();


                //viewmodel bindings
                kernel.Bind<IShell>().To<ShellViewModel>().InSingletonScope();
                kernel.Bind<IHome>().To<HomeViewModel>().InSingletonScope();
                kernel.Bind<INewProject>().To<NewProjectViewModel>();              
                kernel.Bind<IAutomationBuilder>().To<AutomationBuilderViewModel>();
                kernel.Bind<IPrefabEditor>().To<PrefabEditorViewModel>();              
            

                kernel.Bind<ITypeProvider>().To<KnownTypeProvider>().InSingletonScope();              
                kernel.Bind<IServiceResolver>().To<ServiceResolver>();
                kernel.Bind<IControlEditor>().To<ControlEditorViewModel>();

                kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>();
                kernel.Bind<IPrefabFileSystem>().To<PrefabFileSystem>();
                kernel.Bind<ITestCaseFileSystem>().To<TestCaseFileSystem>();

                kernel.Bind<IApplicationWindowManager>().To<ApplicationWindowManager>().InSingletonScope();                
                kernel.Bind<IHighlightRectangleFactory>().To<HighlightRectangleFactory>().InSingletonScope();
                kernel.Bind<IScreenCapture>().To<ScreenCapture>().InSingletonScope();

                kernel.Bind<IPrefabLoader>().To<PrefabLoader>();

            }
            catch (Exception ex)
            {
                Log.Error(ex,ex.Message);
                Debug.Assert(false,ex.Message);
            }
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            try
            {
                if (serviceType == null)
                {
                    serviceType = Type.GetType(key);
                    if (serviceType == null)
                    {
                        throw new ArgumentNullException("serviceType argument is null");
                    }
                }
                var instance = kernel.Get(serviceType, key);
                if (instance != null)
                    return instance;
                throw new Exception(string.Format("Could not locate any instances of contract {0}.", serviceType.ToString()));
            }
            catch(TypeLoadException ex)
            {
                Log.Error(ex, ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw ex;
            }

            

        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var instances = kernel.GetAll(serviceType);
            if(instances!=null)
                return instances;
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", serviceType.ToString()));

        }       

        protected override void BuildUp(object instance)
        {
            base.BuildUp(instance);
        }
           

        protected override void OnExit(object sender, EventArgs e)
        {           
            base.OnExit(sender, e);
            kernel.Dispose();
        }

        protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Trace.WriteLine(e.Exception.Message);
            Debug.Assert(false);
        }


    }
}
