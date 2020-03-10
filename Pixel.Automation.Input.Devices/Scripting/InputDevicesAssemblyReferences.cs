using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Reflection;


namespace Pixel.Automation.Input.Devices
{
    public class InputDevicesAssemblyReferences : IScriptReferencesProvider
    {
        public List<Assembly> GetAssembliesToReference()
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(typeof(InputSimulatorBase).Assembly); //this assembly              
            return assemblies;
        }
    }
}
