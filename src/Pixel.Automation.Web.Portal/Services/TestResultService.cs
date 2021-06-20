using MudBlazor;
using Pixel.Persistence.Core.Models;
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
    }
}
