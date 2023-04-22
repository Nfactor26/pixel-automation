using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;

namespace Pixel.Automation.Web.Portal.Pages.Template
{
    public partial class TemplateList : ComponentBase
    {
        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar SnackBar { get; set; }

        [Inject]
        public NavigationManager Navigator { get; set; }

        private readonly int[] pageSizeOptions = { 10, 20, 30, 40, 50 };
        private MudTable<SessionTemplate> templatesTable;
        private GetTemplatesRequest templatesRequest = new ();
        private bool resetCurrentPage = false;

        /// <summary>
        /// Get templates from api endpoint for the current page of the data table
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task<TableData<SessionTemplate>> GetTemplatesAsync(TableState state)
        {
            try
            {
                templatesRequest.CurrentPage = resetCurrentPage ? 1 : (state.Page + 1);
                resetCurrentPage = false;
                templatesRequest.PageSize = state.PageSize;
                var sessionPage = await TemplateService.GetTemplatesAsync(templatesRequest);

                return new TableData<SessionTemplate>
                {
                    Items = sessionPage.Items,
                    TotalItems = sessionPage.ItemsCount
                };
            }
            catch (Exception ex)
            {
                SnackBar.Add($"Error while retrieving templates.{ex.Message}", Severity.Error);
            }
            return new TableData<SessionTemplate>
            {
                Items = Enumerable.Empty<SessionTemplate>(),
                TotalItems = 0
            };
        }

        /// <summary>
        /// Refresh data for the search query
        /// </summary>
        /// <param name="text"></param>
        private void OnSearch(string text)
        {
            templatesRequest.TemplateFilter = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                templatesRequest.TemplateFilter = text;
            }
            resetCurrentPage = true;
            templatesTable.ReloadServerData();
        }

        /// <summary>
        /// Navigate to add new session template page
        /// </summary>
        void AddNewTemplate()
        {
            Navigator.NavigateTo($"templates/new");
        }

        /// <summary>
        /// Navigate to edit application page
        /// </summary>
        /// <param name="sessionTemplate"></param>
        void EditTemplate(SessionTemplate sessionTemplate)
        {
            Navigator.NavigateTo($"templates/edit/{sessionTemplate.Id}");
        }

        /// <summary>
        /// Delete the template
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        async Task DeleteTemplatesAsync(SessionTemplate sessionTemplate)
        {
            bool? dialogResult = await DialogService.ShowMessageBox("Warning", "Delete can't be undone !!",
                yesText: "Delete!", cancelText: "Cancel", options: new DialogOptions() { FullWidth = true });
            if (dialogResult.GetValueOrDefault())
            {
                var result = await TemplateService.DeleteTemplateAsync(sessionTemplate);
                if (result.IsSuccess)
                {
                    SnackBar.Add("Deleted successfully.", Severity.Success);
                    await templatesTable.ReloadServerData();
                    return;
                }
                SnackBar.Add(result.ToString(), Severity.Error, config =>
                {
                    config.ShowCloseIcon = true;
                    config.RequireInteraction = true;
                });
            }
        }
    }
}
