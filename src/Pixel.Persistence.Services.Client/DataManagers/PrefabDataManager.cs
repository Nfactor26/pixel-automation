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
    public async Task<Entity> GetPrefabEntity(Prefab prefab)
    {
        var prefabEntityFile = Path.Combine(GetPrefabDirectory(prefab), $"{prefab.PrefabId}.proc");
        return serializer.Deserialize<Entity>(prefabEntityFile);
    }

    ///<inheritdoc/>
    public async Task<Prefab> GetPrefabByIdAsync(string applicationId, string prefabId, string version)
    {
        if(IsOnlineMode)
        {
            return await this.prefabsRepositoryClient.GetByIdAsync(prefabId, version);
        }
        string prefabsDirectory = GetPrefabDirectory(applicationId, prefabId, version);
        string prefabFile = Path.Combine(prefabsDirectory, $"{Path.GetFileName(prefabId)}.prefab");
        return serializer.Deserialize<Prefab>(prefabFile);
    }

    ///<inheritdoc/>
    public IEnumerable<Prefab> GetAllPrefabs(string applicationId)
    {       
       string appPrefabsDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
            Constants.PrefabsDirectory);
        foreach (var prefabDirectory in Directory.GetDirectories(appPrefabsDirectory))
        {
            var latestRevision = Directory.EnumerateDirectories(prefabDirectory).Max(a => a);
            string prefabFile = Path.Combine(latestRevision, $"{Path.GetFileName(prefabDirectory)}.prefab");
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
                var latestRevision = Directory.EnumerateDirectories(prefabDirectory).Max(a => a);
                string prefabFile = Path.Combine(latestRevision, $"{prefabId}.prefab");
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
    public async Task DownloadPrefabDataAsync(Prefab prefab)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();

        if (IsOnlineMode)
        {           
            await DownloadFilesWithTagsAsync(prefab, new[] { prefab.PrefabId });
            logger.Information("Download completed for version {0} of prefab project {1}", prefab.Version.ToString(), prefab.DisplayName);
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadPrefabDataFileByNameAsync(Prefab prefab, string fileName)
    {
        Guard.Argument(prefab, nameof(prefab)).NotNull();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        if (IsOnlineMode)
        {
            var file = await this.filesRepositoryClient.DownProjectDataFile(prefab.PrefabId, prefab.Version.ToString(), fileName);
            var prefabsDirectory = GetPrefabDirectory(prefab);
            using (MemoryStream ms = new MemoryStream(file.Bytes))
            {
                using (FileStream fs = new FileStream(Path.Combine(prefabsDirectory, file.FilePath), FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                }
            }
            logger.Information("File : '{0}' was downloaded for version : '{1}' of prefab project : '{2}'.", file.FilePath, prefab.Version.ToString(), prefab.DisplayName);
        }
    }   

    ///<inheritdoc/>
    public async Task AddPrefabToScreenAsync(Prefab prefab, string screenId)
    {
        var prefabFiles = GetPrefabFiles(prefab);
        if (string.IsNullOrEmpty(prefab.ScriptFile))
        {
            prefab.ScriptFile = Path.GetRelativePath(Environment.CurrentDirectory, prefabFiles.ScriptFile);
            using (var fs = File.CreateText(prefabFiles.ScriptFile))
            {
                fs.Write(string.Empty);
            }
        }
        serializer.Serialize<Prefab>(prefabFiles.PrefabFile, prefab);
        serializer.Serialize<Entity>(prefabFiles.ProcessFile, prefab.WorkFlowEntity);        

        if (IsOnlineMode)
        {
            await this.prefabsRepositoryClient.AddPrefabToScreenAsync(prefab, screenId);
            await AddOrUpdateDataFileAsync(prefab, prefabFiles.ProcessFile, prefab.PrefabId);
            await AddOrUpdateDataFileAsync(prefab, prefabFiles.ScriptFile, prefab.PrefabId);
        }     

        ////when a new prefab is created , add it to the prefabs cache
        //if (prefabsCache.ContainsKey(prefab.ApplicationId))
        //{
        //    var prefabsForApplicationId = prefabsCache[prefab.ApplicationId];
        //    if (!prefabsForApplicationId.Any(p => p.PrefabId.Equals(prefab.PrefabId)))
        //    {
        //        prefabsForApplicationId.Add(prefab);
        //    }
        //}
        //else
        //{
        //    prefabsCache.Add(prefab.ApplicationId, [prefab]);
        //}
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
        string prefabFile = Path.Combine(GetPrefabDirectory(prefab), $"{prefab.PrefabId}.prefab");
        serializer.Serialize<Prefab>(prefabFile, prefab);
        logger.Information("Version {0} of prefab {1} was updated.", prefab.Version.ToString(), prefab.DisplayName);
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
        string prefabFile = Path.Combine(GetPrefabDirectory(prefab), $"{prefab.PrefabId}.prefab");
        serializer.Serialize<Prefab>(prefabFile, prefab);
    }

    ///<inheritdoc/>
    public async Task<Prefab> CreateRevision(Prefab fromPrefab, Version newVersion)
    {
        var clonedPrefab = fromPrefab.Clone() as Prefab;
        clonedPrefab.PrefabId = fromPrefab.PrefabId;
        clonedPrefab.Version = newVersion;

        string versionDirectory = GetPrefabDirectory(clonedPrefab);
        if (Directory.Exists(versionDirectory))
        {
            throw new ArgumentException($"{newVersion.ToString()} already exists for Prefab {fromPrefab.DisplayName}");
        }
        Directory.CreateDirectory(versionDirectory);

        string existing = GetPrefabDirectory(fromPrefab);
        foreach(var file in Directory.GetFiles(existing))
        {
            File.Copy(file, Path.Combine(versionDirectory, Path.GetFileName(file)));
        }

        await UpdatePrefabAsync(clonedPrefab);
        await SavePrefabDataAsync(clonedPrefab);


        ////when a new prefab is created , add it to the prefabs cache
        //if (prefabsCache.ContainsKey(clonedPrefab.ApplicationId))
        //{
        //    var prefabsForApplicationId = prefabsCache[clonedPrefab.ApplicationId];
        //    var existingEntry = prefabsForApplicationId.FirstOrDefault(p => p.PrefabId.Equals(clonedPrefab.PrefabId));
        //    if(existingEntry is not null)
        //    {
        //        prefabsForApplicationId.Remove(existingEntry);
        //    }
        //    prefabsForApplicationId.Add(clonedPrefab);
        //}
        //else
        //{
        //    prefabsCache.Add(clonedPrefab.ApplicationId, [clonedPrefab]);
        //}

        return clonedPrefab;
    }

    ///<inheritdoc/>
    public async Task SavePrefabDataAsync(Prefab prefab)
    {      
        var prefabFiles = GetPrefabFiles(prefab);
        serializer.Serialize<Entity>(prefabFiles.ProcessFile, prefab.WorkFlowEntity);

        if (IsOnlineMode)
        {
            await AddOrUpdateDataFileAsync(prefab, prefabFiles.ProcessFile, prefab.PrefabId);
            string prefabsDirectory = GetPrefabDirectory(prefab);
            foreach(var scriptFile in Directory.GetFiles(prefabsDirectory, "*.csx"))
            {
                await AddOrUpdateDataFileAsync(prefab, scriptFile, prefab.PrefabId);
            }
        }
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateDataFileAsync(Prefab prefab, string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            var prefabsDirectory = GetPrefabDirectory(prefab);
            await this.filesRepositoryClient.AddProjectDataFile(new ProjectDataFile()
            {
                ProjectId = prefab.PrefabId,
                ProjectVersion = prefab.Version.ToString(),
                FileName = Path.GetFileName(filePath),
                FilePath = Path.GetRelativePath(prefabsDirectory, filePath),
                Tag = tag,
            }, filePath);
            logger.Information("File {0} belonging to version {1} of prefab project {2} was added/updated.", Path.GetFileName(filePath), prefab.Version.ToString(), prefab.DisplayName);
        }
    }

    /// <inheritdoc/>  
    public async Task DeleteDataFileAsync(Prefab prefab, string fileToDelete)
    {
        if (IsOnlineMode)
        {
            await this.filesRepositoryClient.DeleteProjectDataFile(prefab.PrefabId, prefab.Version.ToString(), Path.GetFileName(fileToDelete));
        }
        if (File.Exists(fileToDelete))
        {
            File.Delete(fileToDelete);
        }
        logger.Information("File {0} belonging to version {1} of prefab {2} was deleted.", Path.GetFileName(fileToDelete), prefab.Version.ToString(), prefab.DisplayName);
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
    async Task DownloadFilesWithTagsAsync(Prefab prefab, string[] tags)
    {
        if (IsOnlineMode)
        {
            //Download data model files and scripts
            var zippedContent = await this.filesRepositoryClient.DownloadProjectDataFilesWithTags(prefab.PrefabId, prefab.Version.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                string versionDirectory = GetPrefabDirectory(prefab);
                if (!Directory.Exists(versionDirectory))
                {
                    Directory.CreateDirectory(versionDirectory);
                }
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }
        }
    }   

    string GetPrefabDirectory(Prefab prefab)
    {
        return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefab.ApplicationId,
            Constants.PrefabsDirectory, prefab.PrefabId, prefab.Version.ToString());
    }

    string GetPrefabDirectory(string applicationId, string prefabId)
    {
        return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
            Constants.PrefabsDirectory, prefabId);
    }

    string GetPrefabDirectory(string applicationId, string prefabId, string version)
    {
        return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
            Constants.PrefabsDirectory, prefabId, version);
    }

    public PrefabFiles GetPrefabFiles(Prefab prefab)
    {
        return new PrefabFiles(prefab, GetPrefabDirectory(prefab));
    }

}