using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces
{
    public interface ITestDataRepository
    {
        /// <summary>
        /// Get TestDataSource by Id for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="fixtureId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TestDataSource> FindByIdAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken);

        /// <summary>
        /// Get TestDataSource by name for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TestDataSource> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken);

        /// <summary>
        /// Get all the TestDataSources available for a given version of project that were modified since specified datetime
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="laterThan"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<TestDataSource>> GetDataSourcesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken);

        /// <summary>
        /// Add a TestDataSource to a given version of project
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddDataSourceAsync(string projectId, string projectVersion, string groupName, TestDataSource dataSource, CancellationToken cancellationToken);

        /// <summary>
        /// Add multiple TestDataSources to a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSources"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddDataSourcesAsync(string projectId, string projectVersion, IEnumerable<TestDataSource> dataSources, CancellationToken cancellationToken);

        /// <summary>
        /// Delete an existing TestDataSource for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId, CancellationToken cancellationToken);   
             
        /// <summary>
        /// Update a TestDataSource for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource, CancellationToken cancellationToken);
    }
}