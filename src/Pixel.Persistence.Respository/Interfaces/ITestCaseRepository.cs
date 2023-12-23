using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

public interface ITestCaseRepository
{
    /// <summary>
    /// Get all test cases for a given project version which were modified after specified datetime
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TestCase>> GetTestCasesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken);

    /// <summary>
    /// Get all the test cases for a given fixture belonging to specified version of project which were modified after specified datetime
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fixtureId"></param>
    /// <param name="laterThan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TestCase>> GetTestCasesAsync(string projectId, string projectVersion, string fixtureId, DateTime laterThan, CancellationToken cancellationToken);

    /// <summary>
    /// Get test cases by Id for a given version of project
    /// </summary>    
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TestCase> FindByIdAsync(string projectId, string projectVersion, string testId, CancellationToken cancellationToken);

    /// <summary>
    /// Get test case by display name for a given veresion of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TestCase> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Add a new TestCase to a given version of project
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddTestCaseAsync(string projectId, string projectVersion, TestCase testCase, CancellationToken cancellationToken);

    /// <summary>
    /// Add multiple TestCases to a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="tests"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddTestCasesAsync(string projectId, string projectVersion, IEnumerable<TestCase> tests, CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing test case
    /// </summary>
    /// <param name="testCaseId"></param>
    /// <param name="fixture"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateTestCaseAsync(string projectId, string projectVersion, TestCase fixture, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an existing test case
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteTestCaseAsync(string projectId, string projectVersion, string testCaseId, CancellationToken cancellationToken);
}
