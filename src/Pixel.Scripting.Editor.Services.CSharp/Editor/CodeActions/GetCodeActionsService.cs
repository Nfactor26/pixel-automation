using Pixel.Scripting.Common.CSharp.Diagnostics;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeActions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public class GetCodeActionsService : BaseCodeActionService<GetCodeActionsRequest, GetCodeActionsResponse>
    {
        private readonly GetCodeActionsResponse emptyResponse = new GetCodeActionsResponse() { CodeActions = new List<EditorCodeAction>() };

        public GetCodeActionsService(AdhocWorkspaceManager workspaceManager, IEnumerable<ICodeActionProvider> providers,
            IDiagnosticsService diagnosticService, CachingCodeFixProviderForProjects codeFixesForProject)
            : base(workspaceManager, providers, diagnosticService, codeFixesForProject)
        {
        }

        public async Task<GetCodeActionsResponse> GetCodeActionsAsync(GetCodeActionsRequest request)
        {
            if (!this.workspaceManager.IsDocumentOpen(request.FileName, request.ProjectName))
            {
                return emptyResponse;
            }
            var availableActions = await GetAvailableCodeActions(request);

            return new GetCodeActionsResponse
            {
                CodeActions = availableActions.Select(ConvertToCodeAction)
            };
        }      

        private static EditorCodeAction ConvertToCodeAction(AvailableCodeAction availableAction)
        {
            return new EditorCodeAction(availableAction.GetIdentifier(), availableAction.GetTitle());
        }
    }
}
