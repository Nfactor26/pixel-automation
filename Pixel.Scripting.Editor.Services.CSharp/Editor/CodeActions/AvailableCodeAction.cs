using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Scripting.Editor.Services.CodeActions
{
    public class AvailableCodeAction
    {
        public CodeAction CodeAction { get; }
        public CodeAction ParentCodeAction { get; }

        public AvailableCodeAction(CodeAction codeAction, CodeAction parentCodeAction = null)
        {
            this.CodeAction = codeAction ?? throw new ArgumentNullException(nameof(codeAction));
            this.ParentCodeAction = parentCodeAction;
        }

        public string GetIdentifier()
        {
            return CodeAction.EquivalenceKey ?? GetTitle();
        }

        public string GetTitle()
        {
            return ParentCodeAction != null
                ? $"{ParentCodeAction.Title} -> {CodeAction.Title}"
                : CodeAction.Title;
        }

        public Task<ImmutableArray<CodeActionOperation>> GetOperationsAsync(CancellationToken cancellationToken)
        {
            return CodeAction.GetOperationsAsync(cancellationToken);
        }
    }
}
