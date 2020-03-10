using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CSharp.Formatting
{
    internal class FormatRangeService
    {
        private readonly AdhocWorkspaceManager workspaceManager;

        public FormatRangeService(AdhocWorkspaceManager workspaceManager)
        {
            this.workspaceManager = workspaceManager;
        }

        public async Task<FormatRangeResponse> GetFormattedRangeAsync(FormatRangeRequest request)
        {
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return null;
            }

            var text = await document.GetTextAsync();
            var start = text.Lines.GetPosition(new LinePosition(request.Line, request.Column));
            var end = text.Lines.GetPosition(new LinePosition(request.EndLine, request.EndColumn));
            var syntaxTree = await document.GetSyntaxRootAsync();
            var tokenStart = syntaxTree.FindToken(start).FullSpan.Start;
            var changes = await FormattingWorker.GetFormattingChanges(document, tokenStart, end);

            return new FormatRangeResponse()
            {
                Changes = changes
            };
        }
    }
}
