using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client;
using Spectre.Console.Cli;
using System.Threading.Tasks;
using Pixel.Automation.Core;
using System.Diagnostics;
using Dawn;
using Spectre.Console;

namespace Pixel.Automation.Test.Runner.Commands;

internal abstract class RunTestCommand<T> : AsyncCommand<T> where T : CommandSettings
{
    protected readonly IAnsiConsole console;
    protected readonly IApplicationDataManager applicationDataManager;
    protected readonly IProjectDataManager projectDataManager;
    protected readonly IPrefabDataManager prefabDataManager;
    protected readonly ProjectManager projectManager;

    public RunTestCommand(IAnsiConsole console, IApplicationDataManager applicationDataManager,
      IProjectDataManager projectDataManager, IPrefabDataManager prefabDataManager, ProjectManager projectManager)
    {
        this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull();
        this.console = Guard.Argument(console, nameof(console)).NotNull().Value;
        this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
        this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
    }

    protected virtual async Task DownloadDataAsync()
    {
        using (Telemetry.DefaultSource?.StartActivity("download-applications-and-controls", ActivityKind.Internal))
        {
            await applicationDataManager.DownloadApplicationsWithControlsAsync();
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
}

