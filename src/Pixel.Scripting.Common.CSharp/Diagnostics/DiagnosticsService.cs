using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Pixel.Scripting.Common.CSharp.Diagnostics
{
    public interface IDiagnosticsService
    {
        public event EventHandler<DiagnosticsUpdatedArgs> DiagnosticsUpdated;
        Task<ImmutableArray<Diagnostic>> GetDiagnostics(Document document);
    }
      
    public class DiagnosticsService : IDiagnosticsService, IDisposable
    {
        private readonly Workspace workspace;
        private readonly IObserver<string> openDocuments;
        private readonly IDisposable disposable;

        public DiagnosticsService(Workspace workspace)
        {
            this.workspace = workspace;
            var openDocumentsSubject = new Subject<string>();
            openDocuments = openDocumentsSubject;

            workspace.WorkspaceChanged += OnWorkspaceChanged;
            
            disposable = openDocumentsSubject
                .Buffer(() => Observable.Amb(
                    openDocumentsSubject.Skip(99).Select(z => Unit.Default),
                    Observable.Timer(TimeSpan.FromMilliseconds(100)).Select(z => Unit.Default)
                ))
                .SubscribeOn(TaskPoolScheduler.Default)
                .Select(ProcessQueue)
                .Merge()
                .Subscribe();
        }

        private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs changeEvent)
        {
            if (changeEvent.Kind == WorkspaceChangeKind.DocumentAdded || changeEvent.Kind == WorkspaceChangeKind.DocumentChanged || changeEvent.Kind == WorkspaceChangeKind.DocumentReloaded)
            {
                var newDocument = changeEvent.NewSolution.GetDocument(changeEvent.DocumentId);
                EmitDiagnostics(new[] { newDocument.Id }.Union(workspace.GetOpenDocumentIds()).Select(x => workspace.CurrentSolution.GetDocument(x).FilePath).ToArray());
            }
            else if (changeEvent.Kind == WorkspaceChangeKind.ProjectAdded || changeEvent.Kind == WorkspaceChangeKind.ProjectReloaded)
            {
                EmitDiagnostics(changeEvent.NewSolution.GetProject(changeEvent.ProjectId).Documents.Select(x => x.FilePath).ToArray());
            }
        }

        private void EmitDiagnostics(params string[] documents)
        {           
            foreach (var document in documents)
            {
                openDocuments.OnNext(document);
            }
        }

        private IObservable<Unit> ProcessQueue(IEnumerable<string> filePaths)
        {
            return Observable.FromAsync(async () =>
            {
                var results = await Task.WhenAll(filePaths.Distinct().Select(ProcessNextItem));
                foreach(var result in results)
                {
                    OnDiagnosticsUpdated(result);
                }
            });
        }

        private async Task<DiagnosticsUpdatedArgs> ProcessNextItem(string filePath)
        {
            var currentSolution = workspace.CurrentSolution;
            var documents = currentSolution.GetDocumentIdsWithFilePath(filePath).Select(id => currentSolution.GetDocument(id)).OfType<Document>();
            var semanticModels = await Task.WhenAll(documents.Select(doc => doc.GetSemanticModelAsync()));
            var items = semanticModels.SelectMany(sm => sm.GetDiagnostics());
            return DiagnosticsUpdatedArgs.DiagnosticsCreated(documents.First().Id, workspace, workspace.CurrentSolution, documents.First().Project.Id, documents.First().Id, items);
        }

        public async Task<ImmutableArray<Diagnostic>> GetDiagnostics(Document document)
        {
            var semanticModel = await document.GetSemanticModelAsync();
            return semanticModel.GetDiagnostics();
        }


        public event EventHandler<DiagnosticsUpdatedArgs> DiagnosticsUpdated = delegate { };

        protected virtual void OnDiagnosticsUpdated(DiagnosticsUpdatedArgs diagnosticsUpdatedArgs)
        {
            this.DiagnosticsUpdated(this, diagnosticsUpdatedArgs);
        }

        public void Dispose()
        {
            workspace.WorkspaceChanged -= OnWorkspaceChanged;           
            disposable.Dispose();
        }
    }
}
