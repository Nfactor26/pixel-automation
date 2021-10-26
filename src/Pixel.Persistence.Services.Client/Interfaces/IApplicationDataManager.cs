﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{ 
    public interface IApplicationDataManager
    {
        /// <summary>
        /// Delete application and automation data directory from local disk
        /// </summary>
        void CleanLocalData();

        /// <summary>
        /// Load all application description from disk (e.g. for use in application explorer) and return loaded application descriptions
        /// </summary>
        /// <returns></returns>
        IEnumerable<ApplicationDescription> GetAllApplications();

        /// <summary>
        /// Add or update a application file
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription);

        /// <summary>
        /// Download all the newer application files
        /// </summary>
        /// <returns></returns>
        Task DownloadApplicationsDataAsync();
        

        /// <summary>
        /// Load all the control files from disk for a given application (e.g. for use in control explorer) and return loaded control descriptions
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription);


        /// <summary>
        /// Add or update a control file
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        Task AddOrUpdateControlAsync(ControlDescription controlDescription);
      
        /// <summary>
        /// Add or update control image for control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="stream"></param>      
        /// <returns></returns>
        Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream stream);

        /// <summary>
        /// Delete specified imageFile for control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        Task DeleteControlImageAsync(ControlDescription controlDescription, string imageFile);

        /// <summary>
        /// Load all the automatin project from disk and return loaded projects
        /// </summary>
        /// <returns></returns>
        IEnumerable<AutomationProject> GetAllProjects();

        /// <summary>
        /// Add or update project along with its data files for a given version
        /// </summary>
        /// <param name="automationProject"></param>
        /// <param name="projectVersion"></param>
        /// <returns></returns>
        Task AddOrUpdateProjectAsync(AutomationProject automationProject, VersionInfo projectVersion);
       
     
        /// <summary>
        /// Download project data files for a given version if newer
        /// </summary>
        /// <param name="automationProject"></param>
        /// <param name="projectVersion"></param>
        /// <returns></returns>
        Task DownloadProjectDataAsync(AutomationProject automationProject, VersionInfo projectVersion);
       
        /// <summary>
        /// Download all the newer project files
        /// </summary>
        /// <returns></returns>
        Task DownloadProjectsAsync();

        /// <summary>
        /// Load all the prefabs for a given applicationId from local storage
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        IEnumerable<PrefabProject> GetAllPrefabs(string applicationId);

        /// <summary>
        /// Find and load PrefabProject from local storage
        /// </summary>
        /// <param name="applicationId">applicationId of the prefab</param>
        /// <param name="prefabId">string prefabId of the prefab</param>
        /// <returns></returns>
        PrefabProject GetPrefab(string applicationId, string prefabId);

        Task AddOrUpdatePrefabAsync(PrefabProject prefabProject, VersionInfo prefabVersion);

        Task AddOrUpdatePrefabDataFilesAsync(PrefabProject prefabProject, VersionInfo prefabVersion);

        Task DownloadPrefabFileAsync(string applicationId, string prefabId);

        Task DownloadPrefabDataAsync(PrefabProject prefabProject, string version);
    }

}