using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeBrowser
    {
        TypeDefinition SelectedType { get; }

        Argument CreateInArgumentForSelectedType();

        Argument CreateOutArgumentForSelectedType();
    }
}
