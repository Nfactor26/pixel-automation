using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IEventHandler
    {
        void HandleSuccess(IComponent component,Dictionary<string,object> extensions);

        void HandleFailure(IComponent component,Dictionary<string, object> extensions);

    }
}
