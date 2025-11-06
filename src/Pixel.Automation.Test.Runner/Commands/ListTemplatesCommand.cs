using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner.Commands;

/// <summary>
/// List templates command is used to list all available templates
/// </summary>
internal sealed class ListTemplatesCommand : AsyncCommand<TemplateSettings>
{
    private readonly TemplateManager templateManager;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="templateManager"></param>
    public ListTemplatesCommand(TemplateManager templateManager)
    {
        this.templateManager = templateManager;
    }

    /// <inheritdoc/>    
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateSettings settings, CancellationToken cancellationToken)
    {
        await templateManager.ListAllAsync();
        return 0;
    }
}
