using Pixel.Automation.Core.Arguments;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IServiceResolver : ICloneable
    {
        T Get<T>(string key = null);      
       
        IEnumerable<T> GetAll<T>();

        void RegisterDefault<T>(T instance) where T : class;

        void ConfigureDefaultServices(IFileSystem fileSystem, ScriptArguments scriptArguments);

        void OnDataModelUpdated(IFileSystem fileSystem, ScriptArguments prevArgs, ScriptArguments newArgs);

    }
}
