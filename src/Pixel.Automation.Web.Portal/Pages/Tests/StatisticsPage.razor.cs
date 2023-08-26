using Microsoft.AspNetCore.Components;
using MudBlazor;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages.Tests
{
    public partial class StatisticsPage : ComponentBase
    {      
        [Parameter]
        public string TestId { get; set; }     
    }
}
