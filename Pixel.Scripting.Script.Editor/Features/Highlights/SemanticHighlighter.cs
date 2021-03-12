using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Editor.Core.Models.Highlights;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Script.Editor.Features
{
    public class SemanticHighlighter : IHighlighter , ILineTracker
    {       
        private readonly string documentName;
        private readonly string projectName;
        private IClassificationHighlightColors classificationHighlightColors;
        private IEditorService editorService;
        private HighlightCache highLightCache;
        private TextView textView;
        Subject<IDocumentLine> highlightRequests = new Subject<IDocumentLine>();
     
        public IDocument Document { get; private set; }     

        public HighlightingColor DefaultTextColor { get; private set; }

        private readonly SynchronizationContext synchonizationContext;

        IDisposable bufferedSubscription;
        IDisposable intervalSubscription;
     
        public SemanticHighlighter(string documentName, string projectName, TextView textView, IDocument document, IEditorService editorService, IClassificationHighlightColors classificationHighlightColors)
        {
            this.textView = textView;
            this.documentName = documentName;
            this.projectName = projectName;
            this.Document = document;
            this.editorService = editorService;
            this.classificationHighlightColors = classificationHighlightColors;
            this.DefaultTextColor = classificationHighlightColors.DefaultBrush;
            this.highLightCache = new HighlightCache();
            synchonizationContext = SynchronizationContext.Current;

            Subscribe();
        }

        private void Subscribe()
        {
            var bufferedObservable = highlightRequests.Buffer<IDocumentLine>(TimeSpan.FromMilliseconds(400), 4).DistinctUntilChanged();
            bufferedSubscription = bufferedObservable.Subscribe(lines =>
            {
                foreach (var documentLine in lines)
                {
                    RequestHighlightAsync(documentLine);
                }
            });

            IObservable<long> intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(500)).ObserveOn(synchonizationContext);
            intervalSubscription = intervalObservable.Subscribe(itr =>
            {
                RequestHighlightAsync(Document.GetLineByNumber(textView.HighlightedLine));
            });
        }


        public void SuspendHighlight()
        {
            bufferedSubscription.Dispose();
            intervalSubscription.Dispose();
        }

        public void ResumeHighlight()
        {
            Subscribe();
        }

        public event HighlightingStateChangedEventHandler HighlightingStateChanged;
       
        public IEnumerable<HighlightingColor> GetColorStack(int lineNumber)
        {
            return null;
        }

        public HighlightingColor GetNamedColor(string name)
        {
            return classificationHighlightColors.GetBrush(name) ?? this.DefaultTextColor;
        }

        public HighlightedLine HighlightLine(int lineNumber)
        {            
            var documentLine = this.Document.GetLineByNumber(lineNumber);

            if (highLightCache.TryGetHighLightedLineFromCache(this.Document, documentLine, out CachedHighlightedLine cachedHighlightedLine))
            {
                if (cachedHighlightedLine.DocumentLine.Length == documentLine.Length)
                {
                    int offsetChange = (documentLine.Offset - cachedHighlightedLine.Offset) + (documentLine.EndOffset - cachedHighlightedLine.EndOffset);
                    var highlightedLine = cachedHighlightedLine.HighlightedLine;
                    for (int i = 0; i < highlightedLine.Sections.Count; i++)
                    {
                        var section = highlightedLine.Sections[i];
                        section.Offset += offsetChange;
                        section.Length = Math.Min(section.Length, documentLine.EndOffset - section.Offset);

                        if (section.Offset < documentLine.Offset || (section.Offset + section.Length) > documentLine.EndOffset)
                        {
                            highlightedLine.Sections.Remove(section as HighlightedSection);
                            i--;
                        }
                    }
                    if (offsetChange != 0)
                    {
                        highLightCache.RemoveLineFromCache(lineNumber);
                        highlightRequests.OnNext(documentLine);
                    }                  
                    return highlightedLine;
                }
                return cachedHighlightedLine.HighlightedLine;
            }
            highlightRequests.OnNext(documentLine);
            return new HighlightedLine(this.Document, documentLine);
        }

        private void RequestHighlightAsync(IDocumentLine documentLine)
        {
            if (documentLine == null || documentLine.IsDeleted)
                return;

            Task highlightLineTask = new Task(async () =>
            {
                int lineNumber = documentLine.LineNumber;               
                HighlightRequest highlightRequest = new HighlightRequest()
                {               
                    FileName = this.documentName,
                    ProjectName = this.projectName,
                    Lines = new int[] { lineNumber - 1 }
                };
                var response = await this.editorService.GetHighlightsAsync(highlightRequest);
                synchonizationContext.Post(p =>
                {
                    var highlightedLine = new HighlightedLine(this.Document, documentLine);
                    foreach (HighlightSpan highlight in response.Highlights)
                    {
                        int offsetStart = Document.GetOffset(highlight.StartLine + 1, highlight.StartColumn + 1);
                        int offsetEnd = Document.GetOffset(highlight.EndLine + 1, highlight.EndColumn + 1);
                        int length = Math.Min(documentLine.Length, offsetEnd - offsetStart);
                        highlightedLine.Sections.Add(new HighlightedSection
                        {
                            Color = classificationHighlightColors.GetBrush(highlight.Kind),
                            Offset = offsetStart,
                            Length = length
                        });
                    }
                    highLightCache.AddOrUpdateCache(new CachedHighlightedLine(documentLine, highlightedLine));
                    if (textView.GetVisualLine(lineNumber) != null)
                    {
                        try
                        {
                            HighlightingStateChanged?.Invoke(lineNumber, lineNumber);
                        }
                        catch
                        {
                        }
                    }
                }, null);
            });
            highlightLineTask.Start();
        }

        public void UpdateHighlightingState(int lineNumber)
        {
            
        }

        public void BeginHighlighting()
        {
           
        }       

        public void EndHighlighting()
        {
          
        }

        protected virtual void Dispose(bool isDisposing)
        {
            bufferedSubscription.Dispose();
            intervalSubscription.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #region ILineTracker

        public void BeforeRemoveLine(DocumentLine line)
        {  
        }

        public void SetLineLength(DocumentLine line, int newTotalLength)
        {
           
        }

        public void LineInserted(DocumentLine insertionPos, DocumentLine newLine)
        { 

        }

        public void RebuildDocument()
        {
            highLightCache.InvalidateCache();
        }

        public void ChangeComplete(DocumentChangeEventArgs e)
        {

        }

        #endregion ILineTracker
    }

  
}
