using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner;

public class TestSessionManager
{
    private readonly ApplicationSettings appSettings;
    private readonly ITestSessionClient testSessionClient;

    private TestSession testSession = null;
    private readonly List<TestResult> testResults = new List<TestResult>();

    bool IsOnlineMode
    {
        get => !this.appSettings.IsOfflineMode;
    }

    public TestSessionManager(ApplicationSettings appSettings, ITestSessionClient testSessionClienet)
    {
        this.appSettings = Guard.Argument(appSettings, nameof(appSettings)).NotNull();
        this.testSessionClient = Guard.Argument(testSessionClienet, nameof(testSessionClienet)).NotNull().Value;
    }


    public async Task<string> AddSessionAsync(TestSession testSession)
    {
        if(IsOnlineMode)
        {
            return await this.testSessionClient.AddSessionAsync(testSession);
        }
        this.testSession = testSession;
        this.testSession.Id = Guid.NewGuid().ToString();
        return this.testSession.Id;
    }

    public async Task UpdateSessionAsync(string sessionId, TestSession testSession)
    {
        if(IsOnlineMode)
        {
            await this.testSessionClient.UpdateSessionAsync(sessionId, testSession);
        }
        this.testSession = testSession;
    }

    public async Task<TestResult> AddResultAsync(TestResult testResult)
    {
        if(IsOnlineMode)
        {
            return await this.testSessionClient.AddResultAsync(testResult);
        }
        testResult.Id = testResult.TestId;
        this.testResults.Add(testResult);
        return testResult;
    }

    public async Task AddTraceImagesAsync(TestResult testResult, IEnumerable<string> imageFiles)
    {
        if(IsOnlineMode)
        {
            await this.testSessionClient.AddTraceImagesAsync(testResult, imageFiles);
        }
    }
}
