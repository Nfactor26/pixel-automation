using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class ProjectDataManager : IProjectDataManager
{
    private readonly ILogger logger = Log.ForContext<ProjectDataManager>();
    private readonly IAutomationsRepositoryClient projectsClient;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;
    private readonly IProjectFilesRepositoryClient filesClient;  
    private readonly IApplicationFileSystem applicationFileSystem;    
    private readonly ISerializer serializer;
    private readonly ApplicationSettings applicationSettings;
  
    bool IsOnlineMode
    {
        get => !this.applicationSettings.IsOfflineMode;
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="projectsClient"></param>
    /// <param name="referencesRepositoryClient"></param>
    /// <param name="filesClient"></param>
    /// <param name="fixturesClient"></param>
    /// <param name="testsClient"></param>
    /// <param name="applicationFileSystem"></param>
    /// <param name="projectFileSystem"></param>
    /// <param name="applicationSettings"></param>
    /// <param name="testDataRepositoryClient"></param>
    public ProjectDataManager(ISerializer serializer, IAutomationsRepositoryClient projectsClient, IReferencesRepositoryClient referencesRepositoryClient,
        IProjectFilesRepositoryClient filesClient, IApplicationFileSystem applicationFileSystem, 
        ApplicationSettings applicationSettings)
    {
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.projectsClient = Guard.Argument(projectsClient, nameof(projectsClient)).NotNull().Value;
        this.referencesRepositoryClient = Guard.Argument(referencesRepositoryClient, nameof(referencesRepositoryClient)).NotNull().Value;
        this.filesClient = Guard.Argument(filesClient, nameof(filesClient)).NotNull().Value;     
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;      
    }

    /// <inheritdoc/>  
    public async Task DownloadProjectsAsync()
    {
        if (IsOnlineMode)
        {
            var projects = await this.projectsClient.GetAllAsync();
            foreach (var project in projects)
            {
                var projectDirectory = this.applicationFileSystem.GetAutomationProjectDirectory(project);
                if (!Directory.Exists(projectDirectory))
                {
                    Directory.CreateDirectory(projectDirectory);
                }
                var projectFile = this.applicationFileSystem.GetAutomationProjectFile(project);
                if (File.Exists(projectFile))
                {
                    File.Delete(projectFile);
                }
                serializer.Serialize(projectFile, project);
            }
            logger.Information("Downloaded details for {0} automation project.", projects.Count());
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadProjectDataFilesAsync(AutomationProject automationProject, VersionInfo projectVersion)
    {
        Guard.Argument(automationProject, nameof(automationProject)).NotNull();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();

        var projectDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion));
        if (!Directory.Exists(projectDirectory))
        {
            Directory.CreateDirectory(projectDirectory);
        }

        if (IsOnlineMode)
        {
            //Download project references data
            var projectReferences = await this.referencesRepositoryClient.GetProjectReferencesAsync(automationProject.ProjectId, projectVersion.ToString());
            if (projectReferences != null)
            {
                var prefabReferencesFile = Path.Combine(projectDirectory, Constants.ReferencesFileName);
                this.serializer.Serialize(prefabReferencesFile, projectReferences);
            }
            await DownloadFilesWithTagsAsync(automationProject, projectVersion, new[] { automationProject.ProjectId });
            logger.Information("Downloaded data files for version : '{0}' of automation project : '{1}'", projectVersion, automationProject.Name);
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadProjectDataFileByNameAsync(AutomationProject automationProject, VersionInfo projectVersion, string fileName)
    {
        Guard.Argument(automationProject, nameof(automationProject)).NotNull();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();      

        if (IsOnlineMode)
        {
            var file = await this.filesClient.DownProjectDataFile(automationProject.ProjectId, projectVersion.ToString(), fileName);
            var projectDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion));
            using (MemoryStream ms = new MemoryStream(file.Bytes))
            {
                using (FileStream fs = new FileStream(Path.Combine(projectDirectory, file.FilePath), FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }
            logger.Information("File : '{0}' was downloaded for version : '{1}' of automation project : '{2}'.", file.FilePath, projectVersion, automationProject.Name);
        }
    }

    /// <inheritdoc/> 
    public async Task DownloadDataModelFilesAsync(AutomationProject automationProject, VersionInfo projectVersion)
    {
        Guard.Argument(automationProject, nameof(automationProject)).NotNull();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
       
        if (IsOnlineMode)
        {
            var dataModelsDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion), Constants.DataModelDirectory);
            if (Directory.Exists(dataModelsDirectory))
            {
                Directory.Delete(dataModelsDirectory, true);
            }
            Directory.CreateDirectory(dataModelsDirectory);
            var zippedContent = await this.filesClient.DownloadProjectDataFilesOfType(automationProject.ProjectId, projectVersion.ToString(), "cs");
            if (zippedContent.Length > 0)
            {
                string versionDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }
            logger.Information("Downloded data model files for version : '{0}' of automation project : '{1}'.", projectVersion, automationProject.Name);
        }
    }

    /// <summary>
    /// Download all the files having specific tags belonging to the version of the AutomationProject being managed
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    async Task DownloadFilesWithTagsAsync(AutomationProject automationProject, VersionInfo projectVersion, string[] tags)
    {
        if (IsOnlineMode)
        {
            //Delete the data models and scripts directory. If any of the data model file or scipe file was deleted by other user, we don't want that leftover
            string projectWorkingDirectory = applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
            if(Directory.Exists(Path.Combine(projectWorkingDirectory, Constants.DataModelDirectory)))
            {
                Directory.Delete(Path.Combine(projectWorkingDirectory, Constants.DataModelDirectory), true);
            }
            if(Directory.Exists(Path.Combine(projectWorkingDirectory, Constants.ScriptsDirectory)))
            {
                Directory.Delete(Path.Combine(projectWorkingDirectory, Constants.ScriptsDirectory), true);
            }

            //create the data models and scripts directory
            Directory.CreateDirectory(Path.Combine(projectWorkingDirectory, Constants.DataModelDirectory));
            Directory.CreateDirectory(Path.Combine(projectWorkingDirectory, Constants.ScriptsDirectory));
          
            //Download data model files and scripts
            var zippedContent = await this.filesClient.DownloadProjectDataFilesWithTags(automationProject.ProjectId, projectVersion.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                string versionDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }          
        }
    }
   
    /// <inheritdoc/>  
    public IEnumerable<AutomationProject> GetAllProjects()
    {
        foreach (var item in Directory.EnumerateDirectories(this.applicationSettings.AutomationDirectory))
        {
            string automationProjectFile = Path.Combine(item, $"{Path.GetFileName(item)}.atm");
            if (File.Exists(automationProjectFile))
            {
                var automationProject = serializer.Deserialize<AutomationProject>(automationProjectFile, null);
                yield return automationProject;
                continue;
            }
            logger.Warning("Project file {file} doesn't exist.", automationProjectFile);
        }
        yield break;
    }

    /// <inheritdoc/>  
    public async Task AddProjectAsync(AutomationProject automationProject)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.AddProjectAsync(automationProject);
            logger.Information("Automation project : '{1}' was added.", automationProject.Name);
        }
    }

    /// <inheritdoc/>  
    public async Task AddProjectVersionAsync(AutomationProject automationProject, VersionInfo newVersion, VersionInfo versionToClone)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.AddProjectVersionAsync(automationProject.ProjectId, newVersion, versionToClone);
            await DownloadProjectDataFilesAsync(automationProject, newVersion);
        }   
        else
        {
            var cloneFromVersionDirectory = Path.Combine(applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, versionToClone));
            var newVersionDirectory = Path.Combine(applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, newVersion));           
            Directory.CreateDirectory(newVersionDirectory);

            ////copy contents from previous version directory to new version directory
            CopyAll(new DirectoryInfo(cloneFromVersionDirectory), new DirectoryInfo(newVersionDirectory));

            void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
        }

        automationProject.AvailableVersions.Add(newVersion);
        string automationProjectsFile =this.applicationFileSystem.GetAutomationProjectFile(automationProject);
        serializer.Serialize(automationProjectsFile, automationProject);
        logger.Information("Added version : '{0}' for automation project : '{1}' cloned from version : '{2}'.", newVersion, automationProject.Name, versionToClone);

    }

    /// <inheritdoc/>  
    public async Task UpdateProjectVersionAsync(AutomationProject automationProject, VersionInfo projectVersion)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.UpdateProjectVersionAsync(automationProject.ProjectId, projectVersion);
        }
        string automationProjectsFile = this.applicationFileSystem.GetAutomationProjectFile(automationProject);
        serializer.Serialize(automationProjectsFile, automationProject);
        logger.Information("Updated version : '{0}' of automation project : '{1}'.",  projectVersion, automationProject.Name);
    }

    /// <inheritdoc/>  
    public async Task AddOrUpdateDataFileAsync(AutomationProject automationProject, VersionInfo projectVersion, string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            var projectsDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, automationProject.ProjectId, projectVersion.ToString());
            await this.filesClient.AddProjectDataFile(new ProjectDataFile()
            {
                ProjectId = automationProject.ProjectId,
                ProjectVersion = projectVersion.ToString(),
                Tag = tag,
                FileName = Path.GetFileName(filePath),
                FilePath = Path.GetRelativePath(projectsDirectory, filePath),
            }, filePath);
            logger.Information("Data file : '{0}' for version : '{1}' of automation project : '{2}' was added  or updated.", Path.GetFileName(filePath), projectVersion, automationProject.Name);
        }
    }

    /// <inheritdoc/>  
    public async Task DeleteDataFileAsync(AutomationProject automationProject, VersionInfo projectVersion, string fileToDelete)
    {
        if (IsOnlineMode)
        {            
            await this.filesClient.DeleteProjectDataFile(automationProject.ProjectId, projectVersion.ToString(), Path.GetFileName(fileToDelete));
        }
        if (File.Exists(fileToDelete))
        {
            File.Delete(fileToDelete);
        }
        logger.Information("Deleted data file : '{0}' for version : '{1}' of automation project : '{2}'.", projectVersion, automationProject.Name, fileToDelete);
    }

    /// <inheritdoc/>  
    public async Task SaveProjectDataAsync(AutomationProject automationProject, VersionInfo projectVersion)
    {
        if (IsOnlineMode)
        {
            string projectDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion));

            string processsFile = Path.Combine(projectDirectory, Constants.AutomationProcessFileName);
            await AddOrUpdateDataFileAsync(automationProject, projectVersion, processsFile, automationProject.ProjectId);

            //save the datamodel assembly file
            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ReferencesDirectory), "*.*"))
            {
                await AddOrUpdateDataFileAsync(automationProject, projectVersion, file, automationProject.ProjectId);
            }
         
            //Save script files other than the initialization script
            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ScriptsDirectory), "*.csx"))
            {
                //Initialization script file will be saved immediately when the file is edited in script editor.Hence, we skip it as there could have been
                //additional edits by other users between this file was saved last time and now.
                if(Path.GetFileName(file).Equals(Constants.InitializeEnvironmentScript))
                {
                    continue;
                }
                await AddOrUpdateDataFileAsync(automationProject, projectVersion, file, automationProject.ProjectId);
            }
            logger.Information("Saved project data for version : '{0}' of automation project : '{1}'.", projectVersion, automationProject.Name);
        }
    }
}

