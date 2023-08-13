using Microsoft.Extensions.Configuration;
using Ninject;
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

            try
            {
                logger.Information("Started with args : {0}", args);
                var pluginManager = new PluginManager(new JsonSerializer());
                var platformFeatureAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.PlatformFeature);
                var pluginAssemblies = pluginManager.LoadPluginsFromDirectory("Plugins", PluginType.Component);

                var kernel = new StandardKernel(new CommonModule(), new ScopedModules(), new ScriptingModule(),
                    new NativeModule(platformFeatureAssemblies), new SettingsModule(), new PersistenceModules());
                kernel.Settings.Set("InjectAttribute", typeof(InjectedAttribute));

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

                var applicationDataManager = kernel.Get<IApplicationDataManager>();
                await applicationDataManager.UpdateApplicationRepository();
                var projectDataManger = kernel.Get<IProjectDataManager>();
                await projectDataManger.DownloadProjectsAsync();
                var prefabDataManager = kernel.Get<IPrefabDataManager>();
                await prefabDataManager.DownloadPrefabsAsync();

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
