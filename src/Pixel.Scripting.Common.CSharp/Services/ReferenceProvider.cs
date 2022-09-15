using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Common.CSharp.Services
{    
    [ExportCompletionProvider(nameof(DllReferenceProvider), LanguageNames.CSharp)]
    public class DllReferenceProvider : CompletionProvider
    {
        List<CompletionItem> completionItems = default;
        private static readonly CompletionItemRules completionRules = CompletionItemRules.Create(
            filterCharacterRules: ImmutableArray<CharacterSetModificationRule>.Empty,
            commitCharacterRules: ImmutableArray<CharacterSetModificationRule>.Empty,
            enterKeyRule: EnterKeyRule.Never,
            selectionBehavior: CompletionItemSelectionBehavior.SoftSelection);
   
        public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
        {
            return true;
        }

        public async override Task ProvideCompletionsAsync(CompletionContext context)
        {
            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;
            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            //if (!tree.IsEntirelyWithinStringLiteral(position, cancellationToken))
            //{
            //    return;
            //}
            var root = await tree.GetRootAsync(cancellationToken);
            var tokenAtContextPosition = root.FindToken(position, findInsideTrivia: true);
            if (IsEntirelyWithinStringLiteral(tree, position, cancellationToken))
            {
                var token = tree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);
                if (token.IsKind(SyntaxKind.EndOfDirectiveToken) || token.IsKind(SyntaxKind.EndOfFileToken))
                {
                    token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
                }

                if (token.IsKind(SyntaxKind.StringLiteralToken) && token.Parent.IsKind(SyntaxKind.ReferenceDirectiveTrivia))
                {
                    if (completionItems == null)
                    {
                        completionItems = new List<CompletionItem>();
                        await InitializeReferenceDlls(document);
                    }
                    context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
                    context.AddItems(this.completionItems);
                    await Task.CompletedTask;
                }
            }

            //if (tokenAtContextPosition.HasLeadingTrivia)
            //{

            //    var leadingTrivia = tokenAtContextPosition.LeadingTrivia.Last();
            //    if(leadingTrivia.Kind().Equals(SyntaxKind.ReferenceDirectiveTrivia) && leadingTrivia.ToString().Equals("#r \""))
            //    {
            //        if (completionItems == null)
            //        {
            //            completionItems = new List<CompletionItem>();
            //            await InitializeReferenceDlls(document);
            //        }
            //        context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
            //        context.AddItems(this.completionItems);
            //        await Task.CompletedTask;
            //    }         

            //}           
            //if (tokenAtContextPosition.Parent != null)
            //{

            //    if (tokenAtContextPosition.Parent.Kind().Equals(SyntaxKind.ReferenceDirectiveTrivia) && tokenAtContextPosition.Parent.ToString().Equals("#r \""))
            //    {
            //        if (completionItems == null)
            //        {
            //            completionItems = new List<CompletionItem>();
            //            await InitializeReferenceDlls(document);
            //        }
            //        context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
            //        context.AddItems(this.completionItems);
            //        await Task.CompletedTask;
            //    }

            //}

            return;
        }

        private async Task InitializeReferenceDlls(Document document)
        {
            await Task.Run(() =>
            {                
                foreach (var directory in Enumerable.Concat(new[] { AppContext.BaseDirectory }, Directory.GetDirectories(Path.Join(AppContext.BaseDirectory, "Plugins"))))
                {
                    var dlls = Directory.EnumerateFiles(directory, "*.dll");
                    foreach (var dll in dlls)
                    {
                        var completionItem = CompletionItem.Create(Path.GetFileName(dll));
                        //Hack : DisplayTextSuffix string is used for filtering with wordToComplete in IntelliSenseService
                        completionItem = completionItem.WithDisplayTextSuffix("");
                        completionItem = completionItem.WithRules(completionRules);
                        this.completionItems.Add(completionItem);
                    }
                }
            });
        }

        bool IsEntirelyWithinStringLiteral(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var token = syntaxTree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);

            // If we ask right at the end of the file, we'll get back nothing. We handle that case
            // specially for now, though SyntaxTree.FindToken should work at the end of a file.
            if (token.IsKind(SyntaxKind.EndOfDirectiveToken) || token.IsKind(SyntaxKind.EndOfFileToken))
            {
                token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken) || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)) 
               // || token.IsKind(SyntaxKind.Utf8StringLiteralToken) || token.IsKind(SyntaxKind.Utf8SingleLineRawStringLiteralToken) || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken))                         
            {
                var span = token.Span;

                // cases:
                // "|"
                // "|  (e.g. incomplete string literal)
                return (position > span.Start && position < span.End)
                    || AtEndOfIncompleteStringOrCharLiteral(token, position, '"', cancellationToken);
            }

            if (token.IsKind(SyntaxKind.InterpolatedStringStartToken) || token.IsKind(SyntaxKind.InterpolatedStringTextToken) || token.IsKind(SyntaxKind.InterpolatedStringEndToken)
                || token.IsKind(SyntaxKind.InterpolatedRawStringEndToken) || token.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken)
                || token.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken))
            {
                return token.SpanStart < position && token.Span.End > position;
            }

            return false;
        }

        private bool AtEndOfIncompleteStringOrCharLiteral(SyntaxToken token, int position, char lastChar, CancellationToken cancellationToken)
        {
            if (!(token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.CharacterLiteralToken) ||
                token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken) || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)))
            {
                throw new ArgumentException("Expected string or char literal.", nameof(token));
            }

            if (position != token.Span.End)
                return false;

            if (token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken) || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
            {
                var sourceText = token.SyntaxTree!.GetText(cancellationToken);
                var startDelimeterLength = 0;
                var endDelimeterLength = 0;
                for (int i = token.SpanStart, n = token.Span.End; i < n; i++)
                {
                    if (sourceText[i] != '"')
                        break;

                    startDelimeterLength++;
                }

                for (int i = token.Span.End - 1, n = token.Span.Start; i >= n; i--)
                {
                    if (sourceText[i] != '"')
                        break;

                    endDelimeterLength++;
                }

                return token.Span.Length == startDelimeterLength ||
                    (token.Span.Length > startDelimeterLength && endDelimeterLength < startDelimeterLength);
            }
            else
            {
                var startDelimeterLength = token.IsVerbatimStringLiteral() ? 2 : 1;
                return token.Span.Length == startDelimeterLength ||
                    (token.Span.Length > startDelimeterLength && token.Text[^1] != lastChar);
            }
        }
    }
}
