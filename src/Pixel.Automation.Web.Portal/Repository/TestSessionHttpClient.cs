using Microsoft.AspNetCore.WebUtilities;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Repository
{
    public class TestSessionHttpClient
    {
        private readonly HttpClient http;
       
        public TestSessionHttpClient(HttpClient http)
        {
            this.http = http;           
        }

        public async Task<PagedList<TestSession>> GetAvailableSessions(TestSessionRequest sessionFilter)
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
                
                var response = await http.GetFromJsonAsync<PagedList<TestSession>>(QueryHelpers.AddQueryString("api/TestSession", queryStringParam));
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());          
                return new PagedList<TestSession>(Enumerable.Empty<TestSession>(), 0, 0, 0);
            }
           
        }
    }
}
