using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Pixel.Automation.Core.Components
{
    /// <summary>
    /// Look if component is decorated with ScriptableAttribute and initialize all properties that idenfies a script file on component.
    /// </summary>   
    public class ScriptFileInitializer : IComponentInitializer
    {          
        public void IntializeComponent(IComponent component, EntityManager entityManager)
        {
            var scriptableAttribute = component.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
            if(scriptableAttribute != null)
            {
                var fileSystem = entityManager.GetServiceOfType<IFileSystem>();

                foreach(var scriptFile in scriptableAttribute.ScriptFiles)
                {
                    string scriptLocation = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid().ToString()}.csx"));
                    component.SetPropertyValue<string>(scriptFile, scriptLocation);
                }
            }               
        }
    }
}
