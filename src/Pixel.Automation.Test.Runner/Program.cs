using McMaster.Extensions.CommandLineUtils;
using Ninject;
using Pixel.Automation.Core;
using Pixel.Automation.Test.Runner.Modules;
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
            var project = app.Option("-p | --project", "Target project containing tests", CommandOptionType.SingleValue);
            var version = app.Option("-v | --version", "Deployed version of target project to use", CommandOptionType.SingleValue);
            var run = app.Option("-r || --run", "Indicates that test should be executed", CommandOptionType.NoValue);
            var list = app.Option("-l | --list", "List all the tests that matches selection criteria", CommandOptionType.NoValue);
            var where = app.Option("-w | --where", "Test selector function e.g. fixture.Category == \"SomeCategory\" && (test.Priority == Priority.Medium || test.Tags[\"key\"] != \"value\")", CommandOptionType.SingleValue);
            var script = app.Option("-s | --script", $"Script file (*.csx) override that can be used to initialize the process data model e.g. -s:\"CustomScript.csx\". By default {Constants.InitializeEnvironmentScript} generated at design time is used", CommandOptionType.SingleOrNoValue);
            var debug = app.Option("-d | --debug", $"Prompt to attach debugger on start", CommandOptionType.SingleOrNoValue);

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.ColoredConsole()
            .WriteTo.RollingFile("logs\\Pixel-Automation-{Date}.txt")
            .CreateLogger();

            var kernel = new StandardKernel(new CommonModule(), new DevicesModule(), new ScopedModules(), new ScriptingModule(), new WindowsModule(), new SettingsModule(), new PersistenceModules());
            var applicationDataManager = kernel.Get<IApplicationDataManager>();
            Log.Information("Downloading application data now");
            _ = applicationDataManager.DownloadApplicationsDataAsync();
            Log.Information("Download of application data completed");
            Log.Information("Downloading project information now");
            _ = applicationDataManager.DownloadProjectsAsync();
            Log.Information("Download of project information completed");

            //Debugger.Launch();

            app.OnExecuteAsync(async c =>
            {
                if(debug.HasValue())
                {
                    Debugger.Launch();
                }

                if (project.HasValue() && version.HasValue() && where.HasValue())
                {
                    var projectManager = kernel.Get<ProjectManager>();
                    await projectManager.LoadProjectAsync(project.Value(), version.Value(), script.Value());
                    projectManager.LoadTestCases();

                    if(run.HasValue())
                    {
                        await projectManager.RunAllAsync(where.Value());
                    }
                    if(list.HasValue())
                    {
                        await projectManager.ListAllAsync(where.Value());
                    }
                    return await Task.FromResult<int>(0);

                }
                else
                {
                    Log.Error("Required arguments are missing. Specify --run to run tests or --list to list down tests matching selection criteria along with project, version and where clause");
                    app.ShowHelp();
                    Console.WriteLine("Press any key to exit");
                    Console.ReadLine();
                    return await Task.FromResult<int>(1);
                }               
            });

            return app.Execute(args);

        }
    }
}
