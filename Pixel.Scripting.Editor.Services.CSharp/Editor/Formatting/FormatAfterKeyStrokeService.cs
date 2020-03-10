using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CSharp.Formatting
{
    internal class FormatAfterKeystrokeService
    {
        private readonly AdhocWorkspaceManager workspaceManager;
            
        public FormatAfterKeystrokeService(AdhocWorkspaceManager workspaceManager)
        {
            this.workspaceManager = workspaceManager;
        }

        public async Task<FormatRangeResponse> GetFormattedCodeAfterKeystrokeAsync(FormatAfterKeystrokeRequest request)
        {
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return null;
            }

            var text = await document.GetTextAsync();
            int position = text.Lines.GetPosition(new LinePosition(request.Line, request.Column));
            var changes = await FormattingWorker.GetFormattingChangesAfterKeystroke(document, position, request.Char);

            return new FormatRangeResponse()
            {
                Changes = changes
            };
        }
    }
}
