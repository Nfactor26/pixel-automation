using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Pixel.Automation.Core.Components;

/// <summary>
/// Look if component is decorated with ScriptableAttribute and initialize all properties that idenfies a script file on component.
/// </summary>   
public class ScriptFileInitializer : IComponentInitializer
{          
    public void IntializeComponent(IComponent component, IEntityManager entityManager)
    {
        var scriptableAttribute = component.GetType().GetCustomAttributes(true).OfType<ScriptableAttribute>().FirstOrDefault();
        if(scriptableAttribute != null)
        {
            var fileSystem = entityManager.GetCurrentFileSystem();
            string scriptsDirectory = fileSystem.ScriptsDirectory;
            if(fileSystem is IProjectFileSystem projectFileSystem)
            {

                if(component.TryGetAnsecstorOfType(out PrefabHostEntity prefabHost))
                {
                    var prefabLoader = entityManager.GetServiceOfType<IPrefabLoader>();
                    Version prefabVersionInUse = prefabLoader.GetPrefabVersionInUse(prefabHost.ApplicationId, prefabHost.PrefabId);
                    scriptsDirectory = Path.Combine(projectFileSystem.ApplicationsDirectory, prefabHost.ApplicationId, Constants.PrefabsDirectory, prefabHost.PrefabId, prefabVersionInUse.ToString());                    
                }
                //On opening a TestFixture or TestCase, it's tag is set to it's Id. We combine them to get the path where scripts should reside
                //when the component is added to a TestCase or TestFixture
                else if (component.TryGetAnsecstorOfType(out TestFixtureEntity testFixtureEntity))
                {
                    scriptsDirectory = Path.Combine(projectFileSystem.TestCaseRepository, testFixtureEntity.Tag);
                    if (component.TryGetAnsecstorOfType(out TestCaseEntity testCaseEntity))
                    {
                        scriptsDirectory = Path.Combine(scriptsDirectory, testCaseEntity.Tag);
                    }
                }                   
            }                         

            foreach(var scriptFile in scriptableAttribute.ScriptFiles)
            {
                string scriptLocation = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(scriptsDirectory, $"{Guid.NewGuid()}.csx")).Replace("\\", "/");
                component.SetPropertyValue<string>(scriptFile, scriptLocation);
            }
        }               
    }
}
