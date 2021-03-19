using System;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface ICompositeTypeExtractor
    {
        /// <summary>
        /// Find all the composite types used by given type in the same assembly to which this type belongs.
        /// This works in a recursive way and any composite type discovered will be processed in the same way as owner type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Type> GetCompositeTypes(Type ownerType);
    }
}
