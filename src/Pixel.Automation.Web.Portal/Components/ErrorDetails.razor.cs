using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Components
{
    public partial class ErrorDetails : ComponentBase
    {
        [Parameter]
        public FailureDetailsViewModel FailureDetails { get; set; }

        [Parameter]
        public bool IsFailureReasonEditable { get; set; }

        [Inject]
        public ITestResultService Service { get; set; }

        private string reasonBeforeEdit;
        private bool isEditingReason = false;

        void ToggleEdit()
        {
            reasonBeforeEdit = FailureDetails.FailureReason;
            isEditingReason = true;
        }

        void CancelEdit()
        {
            FailureDetails.FailureReason = reasonBeforeEdit;
            isEditingReason = false;
        }

        async Task SaveEditAsync()
        {
            await Service.UpdateFailureReasonAsync(FailureDetails.SessionId, FailureDetails.TestId , FailureDetails.FailureReason);
            reasonBeforeEdit = string.Empty;
            isEditingReason = false;
        }
    }
}
