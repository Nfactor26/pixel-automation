﻿using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface ITestsRepositoryClient
{
    /// <summary>
    /// Get TestCase by Id for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testId"></param>
    /// <returns></returns>
    Task<TestCase> GetByIdAsync(string projectId, string projectVersion, string testId);

    /// <summary>
    /// Get TestCase by name for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="displayName"></param>
    /// <returns></returns>
    Task<TestCase> GetByNameAsync(string projectId, string projectVersion, string displayName);

    /// <summary>
    /// Get all the TestCase for a given version of project which were modified since specified time
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<TestCase>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan);

    /// <summary>
    /// Get all the TestCase for a fixture belonging to given version of project which were modified since specified time
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fixtureId"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<TestCase>> GetAllForFixtureAsync(string projectId, string projectVersion, string fixtureId, DateTime laterThan);

    /// <summary>
    /// Add a new TestCase to a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task<TestCase> AddTestCaseAsync(string projectId, string projectVersion, TestCase testCase);

    /// <summary>
    /// Update an existing TestCase for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task<TestCase> UpdateTestCaseAsync(string projectId, string projectVersion, TestCase testCase);

    /// <summary>
    /// Delete an existing TestCase for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="testId"></param>
    /// <returns></returns>
    Task DeleteTestCaseAsync(string projectId, string projectVersion, string testId);
}
