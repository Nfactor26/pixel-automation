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

public class PrefabDataManager : IPrefabDataManager
{
    private readonly ILogger logger = Log.ForContext<PrefabDataManager>();
    private readonly ApplicationSettings applicationSettings;
    private readonly ISerializer serializer;  
    private readonly IPrefabRepositoryClient prefabsRepositoryClient;
    private readonly IApplicationRepositoryClient appRepositoryClient;  
    private readonly IProjectFilesRepositoryClient filesRepositoryClient;
    private readonly Dictionary<string, List<Prefab>> prefabsCache = new();

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
        IPrefabRepositoryClient prefabsRepositoryClient,IProjectFilesRepositoryClient filesRepositoryClient)
    {
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.prefabsRepositoryClient = Guard.Argument(prefabsRepositoryClient).NotNull().Value;            
        this.filesRepositoryClient = Guard.Argument(filesRepositoryClient).NotNull().Value;
    }


    ///<inheritdoc/>
    public async Task<Entity> GetPrefabEntity(Prefab prefab, VersionInfo version)
    {
        var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);       
        return serializer.Deserialize<Entity>(fileSystem.ProcessFile);
    }

    ///<inheritdoc/>
    public async Task<Prefab> GetPrefabByIdAsync(string applicationId, string prefabId, string version)
    {
        if(IsOnlineMode)
        {
            return await this.prefabsRepositoryClient.GetByIdAsync(prefabId, version);
        }
        var fileSystem = GetPrefabFileSystem(new Prefab() { PrefabId = prefabId, ApplicationId = applicationId }, new VersionInfo(version));   
        return serializer.Deserialize<Prefab>(fileSystem.PrefabFile);
    }

    ///<inheritdoc/>
    public IEnumerable<Prefab> GetAllPrefabs(string applicationId)
    {       
       string appPrefabsDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
            Constants.PrefabsDirectory);
        foreach (var prefabDirectory in Directory.GetDirectories(appPrefabsDirectory))
        {           
            string prefabFile = Path.Combine(prefabDirectory, $"{Path.GetFileName(prefabDirectory)}.prefab");
            if (File.Exists(prefabFile))
            {
                Prefab prefab = serializer.Deserialize<Prefab>(prefabFile);
                yield return prefab;
            }
        }
        yield break;
    }    

    ///<inheritdoc/>
    public IEnumerable<Prefab> GetPrefabsForScreen(ApplicationDescription applicationDescription, string screenName)
    {
        if (applicationDescription.Screens.Any(s => s.ScreenName.Equals(screenName)))
        {
            var applicationScreen = applicationDescription.Screens.Single(s => s.ScreenName.Equals(screenName));      
            foreach (var prefabId in applicationScreen.AvailablePrefabs)
            {
                string prefabDirectory = GetPrefabDirectory(applicationDescription.ApplicationId, prefabId);              
                string prefabFile = Path.Combine(prefabDirectory, $"{prefabId}.prefab");
                if (File.Exists(prefabFile))
                {
                    Prefab prefab = serializer.Deserialize<Prefab>(prefabFile);
                    yield return prefab;
                }
            }
        }
        yield break;
    }

    ///<inheritdoc/>
    public async Task DownloadPrefabDataAsync(Prefab prefab, VersionInfo version)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();

        if (IsOnlineMode)
        {           
            await DownloadFilesWithTagsAsync(prefab, version, new[] { prefab.PrefabId });
            logger.Information("Download completed for version {0} of prefab project {1}", version.ToString(), prefab.DisplayName);
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadPrefabDataFileByNameAsync(Prefab prefab, VersionInfo version, string fileName)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        if (IsOnlineMode)
        {
            var file = await this.filesRepositoryClient.DownProjectDataFile(prefab.PrefabId, version.ToString(), fileName);
            var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);
            using (MemoryStream ms = new MemoryStream(file.Bytes))
            {
                using (FileStream fs = new FileStream(Path.Combine(fileSystem.WorkingDirectory, file.FilePath), FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }
            logger.Information("File : '{0}' was downloaded for version : '{1}' of prefab project : '{2}'.", file.FilePath, version.ToString(), prefab.DisplayName);
        }
    }   

    ///<inheritdoc/>
    public async Task AddPrefabToScreenAsync(Prefab prefab, Entity prefabEntity, string screenId)
    {
        var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);     
        serializer.Serialize<Prefab>(fileSystem.PrefabFile, prefab);
        serializer.Serialize<Entity>(fileSystem.ProcessFile, prefabEntity);
        fileSystem.CreateOrReplaceFile(fileSystem.ScriptsDirectory, Path.GetFileName(fileSystem.ScriptFile), string.Empty);
        fileSystem.CreateOrReplaceFile(fileSystem.DataModelDirectory, Path.GetFileName(fileSystem.DataModelFile), string.Empty);

        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.AddPrefabToScreenAsync(prefab, screenId);
            await SavePrefabDataAsync(prefab, prefab.LatestActiveVersion);
        }
    }

    /// <inheritdoc/>
    public async Task MovePrefabToScreen(Prefab prefab, string targetScreenId)
    {
        if (IsOnlineMode)
        {
            await appRepositoryClient.MovePrefabToScreen(prefab, targetScreenId);
        }
    }

    ///<inheritdoc/>
    public async Task UpdatePrefabAsync(Prefab prefab)
    {
        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.UpdatePrefabAsync(prefab);
        }
        var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);
        serializer.Serialize<Prefab>(fileSystem.PrefabFile, prefab);
        logger.Information("Prefab {1} was updated.", prefab.DisplayName);
    }

    ///<inheritdoc/>
    public async Task DeletePrefabAsync(Prefab prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.DeletePrefabAsync(prefab);
        }
        prefab.IsDeleted = true;
        var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);
        serializer.Serialize<Prefab>(fileSystem.PrefabFile, prefab);
    }

    ///<inheritdoc/>
    public async Task<Prefab> AddPrefabVersionAsync(Prefab prefab, VersionInfo newVersion, VersionInfo cloneFrom)
    {
        var fileSystemPrevious = GetPrefabFileSystem(prefab, cloneFrom);     
        var fileSystemNew = GetPrefabFileSystem(prefab, newVersion);       
       
        Directory.CreateDirectory(fileSystemNew.WorkingDirectory);
        CopyAll(new DirectoryInfo(fileSystemPrevious.WorkingDirectory), new DirectoryInfo(fileSystemNew.WorkingDirectory));

        prefab.AvailableVersions.Add(newVersion);
        await UpdatePrefabAsync(prefab);
        await SavePrefabDataAsync(prefab, prefab.LatestActiveVersion);

        return prefab;


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

    ///<inheritdoc/>
    public async Task SavePrefabEntityAsync(Prefab prefab, VersionInfo version, Entity prefabEntity)
    {
        var fileSystem = GetPrefabFileSystem(prefab, version);
        serializer.Serialize<Entity>(fileSystem.ProcessFile, prefabEntity);
        if(IsOnlineMode)
        {
            await AddOrUpdateDataFileAsync(prefab, version, fileSystem.ProcessFile, prefab.PrefabId);
            foreach (var scriptFile in Directory.GetFiles(fileSystem.ScriptsDirectory, "*.csx"))
            {
                await AddOrUpdateDataFileAsync(prefab, version, scriptFile, prefab.PrefabId);
            }
        }

    }

    ///<inheritdoc/>
    public async Task SavePrefabDataAsync(Prefab prefab, VersionInfo version)
    {
        var fileSystem = GetPrefabFileSystem(prefab, version);       
        if (IsOnlineMode)
        {
            await AddOrUpdateDataFileAsync(prefab, version, fileSystem.ProcessFile, prefab.PrefabId);           
            await AddOrUpdateDataFileAsync(prefab, version, fileSystem.DataModelFile, prefab.PrefabId);          
            foreach(var scriptFile in Directory.GetFiles(fileSystem.ScriptsDirectory, "*.csx"))
            {
                await AddOrUpdateDataFileAsync(prefab, version, scriptFile, prefab.PrefabId);
            }
        }
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateDataFileAsync(Prefab prefab, VersionInfo version, string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            var fileSystem = GetPrefabFileSystem(prefab, prefab.LatestActiveVersion);
            await this.filesRepositoryClient.AddProjectDataFile(new ProjectDataFile()
            {
                ProjectId = prefab.PrefabId,
                ProjectVersion = version.ToString(),
                FileName = Path.GetFileName(filePath),
                FilePath = Path.GetRelativePath(fileSystem.WorkingDirectory, filePath),
                Tag = tag,
            }, filePath);
            logger.Information("File {0} belonging to version {1} of prefab project {2} was added/updated.", Path.GetFileName(filePath), version.ToString(), prefab.DisplayName);
        }
    }

    /// <inheritdoc/>  
    public async Task DeleteDataFileAsync(Prefab prefab, VersionInfo version, string fileToDelete)
    {
        if (IsOnlineMode)
        {
            await this.filesRepositoryClient.DeleteProjectDataFile(prefab.PrefabId, version.ToString(), Path.GetFileName(fileToDelete));
        }
        if (File.Exists(fileToDelete))
        {
            File.Delete(fileToDelete);
        }
        logger.Information("File {0} belonging to version {1} of prefab {2} was deleted.", Path.GetFileName(fileToDelete), version.ToString(), prefab.DisplayName);
    }

    /////<inheritdoc/>
    //public async Task AddPrefabVersionAsync(PrefabProject prefabProject, VersionInfo newVersion, VersionInfo cloneFrom)
    //{
    //    if (IsOnlineMode)
    //    {
    //        await this.prefabsRepositoryClient.AddPrefabVersionAsync(prefabProject.ProjectId, newVersion, cloneFrom);
    //        await DownloadPrefabDataAsync(prefabProject, newVersion);
    //    }
    //    else
    //    {
    //        var cloneFromVersionDirectory = Path.Combine(applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, cloneFrom.ToString()));
    //        var newVersionDirectory = Path.Combine(applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, newVersion.ToString()));
    //        Directory.CreateDirectory(newVersionDirectory);

    //        ////copy contents from previous version directory to new version directory
    //        CopyAll(new DirectoryInfo(cloneFromVersionDirectory), new DirectoryInfo(newVersionDirectory));

    //        void CopyAll(DirectoryInfo source, DirectoryInfo target)
    //        {
    //            // Copy each file into the new directory.
    //            foreach (FileInfo fi in source.GetFiles())
    //            {
    //                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
    //            }
    //            // Copy each subdirectory using recursion.
    //            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
    //            {
    //                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
    //                CopyAll(diSourceSubDir, nextTargetSubDir);
    //            }
    //        }
    //    }

    //    prefabProject.AvailableVersions.Add(newVersion);
    //    string prefabDescriptionFile = this.applicationFileSystem.GetPrefabProjectFile(prefabProject);
    //    serializer.Serialize<PrefabProject>(prefabDescriptionFile, prefabProject);
    //    logger.Information("Added new version {0} of prefab {1} from version {2}.", newVersion.ToString(), prefabProject.Name, cloneFrom.ToString());
    //}     
  
  
    /// <summary>
    /// Download all the files having specific tags belonging to the version of the AutomationProject being managed
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    async Task DownloadFilesWithTagsAsync(Prefab prefab, VersionInfo version, string[] tags)
    {
        if (IsOnlineMode)
        {
            //Download data model files and scripts
            var zippedContent = await this.filesRepositoryClient.DownloadProjectDataFilesWithTags(prefab.PrefabId, version.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                var fileSystem = GetPrefabFileSystem(prefab, version);
                if (!Directory.Exists(fileSystem.WorkingDirectory))
                {
                    Directory.CreateDirectory(fileSystem.WorkingDirectory);
                }
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(fileSystem.WorkingDirectory, true);
                }
            }
        }
    }  

    string GetPrefabDirectory(string applicationId, string prefabId)
    {
        return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
            Constants.PrefabsDirectory, prefabId);
    }

    public IPrefabFileSystem GetPrefabFileSystem(Prefab prefab, VersionInfo version)
    {
        var fileSystem = new PrefabFileSystem(this.serializer, this.applicationSettings);
        fileSystem.Initialize(prefab, version);
        return fileSystem;
    }

}