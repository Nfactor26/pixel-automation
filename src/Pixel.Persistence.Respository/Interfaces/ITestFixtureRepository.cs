using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

public interface ITestFixtureRepository
{
    /// <summary>
    /// Get all test fixtures for a given project version which have been modified since specified datetime
    /// </summary>
    /// <param name="projectId">Identifier of the automation project</param>
    /// <param name="projectVersion">Version of the automation project</param>
    /// <returns></returns>
    Task<IEnumerable<TestFixture>> GetFixturesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken);


    /// <summary>
    /// Get a test fixture given it's Id
    /// </summary>
    /// <param name="fixtureId"></param>
    /// <returns></returns>
    Task<TestFixture> FindByIdAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken);


    /// <summary>
    /// Get a test fixture given it's display name
    /// </summary>
    /// <param name="fixtureId"></param>
    /// <returns></returns>
    Task<TestFixture> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Add a new test fixture
    /// </summary>
    /// <param name="fixture"></param>
    /// <returns></returns>
    Task AddFixtureAsync(string projectId, string projectVersion, TestFixture fixture, CancellationToken cancellationToken);

    /// <summary>
    /// Add multiple TestFixtures to a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersio"></param>
    /// <param name="fixtures"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddFixturesAsync(string projectId, string projectVersion, IEnumerable<TestFixture> fixtures, CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing test fixture
    /// </summary>
    /// <param name="fixtureId"></param>
    /// <param name="fixture"></param>
    /// <returns></returns>
    Task UpdateFixtureAsync(string projectId, string projectVersion, TestFixture fixture, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an existing test fixture
    /// </summary>
    /// <param name="fixtureId"></param>
    /// <returns></returns>
    Task DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken);
}
