//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Completion;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Extensions;
//using Microsoft.CodeAnalysis.Options;
//using Microsoft.CodeAnalysis.Scripting;
//using Microsoft.CodeAnalysis.Text;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.IO;
//using System.Threading.Tasks;

//namespace Pixel.Scripting.Editor.Services.Completion.Providers
//{
//    [ExportCompletionProvider("DllReferenceProvider", LanguageNames.CSharp)]
//    public class DllReferenceProvider : CompletionProvider
//    {
//        List<CompletionItem> completionItems = default;
//        private static readonly CompletionItemRules completionRules = CompletionItemRules.Create(
//            filterCharacterRules: ImmutableArray<CharacterSetModificationRule>.Empty,
//            commitCharacterRules: ImmutableArray<CharacterSetModificationRule>.Empty,
//            enterKeyRule: EnterKeyRule.Never,
//            selectionBehavior: CompletionItemSelectionBehavior.SoftSelection);
//        public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
//         {
            
//            return true;
//        }

//        public async override Task ProvideCompletionsAsync(CompletionContext context)
//        {
//            var document = context.Document;
//            var position = context.Position;
//            var cancellationToken = context.CancellationToken;
//            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
//            //if (!tree.IsEntirelyWithinStringLiteral(position, cancellationToken))
//            //{
//            //    return;
//            //}
//            var root = await tree.GetRootAsync(cancellationToken);
//            var tokenAtContextPosition = root.FindToken(position, findInsideTrivia: true);
//            if (tree.IsEntirelyWithinStringLiteral(position, cancellationToken))
//            {
//                var token = tree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);
//                if (token.Kind() == SyntaxKind.EndOfDirectiveToken || token.Kind() == SyntaxKind.EndOfFileToken)
//                {
//                    token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
//                }

//                if (token.Kind() == SyntaxKind.StringLiteralToken && token.Parent.Kind() == SyntaxKind.ReferenceDirectiveTrivia)
//                {
//                    if (completionItems == null)
//                    {
//                        completionItems = new List<CompletionItem>();
//                        await InitializeReferenceDlls(document);
//                    }
//                    context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
//                    context.AddItems(this.completionItems);
//                    await Task.CompletedTask;
//                }
//            }

//            //if (tokenAtContextPosition.HasLeadingTrivia)
//            //{

//            //    var leadingTrivia = tokenAtContextPosition.LeadingTrivia.Last();
//            //    if(leadingTrivia.Kind().Equals(SyntaxKind.ReferenceDirectiveTrivia) && leadingTrivia.ToString().Equals("#r \""))
//            //    {
//            //        if (completionItems == null)
//            //        {
//            //            completionItems = new List<CompletionItem>();
//            //            await InitializeReferenceDlls(document);
//            //        }
//            //        context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
//            //        context.AddItems(this.completionItems);
//            //        await Task.CompletedTask;
//            //    }         

//            //}           
//            //if (tokenAtContextPosition.Parent != null)
//            //{
               
//            //    if (tokenAtContextPosition.Parent.Kind().Equals(SyntaxKind.ReferenceDirectiveTrivia) && tokenAtContextPosition.Parent.ToString().Equals("#r \""))
//            //    {
//            //        if (completionItems == null)
//            //        {
//            //            completionItems = new List<CompletionItem>();
//            //            await InitializeReferenceDlls(document);
//            //        }
//            //        context.CompletionListSpan = new TextSpan(tokenAtContextPosition.SpanStart, position - tokenAtContextPosition.SpanStart);
//            //        context.AddItems(this.completionItems);
//            //        await Task.CompletedTask;
//            //    }

//            //}

//            return;         
//        }
       
//        private async Task InitializeReferenceDlls(Document document)
//        {
//            await Task.Run(() =>
//            {              
//                var scriptEnvironmentService = document.Project.Solution.Workspace.Services.GetService<IScriptEnvironmentService>();

//                foreach (var directory in scriptEnvironmentService.MetadataReferenceSearchPaths)
//                {
//                    var dlls = Directory.EnumerateFiles(directory, "*.dll");
//                    foreach (var dll in dlls)
//                    {
//                        var completionItem = CompletionItem.Create(Path.GetFileName(dll));
//                        //Hack : DisplayTextSuffix string is used for filtering with wordToComplete in IntelliSenseService
//                        completionItem = completionItem.WithDisplayTextSuffix("");                      
//                        completionItem = completionItem.WithRules(completionRules);
//                        this.completionItems.Add(completionItem);
//                    }
//                }
//            });
//        }
//    }
//}
