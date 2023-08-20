using Dawn;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Pixel.Automation.Core;

public static class Telemetry
{
    private static readonly ConcurrentDictionary<string, ActivitySource> activitySources = new();
   
    /// <summary>
    /// Default ActivitySource for the application
    /// </summary>
    public static ActivitySource DefaultSource { get; private set; }

    /// <summary>
    /// Application on startup needs to setup a default ActivitySource
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    public static void InitializeDefault(string name, string version)
    {
        Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
        Guard.Argument(version, nameof(version)).NotNull().NotEmpty();
        DefaultSource = new ActivitySource(name, version);
        activitySources.TryAdd(name, DefaultSource);
    }
 
    /// <summary>
    /// Get ActivitySource with a given name. A new source will be created if it doesn't already exist.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public static ActivitySource GetOrAddSource(string name, string? version)
    {        
        if(!activitySources.ContainsKey(name))
        {
            activitySources.TryAdd(name, new ActivitySource(name, version));
        }
        return  activitySources[name];      
    }

}
