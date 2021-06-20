using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages
{
    public partial class SessionsPage : ComponentBase
    {   
        [Inject]
        public ITestSessionService Service { get; set; }

        private TestSessionRequest sessionQueryParameters = new TestSessionRequest();
        private readonly int[] pageSizeOptions = { 10, 20, 30, 40, 50 };
        private MudTable<TestSession> sessionsTable;
        private bool resetCurrentPage = false;
      
        private async Task<TableData<TestSession>> GetSessionsData(TableState state)
        {
            sessionQueryParameters.CurrentPage = resetCurrentPage ? 1 :  (state.Page + 1);
            resetCurrentPage = false;
            sessionQueryParameters.PageSize = state.PageSize;
            var sessionPage = await Service.GetAllSessionsAsync(sessionQueryParameters);

            return new TableData<TestSession>
            {
                Items = sessionPage.Items,
                TotalItems = sessionPage.ItemsCount
            };
        }

        private void OnSearch(string text)
        {
            sessionQueryParameters.ProjectName = string.Empty;
            sessionQueryParameters.MachineName = string.Empty;
            sessionQueryParameters.TemplateName = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                string[] searchData = text.Split(":");
                if (searchData.Length != 2)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(searchData[0]) && !string.IsNullOrEmpty(searchData[1]))
                {                   
                    switch (searchData[0])
                    {
                        case "project":
                            sessionQueryParameters.ProjectName = searchData[1];
                            break;
                        case "machine":
                            sessionQueryParameters.MachineName = searchData[1];
                            break;
                        case "template":
                            sessionQueryParameters.TemplateName = searchData[1];
                            break;
                    }
                }
            }
            resetCurrentPage = true;
            sessionsTable.ReloadServerData();
        }
    }
}
