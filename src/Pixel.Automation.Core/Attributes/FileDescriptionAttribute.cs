using System;

namespace Pixel.Automation.Core.Attributes
{
    /// <summary>
    /// DataModels can be decorated with this attribute to denote the file extension that will be used while persisting them on disk.
    /// This information is also used by FileSystem when Loading DataModels from a given directory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FileDescriptionAttribute : Attribute
    {       
        /// <summary>
        /// Identifies the extension to be used for persisting the file on disk
        /// </summary>
        public string Extension { get; }

        public FileDescriptionAttribute(string extension)
        {
            this.Extension = extension;           
        }
    }
}
