using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IFixturesRepositoryClient
{
    /// <summary>
    /// Get TestFixture by  Id for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fixtureId"></param>
    /// <returns></returns>
    Task<TestFixture> GetByIdAsync(string projectId, string projectVersion, string fixtureId);

    /// <summary>
    /// Get TestFixture by name for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="displayName"></param>
    /// <returns></returns>
    Task<TestFixture> GetByNameAsync(string projectId, string projectVersion, string displayName);

    /// <summary>
    /// Get all TestFixtures for a given version of project which were modified since specified time
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<TestFixture>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan);

    /// <summary>
    /// Add a new TestFixture for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task<TestFixture> AddFixtureAsync(string projectId, string projectVersion, TestFixture testFixture);

    /// <summary>
    /// Update an existing TestFixture for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task<TestFixture> UpdateFixtureAsync(string projectId, string projectVersion, TestFixture testFixture);

    /// <summary>
    /// Delete an existing TestFixture for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fixtureId"></param>
    /// <returns></returns>
    Task DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId);
}
