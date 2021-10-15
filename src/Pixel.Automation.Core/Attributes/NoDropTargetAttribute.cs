using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Entities decorated with NoDropTargetAttribute won't receive any other components on drag drop at design time. 
    /// </summary>
    public class NoDropTargetAttribute : Attribute
    {
    }
}
