using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using Pixel.Script.Editor.Services.CSharp.Helpers;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.Completions;
using Pixel.Scripting.Editor.Services.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.Completion
{
    internal class IntellisenseService 
    {       
        private readonly FormattingOptions formattingOptions;
        private readonly AdhocWorkspaceManager workspaceManager;
        private readonly IEnumerable<AutoCompleteResponse> emptyResponse = new List<AutoCompleteResponse>();

        public IntellisenseService(AdhocWorkspaceManager workspaceManager, FormattingOptions formattingOptions)
        {
            this.workspaceManager = workspaceManager;
            this.formattingOptions = formattingOptions;
        }

        public async Task<IEnumerable<AutoCompleteResponse>> GetCompletions(AutoCompleteRequest request)
        {
            if (!this.workspaceManager.IsDocumentOpen(request.FileName, request.ProjectName))
            {
                return emptyResponse;
            }

            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            var wordToComplete = request.WordToComplete;
            var completionTrigger = GetCompletionTrigger(request.TriggerCharacter);

            var completions = new HashSet<AutoCompleteResponse>();

            var sourceText = await document.GetTextAsync();
            var position = sourceText.Lines.GetPosition(new LinePosition(request.Line, request.Column));
            var service = CompletionService.GetService(document);
            var completionList = await service.GetCompletionsAsync(document, position, completionTrigger);

            if (completionList != null)
            {
                // Only trigger on space if Roslyn has object creation items
                if (request.TriggerCharacter == ' ' && !completionList.Items.Any(i => i.IsObjectCreationCompletionItem()))
                {
                    return completions;
                }

                // get recommended symbols to match them up later with SymbolCompletionProvider
                var semanticModel = await document.GetSemanticModelAsync();
                var recommendedSymbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, position, this.workspaceManager.GetWorkspace());

                var isSuggestionMode = completionList.SuggestionModeItem != null;
                foreach (var item in completionList.Items)
                {
                    var completionText = item.DisplayText;
                    var preselect = item.Rules.MatchPriority == MatchPriority.Preselect;
                    if (completionText.IsValidCompletionFor(wordToComplete) || item.DisplayTextSuffix.Equals(wordToComplete))
                    {
                        var symbols = await item.GetCompletionSymbolsAsync(recommendedSymbols, document);
                        if (symbols.Any())
                        {
                            foreach (var symbol in symbols)
                            {
                                if (item.UseDisplayTextAsCompletionText())
                                {
                                    completionText = item.DisplayText;
                                }
                                else if (item.TryGetInsertionText(out var insertionText))
                                {
                                    completionText = insertionText;
                                }
                                else
                                {
                                    completionText = symbol.Name;
                                }

                                if (symbol != null)
                                {
                                    if (request.WantSnippet)
                                    {
                                        foreach (var completion in MakeSnippetedResponses(request, symbol, completionText, preselect, isSuggestionMode))
                                        {
                                            completions.Add(completion);
                                        }
                                    }
                                    else
                                    {
                                        completions.Add(MakeAutoCompleteResponse(request, symbol, completionText, preselect, isSuggestionMode));
                                    }
                                }
                            }

                            // if we had any symbols from the completion, we can continue, otherwise it means
                            // the completion didn't have an associated symbol so we'll add it manually
                            continue;
                        }

                        // for other completions, i.e. keywords, create a simple AutoCompleteResponse
                        // we'll just assume that the completion text is the same
                        // as the display text.

                        var completionDescription = await service.GetDescriptionAsync(document, item);                     

                        var response = new AutoCompleteResponse()
                        {
                            CompletionText = item.DisplayText,
                            DisplayText = item.DisplayText,
                            Snippet = string.Empty,
                            Kind = request.WantKind ? item.Tags.FirstOrDefault() : null,
                            IsSuggestionMode = isSuggestionMode,
                            Preselect = preselect,                          
                            Glyph = item.GetGlyph(),
                            SymbolDisplayParts = GetSymbolDisplayPartsFromCompletionDescription(completionDescription)
                        };

                        completions.Add(response);                       
                    }
                }
            }
            return completions
                .OrderByDescending(c => c.CompletionText.IsValidCompletionStartsWithExactCase(wordToComplete))
                .ThenByDescending(c => c.CompletionText.IsValidCompletionStartsWithIgnoreCase(wordToComplete))
                .ThenByDescending(c => c.CompletionText.IsCamelCaseMatch(wordToComplete))
                .ThenByDescending(c => c.CompletionText.IsSubsequenceMatch(wordToComplete))
                .ThenBy(c => c.DisplayText, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompletionText, StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<AutoCompleteResponse> MakeSnippetedResponses(AutoCompleteRequest request, ISymbol symbol, string completionText, bool preselect, bool isSuggestionMode)
        {
            switch (symbol)
            {
                case IMethodSymbol methodSymbol:
                    return MakeSnippetedResponses(request, methodSymbol, completionText, preselect, isSuggestionMode);
                case INamedTypeSymbol typeSymbol:
                    return MakeSnippetedResponses(request, typeSymbol, completionText, preselect, isSuggestionMode);

                default:
                    return new[] { MakeAutoCompleteResponse(request, symbol, completionText, preselect, isSuggestionMode) };
            }
        }

        private IEnumerable<AutoCompleteResponse> MakeSnippetedResponses(AutoCompleteRequest request, IMethodSymbol methodSymbol, string completionText, bool preselect, bool isSuggestionMode)
        {
            var completions = new List<AutoCompleteResponse>();

            if (methodSymbol.Parameters.Any(p => p.IsOptional))
            {
                completions.Add(MakeAutoCompleteResponse(request, methodSymbol, completionText, preselect, isSuggestionMode, includeOptionalParams: false));
            }

            completions.Add(MakeAutoCompleteResponse(request, methodSymbol, completionText, preselect, isSuggestionMode));

            return completions;
        }

        private IEnumerable<AutoCompleteResponse> MakeSnippetedResponses(AutoCompleteRequest request, INamedTypeSymbol typeSymbol, string completionText, bool preselect, bool isSuggestionMode)
        {
            var completions = new List<AutoCompleteResponse>
            {
                MakeAutoCompleteResponse(request, typeSymbol, completionText, preselect, isSuggestionMode)
            };

            if (typeSymbol.TypeKind != TypeKind.Enum)
            {
                foreach (var ctor in typeSymbol.InstanceConstructors)
                {
                    completions.Add(MakeAutoCompleteResponse(request, ctor, completionText, preselect, isSuggestionMode));
                }
            }

            return completions;
        }

        private AutoCompleteResponse MakeAutoCompleteResponse(AutoCompleteRequest request, ISymbol symbol, string completionText, bool preselect, bool isSuggestionMode, bool includeOptionalParams = true)
        {
            var displayNameGenerator = new SnippetGenerator();
            displayNameGenerator.IncludeMarkers = false;
            displayNameGenerator.IncludeOptionalParameters = includeOptionalParams;

            var response = new AutoCompleteResponse();
            response.CompletionText = completionText;

            // TODO: Do something more intelligent here
            response.DisplayText = displayNameGenerator.Generate(symbol);

            response.IsSuggestionMode = isSuggestionMode;

            if (request.WantDocumentationForEveryCompletionResult)
            {
                response.Description = DocumentationConverter.ConvertDocumentation(symbol.GetDocumentationCommentXml(), formattingOptions.NewLine);
            }

            if (request.WantReturnType)
            {
                response.ReturnType = ReturnTypeFormatter.GetReturnType(symbol);
            }

            if (request.WantKind)
            {
                response.Kind = symbol.GetKind();
            }

            if (request.WantSnippet)
            {
                var snippetGenerator = new SnippetGenerator();
                snippetGenerator.IncludeMarkers = true;
                snippetGenerator.IncludeOptionalParameters = includeOptionalParams;
                response.Snippet = snippetGenerator.Generate(symbol);
            }

            if (request.WantMethodHeader)
            {
                response.MethodHeader = displayNameGenerator.Generate(symbol);
            }

            response.Preselect = preselect;

            return response;
        }

        private static CompletionTrigger GetCompletionTrigger(char triggerChar)
        {
            return triggerChar != default(char)
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar)
                : CompletionTrigger.Invoke;
        }

        private IEnumerable<Scripting.Editor.Core.Models.SymbolDisplayPart> GetSymbolDisplayPartsFromCompletionDescription(CompletionDescription completionDescription)
        {
            List<Scripting.Editor.Core.Models.SymbolDisplayPart> symbolDisplayParts = new List<Scripting.Editor.Core.Models.SymbolDisplayPart>();

            var taggedParts = completionDescription.TaggedParts;
            foreach(var taggedPart in taggedParts)
            {
                var symbolDisplayPart = new Scripting.Editor.Core.Models.SymbolDisplayPart(taggedPart.Text, taggedPart.ToSymbolDisplayPartKind());
                symbolDisplayParts.Add(symbolDisplayPart);
            }

            return symbolDisplayParts;
        }
    }
}
