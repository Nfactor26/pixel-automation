using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Pixel.Script.Editor.Services.CSharp.Helpers;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeActions;
using Pixel.Scripting.Editor.Services.CSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public abstract class BaseCodeActionService<TRequest, TResponse>
    {
        protected readonly AdhocWorkspaceManager workspaceManager;
        protected readonly IEnumerable<ICodeActionProvider> Providers;      
        private readonly IDiagnosticService diagnosticService;
        private readonly CachingCodeFixProviderForProjects codeFixesForProject;
        private readonly MethodInfo _getNestedCodeActions;

        protected Lazy<List<CodeRefactoringProvider>> OrderedCodeRefactoringProviders;

        // CS8019 isn't directly used (via roslyn) but has an analyzer that report different diagnostic based on CS8019 to improve user experience.
        private readonly Dictionary<string, string> customDiagVsFixMap = new Dictionary<string, string>
        {
            { "CS8019", "RemoveUnnecessaryImportsFixable" }
        };

        protected BaseCodeActionService(AdhocWorkspaceManager workspaceManager, IEnumerable<ICodeActionProvider> providers,
            IDiagnosticService diagnosticService,
            CachingCodeFixProviderForProjects codeFixesForProject)
        {
            this.workspaceManager = workspaceManager;
            this.Providers = providers;
            this.diagnosticService = diagnosticService;
            this.codeFixesForProject = codeFixesForProject;
            OrderedCodeRefactoringProviders = new Lazy<List<CodeRefactoringProvider>>(() => GetSortedCodeRefactoringProviders());

            // Sadly, the CodeAction.NestedCodeActions property is still internal.
            var nestedCodeActionsProperty = typeof(CodeAction).GetProperty("NestedCodeActions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (nestedCodeActionsProperty == null)
            {
                throw new InvalidOperationException("Could not find CodeAction.NestedCodeActions property.");
            }

            this._getNestedCodeActions = nestedCodeActionsProperty.GetGetMethod(nonPublic: true);

            if (this._getNestedCodeActions == null)
            {
                throw new InvalidOperationException("Could not retrieve 'get' method for CodeAction.NestedCodeActions property.");
            }
        }

        protected async Task<IEnumerable<AvailableCodeAction>> GetAvailableCodeActions(ICodeActionRequest request)
        {
            // To produce a complete list of code actions for the document wait until all projects are loaded.
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return Array.Empty<AvailableCodeAction>();
            }

            var codeActions = new List<CodeAction>();

            var sourceText = await document.GetTextAsync();
            var span = GetTextSpan(request, sourceText);

            await CollectCodeFixesActions(document, span, codeActions);
            await CollectRefactoringActions(document, span, codeActions);

            var distinctActions = codeActions.GroupBy(x => x.Title).Select(x => x.First());

            // Be sure to filter out any code actions that inherit from CodeActionWithOptions.
            // This isn't a great solution and might need changing later, but every Roslyn code action
            // derived from this type tries to display a dialog. For now, this is a reasonable solution.
            var availableActions = ConvertToAvailableCodeAction(distinctActions)
                .Where(a => !a.CodeAction.GetType().GetTypeInfo().IsSubclassOf(typeof(CodeActionWithOptions)));

            return availableActions;
        }

        private TextSpan GetTextSpan(ICodeActionRequest request, SourceText sourceText)
        {
            if (request.Selection != null)
            {
                return sourceText.GetSpanFromRange(request.Selection);
            }

            var position = sourceText.GetPositionFromLineAndOffset(request.Line, request.Column);
            return new TextSpan(position, length: 0);
        }

        private async Task CollectCodeFixesActions(Document document, TextSpan span, List<CodeAction> codeActions)
        {
            var diagnosticDatas = this.diagnosticService.GetDiagnostics(document.Project.Solution.Workspace,
                document.Project.Id, document.Id, null, false, CancellationToken.None);

            var groupedBySpan = diagnosticDatas
                    .Select(x => x.ToDiagnosticAsync(document.Project,CancellationToken.None).Result)
                    .Where(diagnostic => span.IntersectsWith(diagnostic.Location.SourceSpan))
                    .GroupBy(diagnostic => diagnostic.Location.SourceSpan);

            foreach (var diagnosticGroupedBySpan in groupedBySpan)
            {
                var diagnosticSpan = diagnosticGroupedBySpan.Key;
                var diagnosticsWithSameSpan = diagnosticGroupedBySpan.OrderByDescending(d => d.Severity);

                await AppendFixesAsync(document, diagnosticSpan, diagnosticsWithSameSpan, codeActions);
            }
        }

        private async Task AppendFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, List<CodeAction> codeActions)
        {
            foreach (var codeFixProvider in GetSortedCodeFixProviders(document))
            {
                var fixableDiagnostics = diagnostics.Where(d => HasFix(codeFixProvider, d.Id)).ToImmutableArray();

                if (fixableDiagnostics.Length > 0)
                {
                    var context = new CodeFixContext(document, span, fixableDiagnostics, (a, _) => codeActions.Add(a), CancellationToken.None);

                    try
                    {
                        await codeFixProvider.RegisterCodeFixesAsync(context);
                    }
                    catch (Exception ex)
                    {
                        //this.Logger.LogError(ex, $"Error registering code fixes for {codeFixProvider.GetType().FullName}");
                    }
                }
            }
        }

        private List<CodeFixProvider> GetSortedCodeFixProviders(Document document)
        {
            var providerList =
                this.Providers.SelectMany(provider => provider.CodeFixProviders)
                    .Concat(codeFixesForProject.GetAllCodeFixesForProject(document.Project.Id));

            return ExtensionOrderer.GetOrderedOrUnorderedList<CodeFixProvider, ExportCodeFixProviderAttribute>(providerList, attribute => attribute.Name).ToList();
        }

        private List<CodeRefactoringProvider> GetSortedCodeRefactoringProviders()
        {
            var providerList = this.Providers.SelectMany(provider => provider.CodeRefactoringProviders);
            return ExtensionOrderer.GetOrderedOrUnorderedList<CodeRefactoringProvider, ExportCodeFixProviderAttribute>(providerList, attribute => attribute.Name).ToList();
        }

        private bool HasFix(CodeFixProvider codeFixProvider, string diagnosticId)
        {
            return codeFixProvider.FixableDiagnosticIds.Any(id => id == diagnosticId)
                || (customDiagVsFixMap.ContainsKey(diagnosticId) && codeFixProvider.FixableDiagnosticIds.Any(id => id == customDiagVsFixMap[diagnosticId]));
        }

        private async Task CollectRefactoringActions(Document document, TextSpan span, List<CodeAction> codeActions)
        {
            var availableRefactorings = OrderedCodeRefactoringProviders.Value;

            foreach (var codeRefactoringProvider in availableRefactorings)
            {
                try
                {
                    var context = new CodeRefactoringContext(document, span, a => codeActions.Add(a), CancellationToken.None);
                    await codeRefactoringProvider.ComputeRefactoringsAsync(context);
                }
                catch (Exception ex)
                {
                    //this.Logger.LogError(ex, $"Error computing refactorings for {codeRefactoringProvider.GetType().FullName}");
                }
            }
        }

        private IEnumerable<AvailableCodeAction> ConvertToAvailableCodeAction(IEnumerable<CodeAction> actions)
        {
            return actions.SelectMany(action =>
            {
                var nestedActions = this._getNestedCodeActions.Invoke<ImmutableArray<CodeAction>>(action, null);

                if (nestedActions.Any())
                {
                    return nestedActions.Select(nestedAction => new AvailableCodeAction(nestedAction, action));
                }

                return new[] { new AvailableCodeAction(action) };
            });
        }
    }
}
