using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Ninject;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.Modules;
using Pixel.Automation.Designer.ViewModels.Shell;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.Serialization;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{
    public class AppBootstrapper : BootstrapperBase
    {
        private ILogger logger;

        private IKernel kernel;

        private IConfiguration configuration;
           
        public AppBootstrapper()
        {
            #if DEBUG
            ConsoleManager.Show();
            #endif
           
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

          
            logger = Log.ForContext<AppBootstrapper>();
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
          
            var applicationSettings = IoC.Get<ApplicationSettings>();
            if (!applicationSettings.IsOfflineMode)
            {
                using (var resetEvent = new ManualResetEvent(false))
                {
                    var downloadApplicationDataTask = new Task(async () =>
                    {
                        using(var downloadDataActivity = Telemetry.DefaultSource?.StartActivity(nameof(OnStartup), ActivityKind.Internal))
                        {
                            try
                            {

                                var applicationDataManger = IoC.Get<IApplicationDataManager>();
                                var projectDataManager = IoC.Get<IProjectDataManager>();
                                var prefabDataManager = IoC.Get<IPrefabDataManager>();
                                using (Telemetry.DefaultSource?.StartActivity("download-applications-and-controls", ActivityKind.Internal))
                                {
                                    await applicationDataManger.DownloadApplicationsWithControlsAsync();
                                }
                                using (Telemetry.DefaultSource?.StartActivity("download-projects", ActivityKind.Internal))
                                {
                                    await projectDataManager.DownloadProjectsAsync();
                                }
                                using (Telemetry.DefaultSource?.StartActivity("download-prefabs", ActivityKind.Internal))
                                {
                                    await prefabDataManager.DownloadPrefabsAsync();
                                }

                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message, ex);                                
                                downloadDataActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                            }
                            finally
                            {
                                resetEvent.Set();
                            }
                        }                        
                    });
                    downloadApplicationDataTask.Start();
                    resetEvent.WaitOne();
                }
            }
            else
            {
                logger.Information("Application is configured to run in offline mode.");
            }
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

            foreach (var item in Directory.GetFiles(".", "*.Views*.dll", SearchOption.TopDirectoryOnly))
            {
                viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                logger.Information($"Added {item} to view assemblies.");
            }
            foreach (var item in Directory.GetFiles(".", "*.Editor*.dll", SearchOption.TopDirectoryOnly))
            {
                viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                logger.Information($"Added {item} to view assemblies.");
            }
            //Scrapper plugins can have some views used by them
            foreach (var item in Directory.GetFiles(".\\Plugins", "Pixel.Automation.*.dll", SearchOption.AllDirectories))
            {
                if (item.Contains("Scrapper"))
                {
                    viewAssemblies.Add(Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, item)));
                    logger.Information($"Added {item} to view assemblies.");
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

                Telemetry.InitializeDefault("pixel-design", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                string[] enablesSources = configuration.GetSection("OpenTelemetry:Sources").Get<string[]>();

                var traceProviderBuilder = Sdk.CreateTracerProviderBuilder()
                .SetSampler(new TraceIdRatioBasedSampler(0.1))
                .AddSource(enablesSources).ConfigureResource(resource => resource.AddService("pixel-design")).AddHttpClientInstrumentation();
                string otlpTraceEndPoint = configuration["OpenTelemetry:Trace:EndPoint"];             
                if (!string.IsNullOrEmpty(otlpTraceEndPoint))
                {
                    Enum.TryParse<OtlpExportProtocol>(configuration["OpenTelemetry:TraceExporter:OtlpExportProtocol"] ?? "HttpProtobuf", out OtlpExportProtocol exportProtocol);
                    Enum.TryParse<ExportProcessorType>(configuration["OpenTelemetry:TraceExporter:ExportProcessorType"] ?? "Batch", out ExportProcessorType processorType);
                    traceProviderBuilder.AddOtlpExporter(e =>
                    {
                        e.Endpoint = new Uri(otlpTraceEndPoint);
                        e.Protocol = exportProtocol;
                        e.ExportProcessorType = processorType;
                    });
                    logger.Information("Otlp Exporter was enabled. Endpoint is {0}", otlpTraceEndPoint);
                }             
                #if DEBUG
                traceProviderBuilder.AddConsoleExporter();
                #endif
                var traceProvider = traceProviderBuilder.Build();

                var pluginManager = new PluginManager(new JsonSerializer());
                var platformFeatureAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.PlatformFeature);
                var pluginAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.Component);
                var scrapperAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.Scrapper);
                kernel = new StandardKernel(new ViewModules(), new AnchorableModules(), new ScrappersModules(scrapperAssemblies), new GlobalScriptingModules(),
                    new CodeGeneratorModules(), new PersistenceModules(), new UtilityModules(), new NativeModules(platformFeatureAssemblies), new SettingsModules());
                kernel.Settings.Set("InjectAttribute", typeof(InjectedAttribute));
                kernel.Bind<TracerProvider>().ToConstant(traceProvider);
                var knownTypeProvider = kernel.Get<ITypeProvider>();
                knownTypeProvider.LoadTypesFromAssembly(typeof(Entity).Assembly);
                knownTypeProvider.LoadTypesFromAssembly(typeof(ControlEntity).Assembly);
                foreach (var assembly in pluginAssemblies)
                {
                    knownTypeProvider.LoadTypesFromAssembly(assembly);
                }

                pluginManager.ListLoadedAssemblies();
                Debug.Assert(AssemblyLoadContext.All.Count() == 1, "Multiple assembly load context exists.");

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                Debug.Assert(false, ex.Message);
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
            catch (TypeLoadException ex)
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
            if (instances != null)
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
