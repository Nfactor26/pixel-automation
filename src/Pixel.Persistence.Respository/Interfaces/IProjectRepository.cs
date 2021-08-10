using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// IProjectRepository is used to manage the project data files stored in database
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Add or Update Project data
        /// </summary>
        /// <param name="projectMetaData"><see cref="ProjectMetaData"/> of the project</param>
        /// <param name="fileName">FileName to save contents to</param>
        /// <param name="fileData">Content of the files</param>
        /// <returns></returns>
        Task AddOrUpdateProject(ProjectMetaData projectMetaData, string fileName, byte[] fileData);

        /// <summary>
        /// Get the project file  
        /// </summary>
        /// <param name="projectId">ProjectId of the project whose data files are required</param>
        /// <returns></returns>
        Task<byte[]> GetProjectFile(string projectId);

        /// <summary>
        /// Get the project data files (zipped) for a given version of project.
        /// Project data files include process, test cases, scripts, assembiles, etc.
        /// </summary>
        /// <param name="projectId">ProjectId of the project whose data files are required</param>
        /// <param name="version">Version of the project</param>
        /// <returns></returns>
        Task<byte[]> GetProjectDataFiles(string projectId, string version);      
       

        /// <summary>
        /// Get all the availalbe <see cref="ProjectMetaData"/> 
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<ProjectMetaData> GetProjectsMetadataAsync();

        /// <summary>
        /// Get all the available <see cref="ProjectMetaData"/> for a given project
        /// </summary>
        /// <param name="projectId">ProjectId of the project whose meta data needs to be retrieved</param>
        /// <returns></returns>
        IAsyncEnumerable<ProjectMetaData> GetProjectMetadataAsync(string projectId);

        /// <summary>
        /// Remove the file revisions as per specified purge strategy.
        /// As user saves project, revisions accumulate over time and older revisions shold be removed.
        /// </summary>
        /// <param name="purgeStrategy"></param>
        /// <returns></returns>
        Task PurgeRevisionFiles(RetentionPolicy purgeStrategy);


    }
}
