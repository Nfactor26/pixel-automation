using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Pixel.Script.Editor.Services.CSharp.Helpers;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.TypeLookup;
using Pixel.Scripting.Editor.Services.CSharp.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CSharp.TypeLookup
{
    public class TypeLookupService
    {
        private readonly AdhocWorkspaceManager workspaceManager;       
        private readonly TypeLookupResponse emptyResponse = new TypeLookupResponse();
      
        private static readonly SymbolDisplayFormat DefaultFormat = SymbolDisplayFormat.FullyQualifiedFormat.
            WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted).
            WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.None).
            WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

        // default from symbol.ToMinimalDisplayString + IncludeConstantValue
        private static readonly SymbolDisplayFormat MinimalFormat = SymbolDisplayFormat.MinimallyQualifiedFormat.WithMemberOptions(
            SymbolDisplayMemberOptions.IncludeParameters |
            SymbolDisplayMemberOptions.IncludeType |
            SymbolDisplayMemberOptions.IncludeRef |
            SymbolDisplayMemberOptions.IncludeContainingType |
            SymbolDisplayMemberOptions.IncludeConstantValue
         );

        public TypeLookupService(AdhocWorkspaceManager workspaceManager)
        {
            this.workspaceManager = workspaceManager;            
        }

        public async Task<TypeLookupResponse> GetTypeDescriptionAsync(TypeLookupRequest request)
        {
            if (!this.workspaceManager.IsDocumentOpen(request.FileName, request.ProjectName))
            {
                return emptyResponse;
            }

            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            var response = new TypeLookupResponse();
            if (document != null)
            {
                var semanticModel = await document.GetSemanticModelAsync();
                var sourceText = await document.GetTextAsync();
                var position = sourceText.Lines.GetPosition(new LinePosition(request.Line, request.Column));
                var symbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspaceManager.GetWorkspace());
                if (symbol != null)
                {
                    response.Type = symbol.Kind == SymbolKind.NamedType ?
                        symbol.ToDisplayString(DefaultFormat) :
                        symbol.ToMinimalDisplayString(semanticModel, position, MinimalFormat);

                    response.Glyph = symbol.GetGlyph();

                    //Symbol Display parts provide information about part kind which can be used to render the part with a different color / font /etc.
                    var symbolDisplayParts = symbol.ToDisplayParts();
                    List<Core.Models.SymbolDisplayPart> displayParts = new List<Core.Models.SymbolDisplayPart>();
                    foreach (var symbolDisplayPart in symbolDisplayParts)
                    {
                        displayParts.Add(new Core.Models.SymbolDisplayPart(symbolDisplayPart.ToString(), (Core.Models.SymbolDisplayPartKind)(int)symbolDisplayPart.Kind));
                    }
                    response.SymbolDisplayParts = displayParts;

                    if (request.IncludeDocumentation)
                    {
                        //response.Documentation = DocumentationConverter.GetStructuredDocumentation(symbol.GetDocumentationCommentXml(), formattingOptions.NewLine);
                        response.StructuredDocumentation = DocumentationConverter.GetStructuredDocumentation(symbol);
                        response.Documentation = response.StructuredDocumentation.SummaryText;                      
                    }
                }
            }

            return response;
        }
    }
}
