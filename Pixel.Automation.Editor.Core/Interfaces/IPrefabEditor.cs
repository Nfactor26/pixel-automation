using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IPrefabEditor : IEditor
    {
       event EventHandler<PrefabUpdatedEventArgs> PrefabUpdated;

       event EventHandler<EditorClosingEventArgs> EditorClosing;

       void DoLoad(PrefabDescription prefabDescription);
    }
}
