namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeBrowserFactory
    {
        /// <summary>
        /// Create an instance of IArgumentTypeBrowser which will show common and custom types (defined by automation project)
        /// initially while providing an option to show all the available types from known assemblies (loaded by application i.e. include .net types as well)
        /// </summary>
        /// <returns></returns>
        IArgumentTypeBrowser CreateArgumentTypeBrowser();

        /// <summary>
        /// Create an instance of IArgumentTypeBrowser where the initial GenericType e.g. List<T>  is already selected and we are showing 
        /// a follow up ArgumentBrowser to pick T.
        /// </summary>
        /// <param name="selectedType"></param>
        /// <returns></returns>
        IArgumentTypeBrowser CreateArgumentTypeBrowser(TypeDefinition selectedType);

        /// <summary>
        /// Create an instance of IArgumentTypeBrowser where only custom types (defined by automation project) is visible and user doesn't have option
        /// to toggle show all types.
        /// </summary>
        /// <param name="showOnlyCustomTypes"></param>
        /// <returns></returns>
        IArgumentTypeBrowser CreateArgumentTypeBrowser(bool showOnlyCustomTypes);
    }
}
