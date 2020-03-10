using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IServiceResolver : ICloneable
    {
        T Get<T>(string key = null);      
       
        IEnumerable<T> GetAll<T>();

        void RegisterDefault<T>(T instance) where T : class;

        void ConfigureDefaultServices(IFileSystem fileSystem, object globalsInstance);

        void OnDataModelUpdated(IFileSystem fileSystem, object previousModelInstance, object newDataModelInstance);

    }
}
