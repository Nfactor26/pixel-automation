using Dawn;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner.Commands;

/// <summary>
/// Edit template command is used to edit an existing template
/// </summary>
internal sealed class EditTemplateCommand : AsyncCommand<TemplateSettings>
{
    private readonly TemplateManager templateManager;
    private readonly IAnsiConsole console;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="templateManager"></param>
    /// <param name="console"></param>
    public EditTemplateCommand(TemplateManager templateManager, IAnsiConsole console)
    {
        this.templateManager = Guard.Argument(templateManager, nameof(templateManager)).NotNull();
        this.console = Guard.Argument(console, nameof(console)).NotNull().Value;
    }

    /// <inheritdoc/>    
    public override async Task<int> ExecuteAsync(CommandContext context, TemplateSettings settings)
    {
        var templateName = console.Ask<string>("Enter name of [green]template[/] to edit");
        var templateToEdit = await templateManager.GetByNameAsync(templateName) ?? 
            throw new ArgumentException($"Template with name {templateName} could not be retrieved");
        await templateManager.UpdateAsync(templateToEdit);
        return 0;
    }
}
