using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IEntityManager : IDisposable
    {
        object Arguments { get; set; }

        Entity RootEntity { get; set; }      
       
        IArgumentProcessor GetArgumentProcessor();

        IScriptEngine GetScriptEngine();

        void SetCurrentFileSystem(IFileSystem fileSystem);

        IFileSystem GetCurrentFileSystem();      

        T GetServiceOfType<T>(string key = null);

        IEnumerable<T> GetAllServicesOfType<T>();

        void RegisterDefault<T>(T instance) where T : class;

        IApplication GetOwnerApplication(IComponent component);

        T GetOwnerApplication<T>(IComponent component) where T : class, IApplication;

        IControlLocator GetControlLocator(IControlIdentity forControl);

        ICoordinateProvider GetCoordinateProvider(IControlIdentity forControl);

        void RestoreParentChildRelation(IComponent entityComponent, bool resetId = false);

        IEnumerable<string> GetPropertiesOfType(Type propertyType);
    }
}