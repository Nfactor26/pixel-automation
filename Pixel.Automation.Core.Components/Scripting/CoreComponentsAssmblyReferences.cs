using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Core.Components.Scripting
{
    public class CoreComponentsAssmblyReferences : IScriptReferencesProvider
    {
        public List<Assembly> GetAssembliesToReference()
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(typeof(UIControl).Assembly); //Pixel.Automation.Core
            assemblies.Add(typeof(ApplicationEntity).Assembly); //this assembly           
            return assemblies;
        }
    }
}
