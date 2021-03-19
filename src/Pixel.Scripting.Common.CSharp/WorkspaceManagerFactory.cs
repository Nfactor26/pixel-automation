using Dawn;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Scripting.Common.CSharp
{
    public class WorkspaceManagerFactory : IWorkspaceManagerFactory
    {
        public ICodeWorkspaceManager CreateCodeWorkspaceManager(string workingDirectory)
        {
            Guard.Argument(workingDirectory).NotEmpty().NotNull();
            return new CodeWorkspaceManager(workingDirectory);
        }

        public IScriptWorkspaceManager CreateScriptWorkspaceManager(string workingDirectory)
        {
            Guard.Argument(workingDirectory).NotEmpty().NotNull();     
            return new ScriptWorkSpaceManager(workingDirectory);
        }
    }
}
