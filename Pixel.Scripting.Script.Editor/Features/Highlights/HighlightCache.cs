using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Scripting.Script.Editor.Features
{
    internal class HighlightCache
    {
        List<CachedHighlightedLine> cachedLines = new List<CachedHighlightedLine>();
        private readonly object _locker = new object();

        public void AddOrUpdateCache(CachedHighlightedLine cachedLine)
        {           
            lock(_locker)
            {
                cachedLines.RemoveAll(a => a.DocumentLine.IsDeleted);
                var existingLine = cachedLines.FirstOrDefault(a => a.LineNumber == cachedLine.LineNumber);
                if(existingLine == null)
                {
                    cachedLines.Add(cachedLine);
                    return;
                }
                else
                {
                    cachedLines.Remove(existingLine);
                    cachedLines.Add(cachedLine);
                    return;
                }                
            }           
        }

        public void RemoveLineFromCache(int lineNumber)
        {
            //cachedLines.RemoveAll(a => a.DocumentLine.IsDeleted);
            var existingLine = cachedLines.FirstOrDefault(a => a.LineNumber == lineNumber);
            if (existingLine != null)
            {
                cachedLines.Remove(existingLine);
                //Debug.WriteLine($"Removed line number : {lineNumber} from cache");
            }
          
        }

        private void RemoveLineFromCache(CachedHighlightedLine cachedHighlightLine)
        {
            cachedLines.Remove(cachedHighlightLine);
        }
        
        public void InvalidateCache()
        {
            cachedLines.Clear();
        }

        public bool TryGetHighLightedLineFromCache(IDocument document ,IDocumentLine documentLine, out CachedHighlightedLine cachedHighlightedLine)
        {
            lock(_locker)
            {
                cachedLines.RemoveAll(a => a.DocumentLine.IsDeleted);
                cachedHighlightedLine = cachedLines.FirstOrDefault(a => a.LineNumber == documentLine.LineNumber);
                var highlightedLine = cachedHighlightedLine?.HighlightedLine;
                bool isValid = highlightedLine != null && !highlightedLine.DocumentLine.IsDeleted;
                //&& highlightedLine.Document.Version.BelongsToSameDocumentAs(document.Version) &&
                //highlightedLine.Document.Version.CompareAge(document.Version) == 0;
                if (!isValid && highlightedLine != null)
                {
                    RemoveLineFromCache(cachedHighlightedLine);
                    cachedHighlightedLine = null;
                    return false;
                }
                return isValid;
            }            
        }

    }
}
