using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SortDirection = Pixel.Persistence.Core.Enums.SortDirection;

namespace Pixel.Automation.Web.Portal.Components
{
    public partial class TestResultsTable : ComponentBase
    {       
        private readonly int[] pageSizeOptions = { 10, 20, 30, 40, 50 };
       
        private List<TestResultViewModel> TestResults { get; set; } = new List<TestResultViewModel>();

        private MudTable<TestResultViewModel> table;      
        
        private int lastNMonths = 1;
        private string fixtureName;
        private TestStatus testResult;
      
        [Inject]
        public ITestResultService Service { get; set; }

        [Parameter]
        public Func<TestResultRequest, Task<PagedList<TestResult>>> TableStateChanged { get; set; }

        [Parameter]
        public string TableHeader { get; set; }

        [Parameter]
        public bool ShowExecutionOrder { get; set; } = false;

        [Parameter]
        public bool ShowExecutionDate { get; set; } = false;

        [Parameter]
        public bool ShowStatisticsButton { get; set; } = true;

        private async Task<TableData<TestResultViewModel>> GetTestResultsAsync(TableState state)
        {          
            var testResultRequest = new TestResultRequest()
            {
                ExecutedAfter = DateTimeHelper.NMonthsBefore(lastNMonths),                
                CurrentPage = state.Page + 1,
                PageSize = state.PageSize
            };
            if(!string.IsNullOrEmpty(fixtureName))
            {
                testResultRequest.FixtureName = fixtureName;
            }
            if(testResult != TestStatus.None)
            {
                testResultRequest.Result = testResult;
            }
            if(!string.IsNullOrEmpty(state.SortLabel))
            {
                testResultRequest.SortBy = state.SortLabel;
                testResultRequest.SortDirection = (SortDirection)((int)state.SortDirection);
            }
            var result = await TableStateChanged.Invoke(testResultRequest);
            TestResults.Clear();
            TestResults.AddRange(result.Items.ToViewModel());
            return new TableData<TestResultViewModel>
            {
                Items = TestResults,
                TotalItems = result.ItemsCount
            };           
        }

        public void OnToggleErrorDetailsPanelVisibility(string rowId)
        {           
            var item = TestResults.FirstOrDefault(t => t.Id.Equals(rowId));
            item.IsErrorDetailsVisible = !item.IsErrorDetailsVisible;            
        }     

        private async Task OnMonthsChanged(int months)
        {
            this.lastNMonths = months;
            await table.ReloadServerData();
        }

        private async Task OnSearch(string queryStrings)
        {
            if (!string.IsNullOrEmpty(queryStrings))
            {
                string[] queryString = queryStrings.Split("and");
                foreach(var query in queryString)
                {
                    Console.WriteLine($"Process query : {query}");
                    string[] queryParams = query.Trim().Split(":");
                    if(queryParams.Length != 2)
                    {
                        Console.WriteLine($"Invalid query : {query}");
                        continue;
                    }
                    switch(queryParams[0].Trim())
                    {
                        case "fixture":
                            this.fixtureName = queryParams[1].Trim();
                            break;
                        case "result":
                            if(!Enum.TryParse<TestStatus>(queryParams[1].Trim(), out this.testResult))
                            {
                                Console.WriteLine($"Invalid value for result : {queryParams[1]}");
                            }
                            break;
                    }
                }
            }
            await table.ReloadServerData();
        }

        public async Task Reload()
        {
            this.lastNMonths = 1;
            await table.ReloadServerData();
        }
    }
}
