using Dawn;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    public class TemplateManager
    {
        private readonly ITemplateClient templateClient;
        private readonly ProjectManager projectManager;
        private readonly IApplicationDataManager applicationDataManager;

        public TemplateManager(ProjectManager projectManager, IApplicationDataManager applicationDataManager, ITemplateClient templateClient)
        {
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
            this.templateClient = Guard.Argument(templateClient).NotNull().Value ;
            this.projectManager = Guard.Argument(projectManager).NotNull();
        }

        public async Task ListAllAsync()
        {
            var templates = await this.templateClient.GetAllAsync();
            Console.WriteLine("Name    | ProjectName   | ProjectVersion ");
            Console.WriteLine("----------------------------------------");
            foreach (var template in templates)
            {
                Console.WriteLine($"{template.Name} | {template.ProjectName} | {template.ProjectVersion} ");
            }
        }

        public async Task<SessionTemplate> GetByNameAsync(string templateName)
        {
            var result = await templateClient.GetByNameAsync(templateName);
            return result;
        }

        public async Task CreateNewAsync()
        {          
            Console.WriteLine("Enter below details for the template :");
            string name = TryGet(nameof(SessionTemplate.Name), (x) => !string.IsNullOrEmpty(x));
            string projectName = TryGet(nameof(SessionTemplate.ProjectName), (x) => !string.IsNullOrEmpty(x));    
            string projectVersion = TryGet(nameof(SessionTemplate.ProjectVersion), (x) => Version.TryParse(x, out Version version));
            string selector = TryGet(nameof(SessionTemplate.Selector), (x) => !string.IsNullOrEmpty(x));
            Console.Write($"{nameof(SessionTemplate.InitializeScript)} (optional): ");
            string initializeScript = Console.ReadLine();          
            SessionTemplate template = new SessionTemplate()
            {
                Name = name,
                ProjectName = projectName,
                ProjectVersion = projectVersion,
                Selector = selector,
                InitializeScript = initializeScript
            };
            var availableProjects = this.applicationDataManager.GetAllProjects();
            var targetProject = availableProjects.FirstOrDefault(a => a.Name.Equals(projectName)) ?? throw new ArgumentException($"Project with name :" +
                $" {projectName} doesn't exist");
            template.ProjectId = targetProject.ProjectId;
            projectManager.LoadTestCases();
            await projectManager.ListAllAsync(template.Selector);

            Console.WriteLine($"Do you want to proceed and create  template : {name} ? Enter 'Y' to continue, 'N' to cancel.");
            var confirmation = Console.ReadKey();
            Console.WriteLine();
            switch (confirmation.Key)
            {
                case ConsoleKey.Y:
                    await this.templateClient.CreateAsync(template);
                    Console.WriteLine("Template created successfully.");
                    break;
                case ConsoleKey.N:
                    Console.WriteLine("Operation was cancelled.");
                    break;            
            }
        }

        public async Task UpdateAsync(SessionTemplate template)
        {
            Console.WriteLine("-------------Update Template------------ ");
            Console.WriteLine($"Template Id is : {template.Id}");
            Console.WriteLine($"Template Name is : {template.Name}");
            Console.WriteLine($"Project Id is : {template.ProjectId}");
            Console.WriteLine($"Project Name is : {template.ProjectName}");
            Console.WriteLine("----Review below fields for update------");
            if (ShouldUpdate(nameof(SessionTemplate.ProjectVersion), template.ProjectVersion))
            {
                template.ProjectVersion = TryGet(nameof(SessionTemplate.ProjectVersion), (x) => Version.TryParse(x, out Version version));
            }
            if (ShouldUpdate(nameof(SessionTemplate.Selector), template.Selector))
            {
                template.Selector = TryGet(nameof(SessionTemplate.Selector), (x) => !string.IsNullOrEmpty(x));
            }
            if (ShouldUpdate(nameof(SessionTemplate.InitializeScript), template.InitializeScript))
            {
                Console.Write($"{nameof(SessionTemplate.InitializeScript)} (optional): ");
                template.InitializeScript = Console.ReadLine();
            }                    
            await projectManager.LoadProjectAsync(template);
            projectManager.LoadTestCases();
            await projectManager.ListAllAsync(template.Selector);

            Console.WriteLine($"Do you want to proceed and create  template : {template.Name} ? Enter 'Y' to continue, 'N' to cancel, 'M' to modify.");
            var confirmation = Console.ReadKey();
            Console.WriteLine();
            switch (confirmation.Key)
            {
                case ConsoleKey.Y:
                    await this.templateClient.UpdateAsync(template);
                    Console.WriteLine("Template updated successfully.");
                    break;
                case ConsoleKey.N:
                    Console.WriteLine("Operation was cancelled.");
                    break;              
            }
        }

        private bool ShouldUpdate(string fieldName, string existingValue)
        {
            Console.WriteLine($"{fieldName} : {existingValue}");
            Console.WriteLine($"Press 'U' to update {fieldName}. Any other key to skip");
            var confirmation = Console.ReadKey();
            Console.WriteLine();
            switch (confirmation.Key)
            {
                case ConsoleKey.U:
                    return true;
                default:
                    break;
            }
            return false;
        }

        private string TryGet(string fieldName, Func<string, bool> validator)
        {
            Console.Write($"{fieldName} : ");
            string value = Console.ReadLine();
            while (string.IsNullOrEmpty(value) || !validator(value))
            {
                Console.WriteLine("Empty or invalid format. Please enter value again.");
                Console.Write($"{fieldName} : ");
                value = Console.ReadLine();               
            }
            return value;
        }
    }
}
