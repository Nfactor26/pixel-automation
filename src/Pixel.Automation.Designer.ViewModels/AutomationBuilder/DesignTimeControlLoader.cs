using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder;

public class DesignTimeControlLoader : ControlLoader
{
    public DesignTimeControlLoader(ApplicationSettings applicationSettings, IFileSystem fileSystem) 
        : base(applicationSettings, fileSystem)
    {
    }        
}
