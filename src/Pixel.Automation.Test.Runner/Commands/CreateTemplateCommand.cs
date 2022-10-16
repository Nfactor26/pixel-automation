using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner.Commands;

/// <summary>
/// Create template command is used to create a new test session template
/// </summary>
internal sealed class CreateTemplateCommand : AsyncCommand<TemplateSettings>
{
    private readonly TemplateManager templateManager;
 
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="templateManager"></param>
    public CreateTemplateCommand(TemplateManager templateManager)
    {         
        this.templateManager = templateManager;        
    }

    /// <inheritdoc/>    
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateSettings settings)
    {
        await templateManager.CreateNewAsync();
        return 0;
    }
}
