using Pixel.Automation.Core.Arguments;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IArgumentTypeBrowser
    {
        void WithAdditionalTypes(IEnumerable<Type> additionalTypes);

        void WithAdditionalTypesInAssembly(Assembly assembly);

        Argument CreateInArgumentForSelectedType();

        Argument CreateOutArgumentForSelectedType();
    }
}
