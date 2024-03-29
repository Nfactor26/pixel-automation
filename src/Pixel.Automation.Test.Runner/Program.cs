﻿using Microsoft.Extensions.Configuration;
using Ninject;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.Serialization;
using Pixel.Automation.Test.Runner.Commands;
using Pixel.Automation.Test.Runner.Modules;
using Serilog;
using Spectre.Console.Cli;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            string[] filterHttpRequests = configuration.GetSection("OpenTelemetry:FilterRequests").Get<string[]>() ?? Array.Empty<string>();
            Regex[] filterHttpRequestsMatcher = new Regex[filterHttpRequests.Length];
            for (int i = 0; i < filterHttpRequests.Length; i++)
            {
                filterHttpRequestsMatcher[i] = new Regex(filterHttpRequests[i], RegexOptions.Compiled);
            }

            double.TryParse(configuration["OpenTelemetry:Sampling:Ratio"], out double samplingProbablity);
            var traceProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingProbablity)))
            .AddSource(enablesSources).ConfigureResource(resource => resource.AddService("pixel-run"))
            .AddHttpClientInstrumentation(options =>
             {
                 if (filterHttpRequestsMatcher.Any())
                 {
                     options.FilterHttpRequestMessage = (message) =>
                     {
                         foreach (var pattern in filterHttpRequestsMatcher)
                         {
                             if (pattern.IsMatch(message.RequestUri.OriginalString))
                             {
                                 return false;
                             }
                         }
                         return true;
                     };
                 }
             });
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
                        .WithExample(new[] { "run", "template", "template-name", "--list"  });
                        run.AddCommand<ExecuteAdhocTestCommand>("adhoc").WithDescription("Execute tests without using a template")
                        .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "true", "InitializeDefault()" })
                         .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "true", "InitializeDefault()", "--list" })
                        .WithExample(new[] { "run", "adhoc", "project-name", "project-version", "\"fixture.Name.Equals(\"fixture-name\") &&" +
                        " test.Name.Contains(\"test-prefix\")\"", "InitializeDefault()" });
                        run.AddCommand<ExecuteTestWithImportCommand>("with-import").WithDescription("Execute tests by importing a zip file that contains automation process and dependencies")
                        .WithExample(new[] { "run", "with-import", "zip-file-path", "true", "InitializeDefault()" })
                        .WithExample(new[] { "run", "with-import", "zip-file-path", "true", "InitializeDefault()", "--list" })
                        .WithExample(new[] { "run", "with-import", "zip-file-path", "\"fixture.Name.Equals(\"fixture-name\") &&" +
                        " test.Name.Contains(\"test-prefix\")\"", "InitializeDefault()" });
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
