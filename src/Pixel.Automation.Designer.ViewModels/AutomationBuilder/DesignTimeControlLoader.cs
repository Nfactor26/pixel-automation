using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.RunTime;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class DesignTimeControlLoader : ControlLoader
    {
        public DesignTimeControlLoader(ApplicationSettings applicationSettings, IFileSystem fileSystem) 
            : base(applicationSettings, fileSystem)
        {
        }

        /// <summary>
        /// At design time, the ControlReferences file can change and hence we need to load it on every access.
        /// </summary>
        /// <returns></returns>
        protected override ControlReferences GetControlReferences()
        {
           return this.fileSystem.LoadFile<ControlReferences>(this.fileSystem.ControlReferencesFile);
        }
    }
}
