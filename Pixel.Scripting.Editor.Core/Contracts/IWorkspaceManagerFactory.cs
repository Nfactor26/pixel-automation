using System;

namespace Pixel.Scripting.Editor.Core.Contracts
{
    public interface IWorkspaceManagerFactory
    {
        ICodeWorkspaceManager CreateCodeWorkspaceManager(string workingDirectory);

        IScriptWorkspaceManager CreateScriptWorkspaceManager(string workingDirectory);
    }
}
