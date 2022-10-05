using McMaster.Extensions.CommandLineUtils;
using Ninject;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.Serialization;
using Pixel.Automation.Test.Runner.Modules;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption("-h|--help");
           
            var debug = app.Option("-d | --debug", $"Prompt to attach debugger on start", CommandOptionType.SingleOrNoValue);

            var clean = app.Option("-c || --clean", "Clean local data before execution", CommandOptionType.NoValue);
            var run = app.Option("-r || --run", "Indicates that test should be executed", CommandOptionType.NoValue);
            var list = app.Option("-l | --list", "List all the tests that matches selection criteria", CommandOptionType.NoValue);
                   
            var newTemplate = app.Option("-nt | --newTemplate", "Create a new session template", CommandOptionType.NoValue);          
            var listTemplate = app.Option("-lt | --listTemplate", "List all the available templates e.g. --listTemplate", CommandOptionType.NoValue);
            var modifyTemplate = app.Option("-mt | --modifyTemplate", "Modify a template given it's name e.g. --modifyTemplate:\"TemplateName\"", CommandOptionType.SingleValue);
            var template =  app.Option("-t | --template", "Execute the specified template e.g. --template:\"TemplateName\"", CommandOptionType.SingleValue);


            var project = app.Option("-p | --project", "Target project containing tests e.g. --project:\"ProjectName\"", CommandOptionType.SingleOrNoValue);
            var version = app.Option("-v | --version", "Deployed version of target project to use --version:\"n.0.0.0\"", CommandOptionType.SingleOrNoValue);          
            var where = app.Option("-w | --where", "Test selector function e.g. fixture.Category == \"SomeCategory\" && (test.Priority == Priority.Medium || test.Tags[\"key\"] != \"value\")", CommandOptionType.SingleOrNoValue);
            var script = app.Option("-s | --script", $"Script file (*.csx) override that can be used to initialize the process data model e.g. --script:\"CustomScript.csx\". By default {Constants.InitializeEnvironmentScript} generated at design time is used", CommandOptionType.SingleOrNoValue);


            Log.Logger = new LoggerConfiguration()
               .Enrich.WithThreadId()
               .WriteTo.Console(Serilog.Events.LogEventLevel.Information, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Thread:{ThreadId}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
               .WriteTo.File("logs\\Pixel-Automation-.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                 outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Thread:{ThreadId}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
               .CreateLogger();
            var logger = Log.ForContext<Program>();

            template.DefaultValue = "Bing_Demo_Template";

            app.OnExecuteAsync(async c =>
            {
                if(debug.HasValue())
                {
                    Debugger.Launch();
                }

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

                if (clean.HasValue())
                {
                    applicationDataManager.CleanLocalData();
                }

                logger.Information("Downloading application data now");
                await applicationDataManager.DownloadApplicationsDataAsync();
                logger.Information("Download of application data completed");
                logger.Information("Downloading project information now");
                await applicationDataManager.DownloadProjectsAsync();
                logger.Information("Download of project information completed");

                var projectManager = kernel.Get<ProjectManager>();
                var templateManager = kernel.Get<TemplateManager>();

                if (newTemplate.HasValue())
                {
                    await templateManager.CreateNewAsync();
                    return await Task.FromResult<int>(1);
                }
                if(listTemplate.HasValue())
                {
                    await templateManager.ListAllAsync();
                    return await Task.FromResult<int>(1);
                }
                if (modifyTemplate.HasValue())
                {
                    var template = await templateManager.GetByNameAsync(modifyTemplate.Value());
                    await templateManager.UpdateAsync(template);
                    return await Task.FromResult<int>(1);
                }

                SessionTemplate sessionTemplate;
                if(template.HasValue())
                {
                    sessionTemplate = await templateManager.GetByNameAsync(template.Value());
                    if(sessionTemplate == null)
                    {
                        logger.Warning("Template with name : {0} doesn't exist", template.Value());                       
                    }
                }
                else if (project.HasValue() && version.HasValue() && where.HasValue())
                {
                    sessionTemplate = new SessionTemplate()
                    {
                        Name = Guid.NewGuid().ToString(),
                        ProjectName = project.Value(),
                        ProjectVersion = version.Value(),
                        Selector = where.Value()
                    };  
                    if(script.HasValue())
                    {
                        sessionTemplate.InitializeScript = script.Value();
                    }
                }
                else
                {
                    logger.Error("No operation specified to execute or insufficient parameters for operation to be executed.");
                    app.ShowHelp();
                    Console.WriteLine("Press any key to exit");
                    Console.ReadLine();
                    return await Task.FromResult<int>(1);
                }

             
                await projectManager.LoadProjectAsync(sessionTemplate);
                projectManager.LoadTestCases();
               
                if (list.HasValue())
                {
                    await projectManager.ListAllAsync(sessionTemplate.Selector);
                }
                else
                {
                    // use --run as default if --list is not specified
                    await projectManager.RunAllAsync(sessionTemplate.Selector);
                }
                return await Task.FromResult<int>(0);

            });

            Log.CloseAndFlush();

            return app.Execute(args);

        }
    }
}
