using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Scripting.Common.CSharp.Services;

[ExportCompletionProvider(nameof(LoadDirectiveCompletionProvider), LanguageNames.CSharp)]
public class LoadDirectiveCompletionProvider : CompletionProvider
{
    private static readonly CompletionItemRules s_rules = CompletionItemRules.Create(
         filterCharacterRules: [],
         commitCharacterRules: [CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, GetCommitCharacters())],
         enterKeyRule: EnterKeyRule.Never,
         selectionBehavior: CompletionItemSelectionBehavior.HardSelection);

    private static ImmutableArray<char> GetCommitCharacters()
    {
        return ['"', '/', '\\'];
    }


    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
    {
        if (trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character == '"' && caretPosition > 1 && text[caretPosition - 1] == ' ')
        {
            return true;
        }
        return false;
    }

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        // We only want to check the trigger behavior now
        var document = context.Document;
        var cancellationToken = context.CancellationToken;

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        // Get the line
        //TODO : This will cause allocation, optimize it by referring the DllreferenceCompletionProvider
        var line = text.Lines.GetLineFromPosition(context.Position);
        var lineText = line.ToString();

        // Check if the line begins with "#load"
        // (ignoring whitespace)
        if (!lineText.TrimStart().StartsWith("#load", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (document.Project.CompilationOptions?.MetadataReferenceResolver is CachedScriptMetadataResolver resolver)
        {
            string baseDir = resolver.BaseDirectory;
            if (Directory.Exists(Path.Combine(baseDir, "Scripts")))
            {              
                foreach (var scriptFile in Directory.GetFiles(Path.Combine(baseDir, "Scripts"), "*.csx", enumerationOptions: new EnumerationOptions() { RecurseSubdirectories = true }))
                {
                    var item = CompletionItem.Create(displayText: Path.GetFileName(scriptFile), rules: s_rules);
                    context.AddItem(item);
                }
            }         

            foreach (var path in resolver.SearchPaths)
            {
                foreach (var scriptFile in Directory.GetFiles(path, "*.csx"))
                {
                    var item = CompletionItem.Create(displayText: Path.GetFileName(scriptFile), rules: s_rules);
                    context.AddItem(item);
                }
            }
        }
    }

}
