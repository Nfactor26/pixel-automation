using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Editor.Core
{
    public class PrefabUpdatedEventArgs : EventArgs
    {
        public PrefabDescription TargetPrefab { get; }

        public PrefabUpdatedEventArgs(PrefabDescription targetPrefab)
        {
            this.TargetPrefab = targetPrefab;
        }
    }
}
