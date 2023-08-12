using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Pixel.Automation.RunTime;

public class PluginManager
{
    private readonly ILogger logger = Log.ForContext<PluginManager>();
    private readonly ISerializer serializer;
    private List<PluginDescription> pluginDescriptions = new List<PluginDescription>();

    public PluginManager(ISerializer serializer)
    {
        this.serializer = Guard.Argument(serializer).Value;
        var configuredPlugins = this.serializer.Deserialize<List<PluginDescription>>(Path.Combine(AppContext.BaseDirectory, "Plugins.json"));
        this.pluginDescriptions.AddRange(configuredPlugins);
    }

    /// <summary>
    /// Load all the plugins from a given directory.
    /// Plugin directory should contains multiple folders such that each folder is named after the plugin assembly without .dll extension.
    /// Loader will enumerate through all sub folders and load an assembly with name of the subfolder.
    /// </summary>
    /// <param name="pluginDirectory"></param>
    public IEnumerable<Assembly> LoadPluginsFromDirectory(string pluginDirectory, PluginType pluginType)
    {
        Guard.Argument(pluginDirectory).NotNull().NotEmpty();
        if(!Directory.Exists(pluginDirectory))
        {
            throw new DirectoryNotFoundException($"Directory {pluginDirectory} doesn't exist.");
        }

        List<Assembly> pluginAssemblies = new List<Assembly>();

        var pluginsDir = Path.Combine(AppContext.BaseDirectory, pluginDirectory);
        foreach (var pluginDescription in this.pluginDescriptions)
        {
            if (!pluginDescription.Type.Equals(pluginType))
            {              
                continue;
            }

            var pluginDir = Path.Combine(pluginsDir, pluginDescription.Name);
            if(!Directory.Exists(pluginDir))
            {
                logger.Warning("Plugin {0} doesn't exist at path {1}", pluginDescription.Name, pluginDir);
                continue;
            }
            foreach (var supportedPlatform in pluginDescription.SupportedPlatforms)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Create(supportedPlatform)))
                {
                    continue;
                }
                var pluginDll = Path.Combine(pluginDir, pluginDescription.Name + ".dll");
                pluginAssemblies.Add(Assembly.LoadFrom(pluginDll));

                //using (var loader = PluginLoader.CreateFromAssemblyFile(pluginDll, p =>
                //{
                //    p.PreferSharedTypes = true;
                //}))
                //{
                //    pluginAssemblies.Add(loader.LoadDefaultAssembly());
                //    //if (!string.IsNullOrEmpty(pluginDescription.Scrapper))
                //    //{
                //    //    var scrapperPluginDir = Path.Combine(pluginsDir, pluginDescription.Scrapper);
                //    //    foreach (var assemblyFile in Directory.GetFiles(scrapperPluginDir, "*.dll"))
                //    //    {
                //    //        pluginAssemblies.Add(loader.LoadAssemblyFromPath(assemblyFile));
                //    //    }
                //    //}
                //}
                logger.Information("Loaded Plugin {@pluginDescription}", pluginDescription);
                break;
            }           
        }           
        return pluginAssemblies;
    }    

    public void ListLoadedAssemblies()
    {
        foreach (var assemblyLoadContext in AssemblyLoadContext.All)
        {
            logger.Information("There are {0} assemblies in context : {1}", assemblyLoadContext.Assemblies.Count(), assemblyLoadContext.Name);
            foreach (var assembly in assemblyLoadContext.Assemblies)
            {
                logger.Information("-- {0}", assembly.FullName);
            }

        }
    }
}
