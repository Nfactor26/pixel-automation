using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pixel.Persistence.Services.Client.Models;

public class TestSession 
{
    /// <summary>
    /// Identifier of the session
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Id of the Session Template that was used to start this session
    /// </summary> 
    public string TemplateId { get; set; }

    /// <summary>
    /// Name of the Session Template that was used to start this session
    /// </summary>
    public string TemplateName { get; set; }

    /// <summary>
    /// Id of the Project executed in test session
    /// </summary>
    public string ProjectId { get; set; }

    /// <summary>
    /// Name of the Project executed in test session
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Version of the project executed in test session
    /// </summary>
    public string ProjectVersion { get; set; }

    /// <summary>
    /// Name of the Machine where test session was executed
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// Operating System on which test session was execued
    /// </summary>
    public string OSDetails { get; set; }


    /// <summary>
    /// Date and time when the test session was executed
    /// </summary>
    public DateTime SessionStartTime { get; set; }

    /// <summary>
    /// Total session time in minutes
    /// </summary>
    public double SessionTime { get; set; }

    /// <summary>
    /// Number of tests executed in the session
    /// </summary>     
    public int TotalNumberOfTests { get; set; }


    /// <summary>
    /// Number of tests passed in the session
    /// </summary>      
    public int NumberOfTestsPassed { get; set; }


    /// <summary>
    /// Number of tests failed in the session
    /// </summary>      
    public int NumberOfTestsFailed { get; set; }

    /// <summary>
    /// Indicates whether Session is in progress or completed
    /// </summary>      
    public SessionStatus SessionStatus { get; set; }

    /// <summary>
    /// Indicates whether all the tests passed in the session
    /// </summary>      
    public TestStatus SessionResult { get; set; }

    public bool IsProcessed { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TestSession()
    {
        this.SessionStartTime = DateTime.Now.ToUniversalTime();
        this.SessionStatus = SessionStatus.InProgress;
        this.MachineName = Environment.MachineName;
        this.OSDetails = RuntimeInformation.OSDescription;
    }


    public TestSession(SessionTemplate template, string projectVersion) : this()
    {
        this.TemplateId = template.Id;
        this.TemplateName = template.Name;
        this.ProjectId = template.ProjectId;
        this.ProjectName = template.ProjectName;
        this.ProjectVersion = projectVersion;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectVersion">Version of the project</param>
    public TestSession(string projectId, string projectName, string projectVersion) : this()
    {
        this.ProjectId = projectId;
        this.ProjectName = projectName;
        this.ProjectVersion = projectVersion;
    }


    public void OnFinished(IEnumerable<TestResult> testResultCollection)
    {
        this.SessionStatus = SessionStatus.Completed;
        this.TotalNumberOfTests = testResultCollection.Count();
        this.NumberOfTestsPassed = testResultCollection.Where(t => t.Result.Equals(TestStatus.Success)).Count();
        this.NumberOfTestsFailed = testResultCollection.Where(a => !a.Result.Equals(TestStatus.Success)).Count();
        this.SessionResult = testResultCollection.All(a => a.Result.Equals(TestStatus.Success)) ? TestStatus.Success : TestStatus.Failed;

        double sessionTime = 0;
        foreach (var result in testResultCollection)
        {
            sessionTime += result.ExecutionTime;
        }
        this.SessionTime = sessionTime / 60;
    }

}
