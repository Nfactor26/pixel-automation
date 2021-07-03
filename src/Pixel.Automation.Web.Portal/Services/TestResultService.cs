using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{    
    public interface ITestResultService
    {
        /// <summary>
        /// Get all the test results for a TestSession given it's sessionId
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<IEnumerable<TestResult>> GetResultsInSessionAsync(string sessionId);

        /// <summary>
        /// Get all the available Test Session that match the request filter
        /// </summary>
        /// <param name="sessionFilter"></param>
        /// <returns></returns>
        Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest request);

        /// <summary>
        /// Update the failure reason for a test case
        /// </summary>
        /// <param name="testId"></param>
        /// <param name="failureReason"></param>
        /// <returns></returns>
        Task UpdateFailureReasonAsync(string sessionId, string testId, string failureReason);
    }

    public class TestResultService : ITestResultService
    {
        private readonly HttpClient http;
        private readonly ISnackbar snackBar;

        public TestResultService(HttpClient http, ISnackbar snackBar)
        {
            this.http = http;
            this.snackBar = snackBar;
        }

        public async Task<IEnumerable<TestResult>> GetResultsInSessionAsync(string sessionId)
        {
            try
            {
                var testsInSession = await this.http.GetFromJsonAsync<IEnumerable<TestResult>>($"api/TestResult/{sessionId}");
                return testsInSession;
            }
            catch (Exception ex)
            {
                snackBar.Add($"Error while retrieving test result in session : {sessionId}", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());
                return Enumerable.Empty<TestResult>();
            }
        }

        public async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest request)
        {
            try
            {
                var queryStringParam = new Dictionary<string, string>
                {
                    ["currentPage"] = request.CurrentPage.ToString(),
                    ["pageSize"] = request.PageSize.ToString(),
                    ["executedAfter"] = request.ExecutedAfter.ToUniversalTime().ToString(),
                    //["executionTimeGte"] = request.ExecutionTimeGte.ToString(),
                    //["executionTimeLte"] = request.ExecutionTimeLte.ToString(),
                    ["result"] = request.Result.ToString()
                };
                if(!string.IsNullOrEmpty(request.SessionId))
                {
                    queryStringParam.Add("sessionId", request.SessionId);
                }
                if(!string.IsNullOrEmpty(request.ProjectId))
                {
                    queryStringParam.Add("projectId", request.ProjectId);
                }
                if (!string.IsNullOrEmpty(request.TestId))
                {
                    queryStringParam.Add("testId", request.TestId);
                }
                if (!string.IsNullOrEmpty(request.FixtureName))
                {
                    queryStringParam.Add("fixtureName", request.ProjectId);
                }
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    queryStringParam.Add("orderBy", request.OrderBy);
                }
                var response = await this.http.GetFromJsonAsync<PagedList<TestResult>>(QueryHelpers.AddQueryString("api/TestResult", queryStringParam));
                return response;
            }
            catch (Exception ex)
            {
                snackBar.Add("Error while retrieving test results data", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());
                return new PagedList<TestResult>(Enumerable.Empty<TestResult>(), 0, 0, 0);
            }
        }

        public async Task UpdateFailureReasonAsync(string sessionId, string testId, string failureReason)
        {
            try
            {
                var updateReasonRequest = new UpdateFailureReasonRequest(sessionId, testId, failureReason);
                await this.http.PutAsJsonAsync<UpdateFailureReasonRequest>("api/TestResult/failure/reason", updateReasonRequest);                
            }
            catch (Exception ex)
            {
                snackBar.Add($"Error while updating failure reason", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());                
            }
        }
    }
}
