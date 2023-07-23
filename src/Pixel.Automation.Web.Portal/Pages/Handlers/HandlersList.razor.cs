using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Pixel.Automation.Web.Portal.Pages.Handlers;

public partial class HandlersList : ComponentBase
{
    [Inject]
    public IHandlerService HandlerService { get; set; }

    [Inject]
    public IDialogService DialogService { get; set; }

    [Inject]
    public ISnackbar SnackBar { get; set; }

    [Inject]
    public NavigationManager Navigator { get; set; }

    private readonly int[] pageSizeOptions = { 10, 20, 30, 40, 50 };
    private MudTable<TemplateHandler> handlersTable;
    private GetHandlersRequest handlersRequest = new();
    private bool resetCurrentPage = false;

    /// <summary>
    /// Get template handlers from api endpoint for the current page of the data table
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<TableData<TemplateHandler>> GetHandlersAsync(TableState state)
    {
        try
        {
            handlersRequest.CurrentPage = resetCurrentPage ? 1 : (state.Page + 1);
            resetCurrentPage = false;
            handlersRequest.PageSize = state.PageSize;
            var sessionPage = await HandlerService.GetHandlersAsync(handlersRequest);

            return new TableData<TemplateHandler>
            {
                Items = sessionPage.Items,
                TotalItems = sessionPage.ItemsCount
            };
        }
        catch (Exception ex)
        {
            SnackBar.Add($"Error while retrieving template handlers.{ex.Message}", Severity.Error);
        }
        return new TableData<TemplateHandler>
        {
            Items = Enumerable.Empty<TemplateHandler>(),
            TotalItems = 0
        };
    }

    /// <summary>
    /// Refresh data for the search query
    /// </summary>
    /// <param name="text"></param>
    private void OnSearch(string text)
    {
        handlersRequest.HandlerFilter = string.Empty;
        if (!string.IsNullOrEmpty(text))
        {
            handlersRequest.HandlerFilter = text;
        }
        resetCurrentPage = true;
        handlersTable.ReloadServerData();
    }

    /// <summary>
    /// Navigate to add new template handler page
    /// </summary>
    void AddNewHandler()
    {
        Navigator.NavigateTo($"handlers/new");
    }

    /// <summary>
    /// Navigate to edit template handler page
    /// </summary>
    /// <param name="templateHandler"></param>
    void EditHandler(TemplateHandler templateHandler)
    {
        Navigator.NavigateTo($"handlers/edit/{templateHandler.Id}");
    }

    /// <summary>
    /// Delete the template handler
    /// </summary>
    /// <param name="templateHandler"></param>
    /// <returns></returns>
    async Task DeleteHandlerAsync(TemplateHandler templateHandler)
    {
        bool? dialogResult = await DialogService.ShowMessageBox("Warning", "Delete can't be undone !!",
            yesText: "Delete!", cancelText: "Cancel", options: new DialogOptions() { FullWidth = true });
        if (dialogResult.GetValueOrDefault())
        {
            var result = await HandlerService.DeleteHandlerAsync(templateHandler);
            if (result.IsSuccess)
            {
                SnackBar.Add("Deleted successfully.", Severity.Success);
                await handlersTable.ReloadServerData();
                return;
            }
            SnackBar.Add(result.ToString(), Severity.Error, config =>
            {
                config.ShowCloseIcon = true;
                config.RequireInteraction = true;
            });
        }
    }

    string GetParameterString(TemplateHandler handler)
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        foreach (var argument in handler.Parameters)
        {
            sb.Append(argument.Key);
            sb.Append("=");
            sb.Append(argument.Value);
            i++;
            if (i < handler.Parameters.Count)
            {
                sb.Append(',');
            }
        }
        return sb.ToString();
    }
}
