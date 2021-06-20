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
    /// <summary>
    /// Interface definition for ITestSessionService. A client can use this service to retrieve test session data.
    /// </summary>
    public interface ITestSessionService
    {
        /// <summary>
        /// Get a test session given it's sessionId
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<TestSession> GetSessionByIdAsync(string sessionId);

        /// <summary>
        /// Get all the available Test Session that match the request filter
        /// </summary>
        /// <param name="sessionFilter"></param>
        /// <returns></returns>
        Task<PagedList<TestSession>> GetAllSessionsAsync(TestSessionRequest sessionFilter);
    }

    public class TestSessionService : ITestSessionService
    {
        private readonly HttpClient http;
        private readonly ISnackbar snackBar;

        public TestSessionService(HttpClient http, ISnackbar snackBar)
        {
            this.http = http;
            this.snackBar = snackBar;
        }


        public async Task<TestSession> GetSessionByIdAsync(string sessionId)
        {
            try
            {
                var testSession = await this.http.GetFromJsonAsync<TestSession>($"api/TestSession/{sessionId}");
                return testSession;
            }
            catch (Exception ex)
            {
                snackBar.Add($"Error while retrieving Session with id : {sessionId}", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());
                return null;
            }
         
        }

        public async Task<PagedList<TestSession>> GetAllSessionsAsync(TestSessionRequest sessionFilter)
        {
            try
            {
                var queryStringParam = new Dictionary<string, string>
                {
                    ["currentPage"] = sessionFilter.CurrentPage.ToString(),
                    ["pageSize"] = sessionFilter.PageSize.ToString()
                };
                if (!string.IsNullOrEmpty(sessionFilter.ProjectName))
                {
                    queryStringParam.Add("projectName", sessionFilter.ProjectName);
                }
                if (!string.IsNullOrEmpty(sessionFilter.MachineName))
                {
                    queryStringParam.Add("projectName", sessionFilter.MachineName);
                }
                if (!string.IsNullOrEmpty(sessionFilter.TemplateName))
                {
                    queryStringParam.Add("templateName", sessionFilter.TemplateName);
                }

                var response = await this.http.GetFromJsonAsync<PagedList<TestSession>>(QueryHelpers.AddQueryString("api/TestSession", queryStringParam));
                return response;
            }
            catch (Exception ex)
            {
                snackBar.Add("Error while retrieving Sessions data", Severity.Error);
                Console.WriteLine("Error: " + ex.ToString());    
                return new PagedList<TestSession>(Enumerable.Empty<TestSession>(), 0, 0, 0);
            }
        }

    }
}
