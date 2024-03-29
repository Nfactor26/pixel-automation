﻿using System.Collections.Generic;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public class WorkspaceOptions
    {
        public string WorkingDirectory { get; set; }

        public bool EnableDocumentation { get; set; } = true;

        public bool EnableCodeActions { get; set; } = true;

        public bool EnableDiagnostics { get; set; } = true;

        public IEnumerable<string> AssemblyReferences { get; set; }

        public IEnumerable<string> Imports { get; set; }

        public WorkspaceType WorkspaceType { get; set; }
    }

    public enum WorkspaceType
    {
        Code,
        Script
    }
}
