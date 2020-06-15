using McMaster.Extensions.CommandLineUtils;
using Ninject;
using Pixel.Automation.Test.Runner.Modules;
using Serilog;
using System;
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
            var includedCategories = app.Option("-ic | --include-categories", "| seperated test categories to execute e.g. Category1|Category2. Run all categories if no value specified", CommandOptionType.SingleOrNoValue);
            var includedTags = app.Option("-it | --include-tags", "| seperated tags values. Only tests tagged with any of these values will be executed. Run all tests from eligible categories if not value specified", CommandOptionType.SingleOrNoValue);
            var excludedCategories = app.Option("-ec | --exclude-categories", "| seperated test categories to exclude for execution e.g. Category1|Category2.", CommandOptionType.SingleOrNoValue);
            var excludedTags = app.Option("-et | --exclude-tags", "| seperated tags values. Tests tagged with any of these values will not be executed.", CommandOptionType.SingleOrNoValue);

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.ColoredConsole()
            .WriteTo.RollingFile("logs\\Pixel-Automation-{Date}.txt")
            .CreateLogger();

            app.OnExecuteAsync(async c =>
            {
                if (project.HasValue() && version.HasValue())
                {
                    var testSelector = new TestSelector();
                    testSelector.WithCategories(includedCategories.Value()).WithExcludedCategories(excludedCategories.Value())
                        .WithTags(includedTags.Value()).WithExcludedCategories(excludedCategories.Value());

                    var kernel = new StandardKernel(new CommonModule(), new DevicesModule(), new WindowsModule());

                    var projectManager = kernel.Get<ProjectManager>();
                    projectManager.LoadProject(project.Value(), version.Value());
                    projectManager.LoadTestCases();
                    await projectManager.RunAll(testSelector);
                    return await Task.FromResult<int>(0);
                }
                else
                {
                    Log.Error("No arguments provided.");
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
