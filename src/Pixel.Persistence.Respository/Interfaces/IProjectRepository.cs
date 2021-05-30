using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IProjectRepository
    {
        /// <summary>
        /// Get the project data files 
        /// </summary>
        /// <param name="projectId">ProjectId of the project whose data files are required</param>
        /// <returns></returns>
        Task<byte[]> GetProjectFile(string projectId);

        /// <summary>
        /// Get the project data files for a given version of project
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
        /// Add or Update Project data
        /// </summary>
        /// <param name="projectMetaData"><see cref="ProjectMetaData"/> of the project</param>
        /// <param name="fileName">FileName to save contents to</param>
        /// <param name="fileData">Content of the files</param>
        /// <returns></returns>
        Task AddOrUpdateProject(ProjectMetaData projectMetaData, string fileName, byte[] fileData);
    }
}
