using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class PrefabDataManager : IPrefabDataManager
{
    private readonly ILogger logger = Log.ForContext<PrefabDataManager>();
    private readonly ApplicationSettings applicationSettings;
    private readonly ISerializer serializer;
    private readonly IApplicationFileSystem applicationFileSystem;
    private readonly IPrefabsRepositoryClient prefabsRepositoryClient;
    private readonly IPrefabFilesRepositoryClient filesClient;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;
    private readonly Dictionary<string, List<PrefabProject>> prefabsCache = new();

    bool IsOnlineMode => !this.applicationSettings.IsOfflineMode;
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="applicationSettings"></param>
    /// <param name="serializer"></param>
    /// <param name="applicationFileSystem"></param>
    /// <param name="prefabsRepositoryClient"></param>
    /// <param name="filesRepositoryClient"></param>
    /// <param name="referencesRepositoryClient"></param>
    public PrefabDataManager(ApplicationSettings applicationSettings, ISerializer serializer, IApplicationFileSystem applicationFileSystem, 
        IPrefabsRepositoryClient prefabsRepositoryClient, IPrefabFilesRepositoryClient filesRepositoryClient, IReferencesRepositoryClient referencesRepositoryClient)
    {
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.prefabsRepositoryClient = Guard.Argument(prefabsRepositoryClient).NotNull().Value;
        this.filesClient = Guard.Argument(filesRepositoryClient).NotNull().Value;
        this.referencesRepositoryClient = Guard.Argument(referencesRepositoryClient).NotNull().Value;
        this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;
    }

    ///<inheritdoc/>
    public IEnumerable<PrefabProject> GetAllPrefabs(string applicationId)
    {
        if (!prefabsCache.ContainsKey(applicationId))
        {
            prefabsCache.Add(applicationId, LoadPrefabsFromLocalStorage(applicationId));
        }
        return prefabsCache[applicationId];
    }

    ///<inheritdoc/>
    public IEnumerable<PrefabProject> GetPrefabsForScreen(ApplicationDescription applicationDescription, string screenName)
    {
        if (applicationDescription.AvailablePrefabs.ContainsKey(screenName))
        {
            var prefabs = GetAllPrefabs(applicationDescription.ApplicationId);
            foreach (var prefabId in applicationDescription.AvailablePrefabs[screenName])
            {
                var prefab = prefabs.FirstOrDefault(p => p.PrefabId.Equals(prefabId));
                if (prefab != null)
                {
                    yield return prefab;
                }
            }
        }
        yield break;
    }

    ///<inheritdoc/>
    public async Task DownloadPrefabDataAsync(string applicationId, string prefabId, string prefabVersion)
    {
        Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
        Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        Guard.Argument(prefabVersion, nameof(prefabVersion)).NotNull().NotEmpty();       
        var prefabProject = LoadPrefabFromLocalStorage(applicationId, prefabId);
        var versionToDownload = prefabProject.AvailableVersions.FirstOrDefault(a => a.Version.ToString().Equals(prefabVersion))
            ?? throw new ArgumentException($"Version {prefabVersion} doesn't exist for prefab {prefabProject.PrefabName}");
        await DownloadPrefabDataAsync(prefabProject, versionToDownload);
    }

    ///<inheritdoc/>
    public async Task DownloadPrefabDataAsync(PrefabProject prefabProject, PrefabVersion prefabVersion)
    {
        Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
        Guard.Argument(prefabVersion, nameof(prefabVersion)).NotNull();

        var prefabsDirectory = Path.Combine(this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject), prefabVersion.ToString());
        if (!Directory.Exists(prefabsDirectory))
        {
            Directory.CreateDirectory(prefabsDirectory);
        }    
      
        if(IsOnlineMode)
        {           
            if(HasMostRecentDataAlreadyAvailable())
            {
                logger.Information("Skip download of version {0} of prefab project {1}. Data already available.", prefabVersion, prefabProject.PrefabName);
                return;
            }
            
            //Download prefab references data
            var prefabReferences = await this.referencesRepositoryClient.GetProjectReferencesAsync(prefabProject.PrefabId, prefabVersion.ToString());
            if (prefabReferences != null)
            {
                var prefabReferencesFile = Path.Combine(this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject), prefabVersion.ToString(), Constants.ReferencesFileName);
                this.serializer.Serialize(prefabReferencesFile, prefabReferences);
            }
            await DownloadFilesWithTagsAsync(prefabProject, prefabVersion, new[] { prefabProject.PrefabId });
            logger.Information("Download completed for version {0} of prefab project {1}", prefabVersion, prefabProject.PrefabName);
        }

        bool HasMostRecentDataAlreadyAvailable()
        {
            string lastUpdatedDataFile = Path.Combine(prefabsDirectory, Constants.LastUpdatedFileName);
            DateTime lastUpdated = DateTime.MinValue.ToUniversalTime();
            if (File.Exists(lastUpdatedDataFile))
            {
                if (!DateTime.TryParse(File.ReadAllText(lastUpdatedDataFile), out lastUpdated))
                {
                    throw new Exception($"Failed to read last updated data from file : {lastUpdatedDataFile}");
                }
                File.Delete(lastUpdatedDataFile);
            }
            File.WriteAllText(lastUpdatedDataFile, DateTime.Now.ToUniversalTime().ToString("O"));
            return (prefabVersion.IsPublished && prefabVersion.PublishedOn < lastUpdated);           
        }
    }

    ///<inheritdoc/>
    public async Task DownloadPrefabsAsync()
    {
        if (IsOnlineMode)
        {
            var prefabs = await this.prefabsRepositoryClient.GetAllAsync();
            foreach (var prefab in prefabs)
            {
                var prefabsDirectory = this.applicationFileSystem.GetPrefabProjectDirectory(prefab);
                if (!Directory.Exists(prefabsDirectory))
                {
                    Directory.CreateDirectory(prefabsDirectory);
                }
                var prefabFile = this.applicationFileSystem.GetPrefabProjectFile(prefab);
                if (File.Exists(prefabFile))
                {
                    File.Delete(prefabFile);
                }
                serializer.Serialize(prefabFile, prefab);
            }
            logger.Information("Download of Prefabs completed");
        }
    }

    ///<inheritdoc/>
    public async Task AddPrefabAsync(PrefabProject prefabProject)
    {
        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.AddPrefabAsync(prefabProject);

            //Save the automation process file, data model files (*.cs) and script files (*.csx).
            //The project references file will be saved by the reference manager once initialized.
            string projectDirectory = Path.Combine(this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject), prefabProject.LatestActiveVersion.ToString());
            string processsFile = Path.Combine(projectDirectory, Constants.PrefabProcessFileName);
            await AddOrUpdateDataFileAsync(prefabProject, prefabProject.LatestActiveVersion, processsFile, prefabProject.PrefabId);

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.DataModelDirectory), "*.cs"))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabProject.LatestActiveVersion, file, prefabProject.PrefabId);
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ScriptsDirectory), "*.csx"))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabProject.LatestActiveVersion, file, prefabProject.PrefabId);
            }

            logger.Information("Prefab project {0} was added.", prefabProject.PrefabName);
        }

        ////when a new prefab is created , add it to the prefabs cache
        if (prefabsCache.ContainsKey(prefabProject.ApplicationId))
        {
            var prefabsForApplicationId = prefabsCache[prefabProject.ApplicationId];
            if (!prefabsForApplicationId.Any(p => p.PrefabId.Equals(prefabProject.PrefabId)))
            {
                prefabsForApplicationId.Add(prefabProject);
            }
        }
        else
        {
            prefabsCache.Add(prefabProject.ApplicationId, new List<PrefabProject>() { prefabProject });
        }
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateDataFileAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            var prefabsDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefabProject.ApplicationId, Constants.PrefabsDirectory
                , prefabProject.PrefabId, prefabVersion.ToString());
            await this.filesClient.AddProjectDataFile(new Core.Models.ProjectDataFile()
            {
                ProjectId = prefabProject.PrefabId,
                ProjectVersion = prefabVersion.ToString(),
                FileName = Path.GetFileName(filePath),
                FilePath = Path.GetRelativePath(prefabsDirectory, filePath),
                Tag = tag,
            }, filePath);
            logger.Information("File {0} belonging to version {1} of prefab project {2} was added/updated.", Path.GetFileName(filePath), prefabVersion.ToString(), prefabProject.PrefabName);
        }
    }

    /// <inheritdoc/>  
    public async Task DeleteDataFileAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string fileToDelete)
    {
        if (IsOnlineMode)
        {
            await this.filesClient.DeleteProjectDataFile(prefabProject.PrefabId, prefabVersion.ToString(), Path.GetFileName(fileToDelete));
        }
        if (File.Exists(fileToDelete))
        {
            File.Delete(fileToDelete);
        }
        logger.Information("File {0} belonging to version {1} of prefab project {2} was deleted.", Path.GetFileName(fileToDelete), prefabVersion.ToString(), prefabProject.PrefabName);
    }

    ///<inheritdoc/>
    public async Task AddPrefabVersionAsync(PrefabProject prefabProject, PrefabVersion newVersion, PrefabVersion cloneFrom)
    {
        if (IsOnlineMode)
        {           
            await this.prefabsRepositoryClient.AddPrefabVersionAsync(prefabProject.PrefabId, newVersion, cloneFrom);
            await DownloadPrefabDataAsync(prefabProject, newVersion);
        }
        else
        {
            var cloneFromVersionDirectory = Path.Combine(applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, cloneFrom.ToString()));
            var newVersionDirectory = Path.Combine(applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, newVersion.ToString()));
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

        prefabProject.AvailableVersions.Add(newVersion);
        string prefabDescriptionFile = this.applicationFileSystem.GetPrefabProjectFile(prefabProject);
        serializer.Serialize<PrefabProject>(prefabDescriptionFile, prefabProject);
        logger.Information("Added new version {0} of prefab {1} from version {2}.", newVersion.ToString(), prefabProject.PrefabName, cloneFrom.ToString());
    }

    ///<inheritdoc/>
    public async Task UpdatePrefabVersionAsync(PrefabProject prefabProject, PrefabVersion prefabVersion)
    {
        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.UpdatePrefabVersionAsync(prefabProject.PrefabId, prefabVersion);
        }
        string prefabDescriptionFile = this.applicationFileSystem.GetPrefabProjectFile(prefabProject);
        serializer.Serialize<PrefabProject>(prefabDescriptionFile, prefabProject);
        logger.Information("Version {0} of prefab {1} was updated.", prefabVersion.ToString(), prefabProject.PrefabName);
    }

    ///<inheritdoc/>
    public async Task SavePrefabDataAsync(PrefabProject prefabProject, PrefabVersion prefabVersion)
    {
        if (IsOnlineMode)
        {
            string projectDirectory = Path.Combine(this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject), prefabVersion.ToString());
            
            string processsFile = Path.Combine(projectDirectory, Constants.PrefabProcessFileName);
            await AddOrUpdateDataFileAsync(prefabProject, prefabVersion, processsFile, prefabProject.PrefabId);

            string templateFile = Path.Combine(projectDirectory, Constants.PrefabTemplateFileName);
            if(File.Exists(templateFile))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabVersion, templateFile, prefabProject.PrefabId);
            }
                       
            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ReferencesDirectory), "*.*"))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabVersion, file, prefabProject.PrefabId);
            }            

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.DataModelDirectory), "*.cs"))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabVersion, file, prefabProject.PrefabId);
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ScriptsDirectory), "*.csx"))
            {
                await AddOrUpdateDataFileAsync(prefabProject, prefabVersion, file, prefabProject.PrefabId);
            }

            logger.Information("Data for version {0} of prefab {1} was saved.", prefabVersion.ToString(), prefabProject.PrefabName);
        }
    }

    /// <summary>
    /// Download all the files having specific tags belonging to the version of the AutomationProject being managed
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    async Task DownloadFilesWithTagsAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string[] tags)
    {
        if (IsOnlineMode)
        {
            //Download data model files and scripts
            var zippedContent = await this.filesClient.DownloadProjectDataFilesWithTags(prefabProject.PrefabId, prefabVersion.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                string versionDirectory = this.applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, prefabVersion);
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }
        }
    }

    private List<PrefabProject> LoadPrefabsFromLocalStorage(string applicationId)
    {
        List<PrefabProject> prefabProjects = new();
        var prefabDirectory = this.applicationFileSystem.GetApplicationPrefabsDirectory(applicationId);
        if (Directory.Exists(prefabDirectory))
        {
            foreach (var prefab in Directory.GetDirectories(prefabDirectory))
            {
                string prefabProjectFile = Directory.GetFiles(prefab, "*.atm", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (File.Exists(prefabProjectFile))
                {
                    PrefabProject prefabProject = serializer.Deserialize<PrefabProject>(prefabProjectFile);
                    prefabProjects.Add(prefabProject);
                    continue;
                }
                logger.Warning("Prefab project file : {0} doesn't exist", prefabProjectFile);
            }
        }
        return prefabProjects;
    }

    private PrefabProject LoadPrefabFromLocalStorage(string applicationId, string prefabId)
    {        
        var prefabProjectFile = Path.Combine(this.applicationFileSystem.GetApplicationPrefabsDirectory(applicationId), prefabId, $"{prefabId}.atm");      
        if (File.Exists(prefabProjectFile))
        {
            PrefabProject prefabProject = serializer.Deserialize<PrefabProject>(prefabProjectFile);
            return prefabProject;
        }
        throw new FileNotFoundException($"File {prefabProjectFile} doesn't exist.");
    }

}
