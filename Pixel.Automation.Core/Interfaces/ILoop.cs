using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Interfaces
{
    public interface ILoop : IComponent
    {
        bool ExitCriteriaSatisfied
        {
            get;set;
        }  
      
    }
}
