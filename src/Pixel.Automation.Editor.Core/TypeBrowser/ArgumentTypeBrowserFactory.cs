using Dawn;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.TypeBrowser;
using System;
using System.Collections.Generic;

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

        public IArgumentTypeBrowser CreateArgumentTypeBrowser(IEnumerable<Type> typesToShow)
        {
            return new ArgumentTypeBrowserViewModel(new SimpleArgumentTypeProvider(typesToShow), true);
        }

        public IArgumentTypeBrowser CreateArgumentTypeBrowser(TypeDefinition selectedType)
        {
            Guard.Argument(selectedType).NotNull();
            return new ArgumentTypeBrowserViewModel(this.typeProvider, selectedType);
        }

        public IArgumentTypeBrowser CreateArgumentTypeBrowser(bool showOnlyCustomTypes)
        {
            return new ArgumentTypeBrowserViewModel(this.typeProvider, showOnlyCustomTypes);
        }
    }
}
