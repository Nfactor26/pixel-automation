﻿using Dawn;
using Pixel.Persistence.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using static Pixel.Automation.Test.Runner.Commands.ExecuteAdhocTestCommand;

namespace Pixel.Automation.Test.Runner.Commands;

/// <summary>
/// Execute adhoc test command is used to execute tests without using a template
/// </summary>
internal sealed class ExecuteAdhocTestCommand : AsyncCommand<AdhocTestSettings>
{
    public sealed class AdhocTestSettings : ExecuteTestSettings
    {
        [Description("Name of the automation project")]
        [CommandArgument(0, "<Project>")]
        public string Project { get; init; }

        [Description("Version of the automation project to use")]
        [CommandArgument(1, "[Version]")]
        public string Version { get; init; }

        [Description("Test case selector script")]
        [CommandArgument(2, "<Selector>")]
        public string Selector { get; init; }

        [Description("Name of the initialization script file")]
        [CommandArgument(3, "<InitScriptFile>")]
        public string InitializationScript { get; init; }

        [Description("Only list the test cases without executing them")]
        [CommandOption("-l|--list")]
        public bool? List { get; set; }
    }

    private readonly IAnsiConsole console;
    private readonly ProjectManager projectManager;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="console"></param>
    /// <param name="projectManager"></param>
    public ExecuteAdhocTestCommand(IAnsiConsole console, ProjectManager projectManager)
    {
        this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull();
        this.console = Guard.Argument(console, nameof(console)).NotNull().Value;
    }

    /// <inheritdoc/>    
    public override async Task<int> ExecuteAsync(CommandContext context, AdhocTestSettings settings)
    {
        Debugger.Launch();
        var sessionTemplate = new SessionTemplate()
        {
            Name = Guid.NewGuid().ToString(),
            ProjectName = settings.Project,            
            Selector = settings.Selector,
            InitializeScript =  settings.InitializationScript
        };       
    
        await projectManager.LoadProjectAsync(sessionTemplate, settings.Version);
        projectManager.LoadTestCases();
        if (settings.List.GetValueOrDefault())
        {
            await projectManager.ListAllAsync(sessionTemplate.Selector, console);
        }
        else
        {
            await projectManager.RunAllAsync(sessionTemplate.Selector);
        }
        return 0;
    }
}