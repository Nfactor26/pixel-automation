using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.Diagnostics;
using System.IO;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab;

public class NewPrefabViewModel : SmartScreen
{
    private readonly ILogger logger = Log.ForContext<NewPrefabViewModel>();

    private readonly ApplicationDescriptionViewModel application;  
    private readonly IPrefabDataManager prefabDataManager;  
    private List<PrefabProject> existingProjects = new List<PrefabProject>();

    public PrefabProject NewProject { get; }

    public string Name
    {
        get => this.NewProject.Name;
        set
        {
            value = value ?? string.Empty;
            this.NewProject.Name = value;
            ValidateProjectName(value);
            NotifyOfPropertyChange(() => Name);
            NotifyOfPropertyChange(() => CanCreateNewProject);
        }
    }

    public NewPrefabViewModel(ApplicationDescriptionViewModel application, IPrefabDataManager prefabDataManager)
    {
        this.DisplayName = "Create New Project";

        this.application = Guard.Argument(application, nameof(application)).NotNull().Value;        
        this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;       
        Version defaultVersion = new Version(1, 0, 0, 0);
        this.NewProject = new PrefabProject()
        {
            ApplicationId = application.ApplicationId
        };
        this.NewProject.AvailableVersions.Add(new VersionInfo(defaultVersion) { IsActive = true });

        this.existingProjects.AddRange(this.prefabDataManager.GetAllPrefabs(this.application.ApplicationId));

    }

    public async Task CreatePrefabProject()
    {
        using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreatePrefabProject), ActivityKind.Internal))
        {
            try
            {
                this.NewProject.Name = this.NewProject.Name.Trim();
                this.NewProject.Namespace = $"{Constants.NamespacePrefix}.{this.NewProject.Name.Replace(' ', '.')}";
                activity?.SetTag("ProjectName", this.NewProject.Name);
                activity?.SetTag("Namespace", this.NewProject.Namespace);              
                await this.prefabDataManager.AddPrefabToScreenAsync(this.NewProject, application.ScreenCollection.SelectedScreen.ScreenId);   
                logger.Information("Created new prefab project : '{0}' for application : '{1}'", this.Name, this.application.ApplicationName);
                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Warning("There was an error while trying to create new project : {0}", this.NewProject.Name);
                logger.Error(ex.Message, ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                await this.TryCloseAsync(false);
            }
        }
    }

    public bool CanCreateNewProject
    {
        get
        {
            return !this.HasErrors && !string.IsNullOrEmpty(this.Name) && !!IsNameAvailable(this.Name);
        }

    }

    public void ValidateProjectName(string projectName)
    {
        ClearErrors(nameof(Name));
        ValidateRequiredProperty(nameof(Name), projectName);
        ValidatePattern("^([A-Za-z]|[_ ]){4,}$", nameof(Name), projectName, "Name must contain only alphabets or ' ' or '_' and should be atleast 4 characters in length.");
        if (!IsNameAvailable(projectName))
        {
            this.AddOrAppendErrors(nameof(Name), "An application already exists with this name");
        }

    }

    private bool IsNameAvailable(string projectName)
    {
        return !this.existingProjects.Any(a => a.Name.Equals(projectName));
    }

    public async void Cancel()
    {
        await this.TryCloseAsync(false);
    }

}
