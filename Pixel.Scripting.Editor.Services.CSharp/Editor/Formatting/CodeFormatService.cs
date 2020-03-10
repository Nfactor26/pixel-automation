using Microsoft.CodeAnalysis;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CSharp.Formatting
{
    internal class CodeFormatService
    {     
        private readonly AdhocWorkspaceManager workspaceManager;
      
        public CodeFormatService(AdhocWorkspaceManager workspaceManager)
        {
            this.workspaceManager = workspaceManager;           
        }

        public async Task<CodeFormatResponse> GetFormattedCodeAsync(CodeFormatRequest request)
        {
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return null;
            }

            if (request.WantsTextChanges)
            {
                var textChanges = await FormattingWorker.GetFormattedTextChanges(document);
                return new CodeFormatResponse()
                {
                    Changes = textChanges
                };
            }

            var newText = await FormattingWorker.GetFormattedText(document);

            return new CodeFormatResponse
            {
                Buffer = newText
            };
        }
    }
}
