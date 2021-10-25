using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IControlEditorFactory
    {
        IControlEditor CreateControlEditor(IControlIdentity controlIdentity);
    }
}
