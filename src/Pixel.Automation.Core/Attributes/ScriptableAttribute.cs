using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// Indicates that a component has local scripts.
    /// This information is used by prefab builder to import associated script files
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScriptableAttribute : Attribute
    {
        public List<string> ScriptFiles { get; }

        public ScriptableAttribute(params string[] scriptFiles)
        {
            ScriptFiles = new List<string>();
            foreach(var file in scriptFiles)
            {
                ScriptFiles.Add(file);
            }
        }
    }
}
