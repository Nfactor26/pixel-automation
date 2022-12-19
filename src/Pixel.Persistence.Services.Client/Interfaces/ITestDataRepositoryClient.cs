using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITestDataRepositoryClient
    {        
        /// <summary>
        /// Get a TestDataSource by Id for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        Task<TestDataSource> GetByIdAsync(string projectId, string projectVersion, string dataSourceId);
   
        /// <summary>
        /// Get a TestDataSource by name for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<TestDataSource> GetByNameAsync(string projectId, string projectVersion, string name);

        /// <summary>
        /// Get all the TestDataSources belonging to a given version of project which were updated since specified time
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="laterThan"></param>
        /// <returns></returns>
        Task<IEnumerable<TestDataSource>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan);

        /// <summary>
        /// Add a TestDataSource to a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="testData"></param>
        /// <returns></returns>
        Task<TestDataSource> AddDataSourceAsync(string projectId, string projectVersion, TestDataSource testData);

        /// <summary>
        /// Update an existing TestDataSource for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        Task<TestDataSource> UpdateDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource);

        /// <summary>
        /// Delete an existing TestDataSource from a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="dataSourceId"></param>
        /// <returns></returns>
        Task DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId);     
    
    }
}