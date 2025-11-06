using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Pixel.Automation.Test.Runner.Commands.ExecuteTestWithImportCommand;

namespace Pixel.Automation.Test.Runner.Commands;

internal class ExecuteTestWithImportCommand : RunTestCommand<ExecuteTestWithImportSettings>
{
    private readonly ApplicationSettings appSettings;

    public sealed class ExecuteTestWithImportSettings : ExecuteTestSettings
    {
        [Description("Zip file containing the automation process and dependencies")]
        [CommandArgument(0, "<ImportFile>")]
        public string ImportFile { get; init; }

        [Description("Test case selector script")]
        [CommandArgument(2, "<Selector>")]
        public string Selector { get; init; }

        [Description("Name of the initializer function in initialize environment script file")]
        [CommandArgument(3, "[InitFunction]")]
        public string InitFunction { get; init; }

        [Description("Only list the test cases without executing them")]
        [CommandOption("-l|--list")]
        public bool? List { get; set; }
    }

    public ExecuteTestWithImportCommand(IAnsiConsole console, IApplicationDataManager applicationDataManager,
      IProjectDataManager projectDataManager, IPrefabDataManager prefabDataManager, ProjectManager projectManager,
      ApplicationSettings appSettings) : base(console, applicationDataManager, projectDataManager, prefabDataManager, projectManager)
    {      
        this.appSettings = Guard.Argument(appSettings, nameof(appSettings)).NotNull().Value;
        appSettings.IsOfflineMode = true;
        TraceManager.IsEnabled = false;
    }

    public async override Task<int> ExecuteAsync(CommandContext context, ExecuteTestWithImportSettings settings, CancellationToken cancellationToken)
    {
        using (Telemetry.DefaultSource?.StartActivity(nameof(ExecuteAsync), ActivityKind.Internal))
        {
            if(!File.Exists(settings.ImportFile))
            {
                throw new FileNotFoundException($"File : '{settings.ImportFile}' doesn't exist");
            }
            if(Directory.Exists(appSettings.ApplicationDirectory))
            {
                Directory.Delete(appSettings.ApplicationDirectory, true );
            }
            if(Directory.Exists(appSettings.AutomationDirectory))
            {
                Directory.Delete(appSettings.AutomationDirectory, true );
            }
            ZipFile.ExtractToDirectory(settings.ImportFile, Environment.CurrentDirectory);

            var automationProject = projectDataManager.GetAllProjects().Single();
            VersionInfo projectVersionToUse = null;
            foreach(var version in automationProject.AvailableVersions)
            {
                if(Directory.Exists(Path.Combine(appSettings.AutomationDirectory, automationProject.ProjectId, version.ToString())))
                {
                    projectVersionToUse = version;
                    break;
                }
            }
            if(projectVersionToUse == null)
            {
                throw new Exception($"Project : '{automationProject.Name}' doesn't have any versions available to use");
            }
          
            var sessionTemplate = new SessionTemplate()
            {
                Name = Guid.NewGuid().ToString(),
                ProjectName = automationProject.Name,
                ProjectId = automationProject.ProjectId,
                Selector = settings.Selector,
                InitFunction = settings.InitFunction
            };

            await projectManager.LoadProjectAsync(sessionTemplate, sessionTemplate.TargetVersion);
            await projectManager.LoadTestCasesAsync();
            if (settings.List.GetValueOrDefault())
            {
                await projectManager.ListAllAsync(sessionTemplate.Selector, console);
            }
            else
            {
                await projectManager.RunAllAsync(sessionTemplate.Selector, console);
            }
            return 0;
        }
    }

}
