using Microsoft.AspNetCore.Components;
using Pixel.Automation.Web.Portal.Services;
using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Pages
{
    public partial class SessionDetailsPage : ComponentBase
    {
        [Inject]
        public ITestResultService TestResultService { get; set; }

        [Inject]
        public ITestSessionService TestSessionService { get; set; }

        [Parameter]
        public string SessionId { get; set; }

        private TestSessionViewModel testSessionVm;

        protected override async Task OnInitializedAsync()
        {
            var testSession = await TestSessionService.GetSessionByIdAsync(SessionId);
            var testsInSession = await TestResultService.GetResultsInSessionAsync(SessionId);
            testSessionVm = new TestSessionViewModel(testSession, testsInSession);
        }

        async Task<PagedList<TestResult>> GetTestResultsAsync(TestResultRequest testResultRequest)
        {
            testResultRequest.SessionId = SessionId;
            var result = await TestResultService.GetTestResultsAsync(testResultRequest);
            return result;
        }
    }
}
