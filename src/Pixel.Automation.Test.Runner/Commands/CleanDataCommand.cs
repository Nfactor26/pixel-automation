using Dawn;
using Pixel.Persistence.Services.Client;
using Spectre.Console.Cli;
using System.Threading;

namespace Pixel.Automation.Test.Runner.Commands;

/// <summary>
/// Clean data command deletes all the local application and automation data
/// </summary>
internal sealed class CleanDataCommand : Command<Settings>
{
    private readonly IApplicationDataManager applicationDataManager;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="applicationDataManager"></param>
    public CleanDataCommand(IApplicationDataManager applicationDataManager)
    {
        this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        applicationDataManager.CleanLocalData();
        return 0;
    }
}
