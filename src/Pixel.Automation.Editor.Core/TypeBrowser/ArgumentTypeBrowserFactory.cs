using Dawn;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Editor.TypeBrowser
{
    public class ArgumentTypeBrowserFactory : IArgumentTypeBrowserFactory
    {
        private readonly IArgumentTypeProvider typeProvider;

        public ArgumentTypeBrowserFactory(IArgumentTypeProvider typeProvider)
        {
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;
        }

        public IArgumentTypeBrowser CreateArgumentTypeBrowser()
        {
            return new ArgumentTypeBrowserViewModel(this.typeProvider);
        }

        public IArgumentTypeBrowser CreateArgumentTypeBrowser(TypeDefinition selectedType)
        {
            Guard.Argument(selectedType).NotNull();
            return new ArgumentTypeBrowserViewModel(this.typeProvider, selectedType);
        }
    }
}
