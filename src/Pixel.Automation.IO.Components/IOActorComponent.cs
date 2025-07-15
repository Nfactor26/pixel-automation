using Pixel.Automation.Core;

namespace Pixel.Automation.IO.Components;

public abstract class IOActorComponent : ActorComponent
{

    public IOActorComponent(string name, string displayName) : base(name, displayName)
    {
        
    }
    
    protected void ThrowIfPathNotExists(string path)
    {
        if (!Path.Exists(path))
        {
            if(Path.GetExtension(path) == string.Empty)
            {
                throw new DirectoryNotFoundException($"The specified directory '{path}' does not exist.");
            }
            throw new FileNotFoundException($"The specified path '{path}' does not exist.");
        }
    }
}
