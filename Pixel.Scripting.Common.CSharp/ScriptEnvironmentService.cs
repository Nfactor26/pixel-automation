using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Pixel.Scripting.Common.CSharp
{
    [ExportWorkspaceService(typeof(IScriptEnvironmentService))]
    public class ScriptEnvironmentService : IScriptEnvironmentService
    {
        public string BaseDirectory => Environment.CurrentDirectory;

        //TODO : These folders should come from configuration
        public ImmutableArray<string> MetadataReferenceSearchPaths =>
        ImmutableArray.Create<string>(new string[]
        {
            BaseDirectory,
            Path.Combine(BaseDirectory,"Components"),
            Path.Combine(BaseDirectory,"Core")
        });

        public ImmutableArray<string> SourceReferenceSearchPaths =>
            ImmutableArray.Create<string>(new string[] { string.Empty });
    }
}
