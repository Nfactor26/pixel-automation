using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IPrefabEditor : IEditor
    {      
       Task DoLoad(PrefabProject prefabProject, VersionInfo versionInfo = null);
    }
}
