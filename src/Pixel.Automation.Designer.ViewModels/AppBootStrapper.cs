﻿using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Ninject;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Automation.Designer.ViewModels.Shell;
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
        private ILogger logger;

        private IKernel kernel;
   
        public AppBootstrapper()
        {
            ConsoleManager.Show();

            Log.Logger = new LoggerConfiguration()              
              .Enrich.WithThreadId()           
              .WriteTo.Console(Serilog.Events.LogEventLevel.Information, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Thread:{ThreadId}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
              .WriteTo.File("logs\\Pixel-Automation-.txt",  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Thread:{ThreadId}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}",  rollingInterval: RollingInterval.Day)
              .CreateLogger();
            logger = Log.ForContext<AppBootstrapper>();
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootViewForAsync<MainWindowViewModel>();         
        }

        /// <summary>
        /// Return all the Assemblies which will be searched for View by Caliburn micro
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            List<Assembly> viewAssemblies = new List<Assembly>();
            viewAssemblies.Add(Assembly.GetEntryAssembly());
                      
            foreach(var item in Directory.GetFiles(".", "*.Views*.dll", SearchOption.TopDirectoryOnly))
            {
                viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                logger.Information($"Added {item} to view assemblies.");
            }
            foreach (var item in Directory.GetFiles(".", "*.Editor*.dll", SearchOption.TopDirectoryOnly))
            {
                viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                logger.Information($"Added {item} to view assemblies.");
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

                kernel = new StandardKernel(new ViewModules(), new AnchorableModules(), new ScrappersModules(), new GlobalScriptingModules(), new DevicesModules(),
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
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
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
            Log.CloseAndFlush();
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
