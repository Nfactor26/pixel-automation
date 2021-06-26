using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.ViewModels;
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

        [Parameter]
        public List<TestResultViewModel> TestResults { get; set; }
      
        public void OnToggleErrorDetailsPanelVisibility(string rowId)
        {
            var row = TestResults.FirstOrDefault(t => t.Id.Equals(rowId));
            if (row != null)
            {
                row.IsErrorDetailsVisible = !row.IsErrorDetailsVisible;
            }
        }
    }
}
