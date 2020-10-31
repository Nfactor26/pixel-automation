using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeBrowser
    {
        /// <summary>
        /// Current Type selected 
        /// </summary>
        TypeDefinition SelectedType { get; }

        TypeDefinition GetCreatedType();
       
        Argument CreateInArgumentForSelectedType();

        Argument CreateOutArgumentForSelectedType();
    }
}
