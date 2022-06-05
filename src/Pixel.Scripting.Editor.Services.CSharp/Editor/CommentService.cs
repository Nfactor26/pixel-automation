using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Models.CodeFormat;
using Pixel.Scripting.Editor.Services.CSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CSharp
{
    internal class CommentService
    {
        private readonly string singleLineCommentLiteral = "//";
        //private readonly string multiLineCommentStartLiteral = "/*";
        //private readonly string multiLineCommentEndLiteral = "*/";

        private readonly AdhocWorkspaceManager workspaceManager;

        public CommentService(AdhocWorkspaceManager workspaceManager)
        {
            this.workspaceManager = workspaceManager;
        }

        public async Task<FormatRangeResponse> CommentSelectionAsync(FormatRangeRequest request)
        {
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return null;
            }

            var text = await document.GetTextAsync();
            var start = text.Lines.GetPosition(new LinePosition(request.Line, request.Column));
            var end = text.Lines.GetPosition(new LinePosition(request.EndLine, request.EndColumn));
            var selectedRange = new TextSpan(start, end);

            var changes = new List<TextChange>();
            var lines = text.Lines.SkipWhile(x => !x.Span.IntersectsWith(selectedRange))
               .TakeWhile(x => x.Span.IntersectsWith(selectedRange)).ToArray();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(text.GetSubText(line.Span).ToString()))
                {
                    changes.Add(new TextChange(new TextSpan(line.Start, 0), singleLineCommentLiteral));
                }
            }

            return new FormatRangeResponse()
            {
                Changes = TextChanges.Convert(text,changes)
            };
        }

        public async Task<FormatRangeResponse> UncommentSelectionAsync(FormatRangeRequest request)
        {
            this.workspaceManager.TryGetDocument(request.FileName, out Document document);
            if (document == null)
            {
                return null;
            }
            var text = await document.GetTextAsync();
            var start = text.Lines.GetPosition(new LinePosition(request.Line, request.Column));
            var end = text.Lines.GetPosition(new LinePosition(request.EndLine, request.EndColumn));
            var selectedRange = new TextSpan(start, end);

            var changes = new List<TextChange>();
            var lines = text.Lines.SkipWhile(x => !x.Span.IntersectsWith(selectedRange))
               .TakeWhile(x => x.Span.IntersectsWith(selectedRange)).ToArray();

            foreach (var line in lines)
            {
                var lineText = text.GetSubText(line.Span).ToString();
                if (lineText.TrimStart().StartsWith(singleLineCommentLiteral, StringComparison.Ordinal))
                {
                    changes.Add(new TextChange(new TextSpan(
                        line.Start + lineText.IndexOf(singleLineCommentLiteral, StringComparison.Ordinal),
                        singleLineCommentLiteral.Length), string.Empty));
                }
            }

            return new FormatRangeResponse()
            {
                Changes = TextChanges.Convert(text, changes)
            };
        }
    }
}
