using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Ninject;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{
    public class AppBootstrapper : BootstrapperBase
    {
        private readonly ILogger logger = Log.ForContext<AppBootstrapper>();

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
            var resetEvent = new ManualResetEvent(false);
            var applicationSettings = IoC.Get<ApplicationSettings>();
            if(!applicationSettings.IsOfflineMode)
            {
                var downloadApplicationDataTask = new Task(async () =>
                {
                    try
                    {
                        var applicationDataManger = IoC.Get<IApplicationDataManager>();
                        Log.Information("Downloading application data now");
                        await applicationDataManger.DownloadApplicationsDataAsync();
                        Log.Information("Download of application data completed");
                        Log.Information("Downloading project information now");
                        await applicationDataManger.DownloadProjectsAsync();
                        Log.Information("Download of project information completed");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }                  
                });
                downloadApplicationDataTask.Start();
                Log.Information("Waiting for data download");
                resetEvent.WaitOne();
            }
            else
            {
                Log.Information("Application is configured to run in offline mode.");
            }
            
            Log.Information("Initializing Root View now");
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
                        viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                        break;
                }
            }       
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

                kernel = new StandardKernel(new ViewModules(), new ToolBoxModules(), new ScrappersModules(), new ScriptingModules(),
                    new CodeGeneratorModules(), new PersistenceModules(), new UtilityModules(),  new WindowsModules(), new SettingsModules());                
                kernel.Settings.Set("InjectAttribute", typeof(InjectedAttribute));             

            }
            catch (Exception ex)
            {
                logger.Error(ex,ex.Message);
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
                        throw new ArgumentNullException($"{nameof(serviceType)} argument is null");
                    }
                }
                var instance = kernel.Get(serviceType, key);
                if (instance != null)
                {
                    return instance;
                }
                throw new Exception(string.Format("Could not locate any instances of contract {0}.", serviceType.ToString()));
            }
            catch(TypeLoadException ex)
            {
                logger.Error(ex, ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw ex;
            }        

        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var instances = kernel.GetAll(serviceType);
            if(instances!=null)
            {
                return instances;
            }
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
            logger.Error(e.Exception, e.Exception.Message);
        }


    }
}
