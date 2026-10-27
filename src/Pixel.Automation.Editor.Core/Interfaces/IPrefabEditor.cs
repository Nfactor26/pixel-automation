using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces;

public interface IPrefabEditor : IEditor
{

    PrefabProject PrefabProject { get; }

    Entity RootEntity { get; }

    Task<Entity> DoLoad(PrefabProject prefabProject, VersionInfo versionInfo = null);
}
