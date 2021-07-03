using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components
{
    public partial class TestResultsTable : ComponentBase
    {
        private string searchString = "";
        private readonly int[] pageSizeOptions = { 10, 20, 30, 40, 50 };

        [Inject]
        public ITestResultService Service { get; set; }

        [Parameter]
        public Func<TableState, Task<PagedList<TestResult>>> TableStateChanged { get; set; }

        [Parameter]
        public string TableHeader { get; set; }

        [Parameter]
        public bool ShowExecutionOrder { get; set; } = false;

        [Parameter]
        public bool ShowExecutionDate { get; set; } = false;

        //[Parameter]
        private List<TestResultViewModel> TestResults { get; set; } = new List<TestResultViewModel>();

        private MudTable<TestResultViewModel> table;

        private async Task<TableData<TestResultViewModel>> GetTestResultsAsync(TableState state)
        {
            var result = await TableStateChanged.Invoke(state);
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
            //var item = table.Context.Rows.FirstOrDefault(t => t.Key.Id.Equals(rowId)).Key;
            var item = TestResults.FirstOrDefault(t => t.Id.Equals(rowId));
            item.IsErrorDetailsVisible = !item.IsErrorDetailsVisible;
            //var row = TestResults.FirstOrDefault(t => t.Id.Equals(rowId));
            //if (row != null)
            //{
            //    row.IsErrorDetailsVisible = !row.IsErrorDetailsVisible;
            //}
        }
    }
}
