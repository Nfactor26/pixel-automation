using ICSharpCode.AvalonEdit.Document;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models.Diagnostics;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pixel.Scripting.Script.Editor.Features
{
    public class DiagnosticsManager : IDisposable
    {     
        private readonly TextDocument textDocument;
        private readonly TextMarkerService textMarkerService;
        private IObservable<DiagnosticResultEx> diagnostiResultObservervable;
        private IDisposable diagnoticResultObserver;
        private readonly Dispatcher dispatcher;

        public DiagnosticsManager(IEditorService editorService, CodeTextEditor textEditor, TextDocument textDocument, TextMarkerService textMarkerService)
        {          
            this.textDocument = textDocument;
            this.textMarkerService = textMarkerService;
            dispatcher = Dispatcher.CurrentDispatcher;

            string documentName = textEditor.FileName;
            string projectName = textEditor.ProjectName;
            diagnostiResultObservervable = editorService.DiagnosticsUpdated?.Where(r =>r.FileName.Equals(documentName) && r.ProjectName.Equals(projectName));
            diagnoticResultObserver = diagnostiResultObservervable?.Subscribe(OnDiagnosticsUpdated);

        }

        private void OnDiagnosticsUpdated(DiagnosticResultEx diagnosticResult)
        {
            this.dispatcher.InvokeAsync(() => ProcessDiagnostics(diagnosticResult));
        }

        private void ProcessDiagnostics(DiagnosticResultEx diagnosticResult)
        {
            textMarkerService.RemoveAll(textMarker => Equals(diagnosticResult.Id, textMarker.Tag));

            foreach (var quickFix in diagnosticResult.QuickFixes)
            {
                if (quickFix.LogLevel == "Hidden")
                {
                    continue;
                }

                var startOffset = textDocument.GetOffset(quickFix.Line + 1, quickFix.Column + 1);
                var endOffset = textDocument.GetOffset(quickFix.EndLine + 1, quickFix.EndColumn + 1);

                var marker = textMarkerService.TryCreate(startOffset, endOffset - startOffset);
                if (marker != null)
                {
                    marker.Tag = diagnosticResult.Id;
                    marker.MarkerColor = GetDiagnosticsColor(quickFix.LogLevel);
                    marker.ToolTip = quickFix.Text;
                }
            }

        }

        private static Color GetDiagnosticsColor(string logLevel)
        {
            switch (logLevel)
            {
                case "Info":
                    return Colors.LimeGreen;
                case "Warning":
                    return Colors.DodgerBlue;
                case "Error":
                    return Colors.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Dispose()
        {
            diagnoticResultObserver?.Dispose();
        }
    }
}
