using System;

namespace Pixel.Automation.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ImportReferencesFromAttribute : Attribute
{
    public Type ReferencesProvider { get; } 

    public ImportReferencesFromAttribute(Type referencesProvider)
    {
        this.ReferencesProvider = referencesProvider;
    }
}
