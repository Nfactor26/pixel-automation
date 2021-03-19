namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeBrowserFactory
    {
        IArgumentTypeBrowser CreateArgumentTypeBrowser();

        IArgumentTypeBrowser CreateArgumentTypeBrowser(TypeDefinition selectedType);
    }
}
