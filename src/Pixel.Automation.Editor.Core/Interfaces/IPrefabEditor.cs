using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IPrefabEditor : IEditor
    {      
       void DoLoad(PrefabProject prefabProject, VersionInfo versionInfo = null);
    }
}
