using Microsoft.Extensions.Configuration;
using Ninject;
using OpenTelemetry.Exporter;
using OpenTelemetry;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.Serialization;
using Pixel.Automation.Test.Runner.Commands;
using Pixel.Automation.Test.Runner.Modules;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using Spectre.Console.Cli;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

namespace Pixel.Automation.Test.Runner
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            var logger = Log.ForContext<Program>();

            Telemetry.InitializeDefault("pixel-run", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            string[] enablesSources = configuration.GetSection("OpenTelemetry:Sources").Get<string[]>();

            double.TryParse(configuration["OpenTelemetry:Sampling:Ratio"], out double samplingProbablity);
            var traceProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingProbablity)))
            .AddSource(enablesSources).ConfigureResource(resource => resource.AddService("pixel-run")).AddHttpClientInstrumentation();
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
            }
            #if DEBUG
            traceProviderBuilder.AddConsoleExporter();
            #endif
            var traceProvider = traceProviderBuilder.Build();

            TraceManager.IsEnabled = true;

            try
            {
                logger.Information("Started with args : {0}", args);
                var pluginManager = new PluginManager(new JsonSerializer());
                var platformFeatureAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.PlatformFeature);
                var pluginAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.Component);

                var kernel = new StandardKernel(new CommonModule(), new ScopedModules(), new ScriptingModule(),
                    new NativeModule(platformFeatureAssemblies), new SettingsModule(), new PersistenceModules());
                kernel.Settings.Set("InjectAttribute", typeof(InjectedAttribute));
                kernel.Bind<TracerProvider>().ToConstant(traceProvider);
             
                var knownTypeProvider = kernel.Get<ITypeProvider>();
                knownTypeProvider.LoadTypesFromAssembly(typeof(Entity).Assembly);
                knownTypeProvider.LoadTypesFromAssembly(typeof(ControlEntity).Assembly);
                foreach (var assembly in pluginAssemblies)
                {
                    knownTypeProvider.LoadTypesFromAssembly(assembly);
                }

                #if DEBUG
                pluginManager.ListLoadedAssemblies();
                #endif

                using (Telemetry.DefaultSource?.StartActivity("download-applications-and-controls", ActivityKind.Internal))
                {
                    var applicationDataManager = kernel.Get<IApplicationDataManager>();
                    await applicationDataManager.DownloadApplicationsWithControlsAsync();
                }
                using (Telemetry.DefaultSource?.StartActivity("download-projects", ActivityKind.Internal))
                {
                    var projectDataManger = kernel.Get<IProjectDataManager>();
                    await projectDataManger.DownloadProjectsAsync();
                }
                using (Telemetry.DefaultSource?.StartActivity("download-prefabs", ActivityKind.Internal))
                {
                    var prefabDataManager = kernel.Get<IPrefabDataManager>();
                    await prefabDataManager.DownloadPrefabsAsync();
                }            
          
                //System.Diagnostics.Debugger.Launch();
                var registrar = new TypeRegistrar(kernel);
                var app = new CommandApp(registrar);
                app.Configure(config =>
                {
                    config.AddBranch<ExecuteTestSettings>("run", run =>
                    {
                        run.SetDescription("Execute test cases");
                        run.AddCommand<ExecuteTestFromTemplateCommand>("template")
                        .WithDescription("Execute tests by specifying a template and optionally project version to use")
                        .WithExample(new[] { "run", "template", "template-name"})
                        .WithExample(new[] { "run", "template", "template-name", "--list"  })
                        .WithExample(new[] { "run", "template", "template-name", "1.0.0.0" });
                        run.AddCommand<ExecuteAdhocTestCommand>("adhoc").WithDescription("Execute tests without using a template")
                        .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "true", "InitializationScript.csx" })
                         .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "true", "InitializationScript.csx", "--list" })
                        .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "\"fixture.Name.Equals(\"fixture-name\") &&" +
                        " test.Name.Contains(\"test-prefix\")\"", "InitializationScript.csx" });
                    });
                    config.AddBranch<TemplateSettings>("template", template =>
                    {
                        template.SetDescription("create or edit or list templates");
                        template.AddCommand<CreateTemplateCommand>("new").WithDescription("Create a new template for tests")
                         .WithExample(new[] { "template", "new" });
                        template.AddCommand<ListTemplatesCommand>("list").WithDescription("List existing templates")
                        .WithExample(new[] { "template", "list" });
                        template.AddCommand<EditTemplateCommand>("update").WithDescription("Update an existing template")
                        .WithExample(new[] { "template", "edit" });
                    });
                    config.AddCommand<CleanDataCommand>("clean").WithDescription("Clean local data and download latest")
                    .WithExample(new[] { "clean" });
                });
                return await app.RunAsync(args);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
