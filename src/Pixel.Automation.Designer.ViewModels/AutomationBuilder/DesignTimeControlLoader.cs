using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Scripting.Reference.Manager.Contracts;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public class DesignTimeControlLoader : ControlLoader
    {
        public DesignTimeControlLoader(ApplicationSettings applicationSettings, IFileSystem fileSystem, IReferenceManager referenceManager) 
            : base(applicationSettings, fileSystem, referenceManager)
        {
        }        
    }
}
