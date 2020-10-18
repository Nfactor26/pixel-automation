using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IEditorFactory
    {
        IAutomationEditor CreateAutomationEditor();

        IPrefabEditor CreatePrefabEditor();
    }
}
