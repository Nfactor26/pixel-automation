using Dawn;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    public class TemplateManager
    {
        private readonly ITemplateClient templateClient;       
        private readonly IProjectDataManager projectDataManager;
        private readonly IAnsiConsole ansiConsole;

        public TemplateManager(IProjectDataManager projectDataManager,
            ITemplateClient templateClient, IAnsiConsole ansiConsole)
        {
            this.projectDataManager = Guard.Argument(projectDataManager).NotNull().Value;
            this.templateClient = Guard.Argument(templateClient).NotNull().Value ;          
            this.ansiConsole = Guard.Argument(ansiConsole).NotNull().Value;
        }

        public async Task ListAllAsync()
        {
            var templates = await this.templateClient.GetAllAsync();

            var table = new Table();
            table.AddColumn(new TableColumn("[blue]Template Name[/]"));
            table.AddColumn(new TableColumn("[blue]Project Name[/]"));
            table.AddColumn(new TableColumn("[blue]Initialize Function[/]"));
            table.AddColumn(new TableColumn("[blue]Test Selector[/]"));
            foreach (var template in templates)
            {
                table.AddRow(template.Name, template.ProjectName, template.InitFunction, template.Selector);               
            }
            ansiConsole.Write(table);
        }

        public async Task<SessionTemplate> GetByNameAsync(string templateName)
        {
            var result = await templateClient.GetByNameAsync(templateName);
            return result;
        }

        public async Task CreateNewAsync()
        {          
            ansiConsole.WriteLine("Enter below details for the template :");
            string name = ansiConsole.Prompt(new TextPrompt<string>("[green]Template Name[/] :")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Tempalte Name is required.[/]")
                .Validate(n =>
                {
                    if(string.IsNullOrEmpty(n) || string.IsNullOrWhiteSpace(n))
                    {
                        return ValidationResult.Error("[red]Template Name is required.[/]");
                    }
                    return ValidationResult.Success();
                }));           
            string projectName = ansiConsole.Prompt(new TextPrompt<string>("[green]Project Name[/] :")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Project Name is required.[/]")
                .Validate(n =>
                {
                    if (string.IsNullOrEmpty(n) || string.IsNullOrWhiteSpace(n))
                    {
                        return ValidationResult.Error("[red]Project Name is required.[/]");
                    }
                    return ValidationResult.Success();
                }));          
            string selector = ansiConsole.Prompt(new TextPrompt<string>("[green]Test Selector[/] :")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Test Selector is required.[/]")
                .Validate(n =>
                {
                    if (string.IsNullOrEmpty(n) || string.IsNullOrWhiteSpace(n))
                    {
                        return ValidationResult.Error("[red]Test Selector is required.[/]");
                    }
                    return ValidationResult.Success();
                }));           
            string initializeScript = ansiConsole.Prompt(new TextPrompt<string>("[grey][[Optional]][/] [green]Initialize Function[/] :").AllowEmpty());
            SessionTemplate template = new SessionTemplate()
            {
                Name = name,
                ProjectName = projectName,                
                Selector = selector,
                InitFunction = initializeScript
            };
            var availableProjects = this.projectDataManager.GetAllProjects();
            var targetProject = availableProjects.FirstOrDefault(a => a.Name.Equals(projectName)) ?? throw new ArgumentException($"Project with name :" +
                $" {projectName} doesn't exist");
            template.ProjectId = targetProject.ProjectId;
           
            if(AnsiConsole.Confirm($"Do you want to proceed and create  template : {name} ?"))
            {
                await this.templateClient.CreateAsync(template);
                ansiConsole.WriteLine("Template created successfully.");                
                await ListAllAsync();
                return;
            }
            ansiConsole.WriteLine("Operation was cancelled.");
        }

        public async Task UpdateAsync(SessionTemplate template)
        {
            var table = new Table();         
            table.AddColumn(new TableColumn("Template Name"));           
            table.AddColumn(new TableColumn("Project Name"));
            table.AddColumn(new TableColumn("Test Selector"));
            table.AddColumn(new TableColumn("Initialize Function"));
            table.AddRow(template.Name, template.ProjectName, template.Selector, template.InitFunction);
           
            ansiConsole.WriteLine("[blue]Enter below details to update template :[/]");

            string name = ansiConsole.Prompt(new TextPrompt<string>("[grey][[Optional]][/] [green]Template Name[/] ").AllowEmpty());
            if(!string.IsNullOrEmpty(name) || !string.IsNullOrWhiteSpace(name))
            {
                template.Name = name;
            }
            string selector = ansiConsole.Prompt(new TextPrompt<string>("[grey][[Optional]][/] [green]Test Selector[/] ").AllowEmpty());
            if (!string.IsNullOrEmpty(selector) || !string.IsNullOrWhiteSpace(selector))
            {
                template.Selector = selector;
            }
            string initializeScript = ansiConsole.Prompt(new TextPrompt<string>("[grey][[Optional]][/] [green]Initialize Function[/]").AllowEmpty());
            if (!string.IsNullOrEmpty(initializeScript) || !string.IsNullOrWhiteSpace(initializeScript))
            {
                template.InitFunction = initializeScript;
            }
            
            if (ansiConsole.Confirm($"Do you want to proceed and update  template : {template.Name} ?"))
            {
                await this.templateClient.UpdateAsync(template);
                ansiConsole.WriteLine("Template updated successfully.");
                await ListAllAsync();
                return;
            }
            ansiConsole.WriteLine("Operation was cancelled.");
        }
    }
}
