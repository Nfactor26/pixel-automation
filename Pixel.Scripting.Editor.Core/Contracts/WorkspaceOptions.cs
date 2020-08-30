using System.Reflection;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public class WorkspaceOptions
    {
        public string WorkingDirectory { get; set; }

        public bool EnableDocumentation { get; set; } = true;

        public bool EnableCodeActions { get; set; } = true;

        public bool EnableDiagnostics { get; set; } = true;

        public string[] AssemblyReferences { get; set; } 

        public WorkspaceType WorkspaceType { get; set; }
    }

    public enum WorkspaceType
    {
        Code,
        Script
    }
}
