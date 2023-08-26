﻿using Dawn;
using Pixel.Automation.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using static Pixel.Automation.Test.Runner.Commands.ExecuteTestFromTemplateCommand;

namespace Pixel.Automation.Test.Runner.Commands;


/// <summary>
/// Execute test from template command is used to execute tests by specifying a template and optionally project version to use
/// </summary>
internal sealed class ExecuteTestFromTemplateCommand : AsyncCommand<ExecuteTestFromTemplateSettings>
{
    public sealed class ExecuteTestFromTemplateSettings : ExecuteTestSettings
    {
        [Description("Name of the template to use")]
        [CommandArgument(0, "<Name>")]
        public string TemplateName { get; init; }
       
        [Description("Only list the test cases without executing them")]
        [CommandOption("-l|--list")]
        public bool? List { get; set; }

    }

    private readonly IAnsiConsole console;
    private readonly TemplateManager templateManager;
    private readonly ProjectManager projectManager;

    public ExecuteTestFromTemplateCommand(IAnsiConsole console, TemplateManager templateManager, ProjectManager projectManager)
    {
        this.templateManager = Guard.Argument(templateManager, nameof(templateManager)).NotNull();
        this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull();
        this.console = Guard.Argument(console, nameof(console)).NotNull().Value;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ExecuteTestFromTemplateSettings settings)
    {
        using (Telemetry.DefaultSource?.StartActivity(nameof(ExecuteAsync), ActivityKind.Internal))
        {
            var sessionTemplate = await templateManager.GetByNameAsync(settings.TemplateName) ??
                throw new ArgumentException($"Template with name : {0} doesn't exist", settings.TemplateName);
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
