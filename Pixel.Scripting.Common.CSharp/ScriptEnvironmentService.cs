using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;

namespace Pixel.Scripting.Common.CSharp
{
    [ExportWorkspaceService(typeof(IScriptEnvironmentService))]
    public class ScriptEnvironmentService : IScriptEnvironmentService
    {
        public string BaseDirectory => Environment.CurrentDirectory;
     
        public ImmutableArray<string> MetadataReferenceSearchPaths =>  ImmutableArray.Create<string>(new string[]
        {
            BaseDirectory
        });

        public ImmutableArray<string> SourceReferenceSearchPaths =>  ImmutableArray.Create<string>(new string[] { string.Empty });
    }
}
