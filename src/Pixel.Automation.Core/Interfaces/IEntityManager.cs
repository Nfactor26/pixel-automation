using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IEntityManager : IDisposable
    {
        /// <summary>
        /// Arguments for entity manager is the globals type for the script manager at this scope.
        /// The scripting system can access and modify any properties available on globals object.
        /// Additionally, the argument system can data bind to properties available on Arguments object.
        /// </summary>
        object Arguments { get; set; }

        /// <summary>
        /// RootEntity always points to the root entity for a process.
        /// Primary and secondary entity managers all point to same root irrespective of their scope.
        /// </summary>
        Entity RootEntity { get; set; }

        /// <summary>
        /// Set a identifier for the entity manager.
        /// This is helpful while debugging.
        /// </summary>
        /// <param name="identifier">Value of identifier</param>
        void SetIdentifier(string identifier);

        /// <summary>
        /// Set the <see cref="IFileSystem"/> that should be used by entity manager.
        /// </summary>
        /// <param name="fileSystem"></param>
        void SetCurrentFileSystem(IFileSystem fileSystem);

        /// <summary>
        /// Get the <see cref="IFileSystem"/> associated with the entit manager.
        /// <see cref="IFileSystem"/> understands the directory structure for project reosurces and helps to locate and work with them. 
        /// </summary>
        /// <returns></returns>
        IFileSystem GetCurrentFileSystem();


        /// <summary>
        /// Get the <see cref="IArgumentProcessor"/> associated with this entity manager.
        /// Components can get and set argument values using the <see cref="IArgumentProcessor"/> for the entity manager.
        /// </summary>
        /// <returns></returns>
        IArgumentProcessor GetArgumentProcessor();

        /// <summary>
        /// Get the <see cref="IScriptEngine"/> associated with this entity manager.
        /// <see cref="IScriptEngine"/> can be used to execute a script file.
        /// </summary>
        /// <returns></returns>
        IScriptEngine GetScriptEngine();
       
        /// <summary>
        /// Resolve any dependencies using the underlying service resolver.
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="key">Key to resolve if any</param>
        /// <returns></returns>
        T GetServiceOfType<T>(string key = null);

        /// <summary>
        /// Resolve all dependencies of a given type using underlying service resolver.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAllServicesOfType<T>();

        /// <summary>
        /// Register an instance for a type as default. Any attempt to resolve this type will always return
        /// this instance. Instance is registered as default for type with the underlying service resolver.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        void RegisterDefault<T>(T instance) where T : class;

        /// <summary>
        /// Any component that is a child of <see cref="IApplicationContext"/> entity can get the <see cref="IApplication"/> 
        /// instance for this application context. Actor components reuiqre access to <see cref="IApplication"/> to interacte with them
        /// e.g. close the application or maximize or minimize application, etc.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        bool TryGetOwnerApplication(IComponent component, out IApplication application);

        /// <summary>
        /// Any component that is a child of <see cref="IApplicationContext"/> entity can get the <see cref="IApplication"/> 
        /// instance for this application context. Actor components reuiqre access to <see cref="IApplication"/> to interacte with them
        /// e.g. close the application or maximize or minimize application, etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        bool TryGetOwnerApplication<T>(IComponent component, out T application) where T : class, IApplication;

        /// <summary>
        /// Any component that is a child of <see cref="IApplicationContext"/> entity can get the <see cref="IApplication"/> 
        /// instance for this application context. Actor components reuiqre access to <see cref="IApplication"/> to interacte with them
        /// e.g. close the application or maximize or minimize application, etc.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        IApplication GetOwnerApplication(IComponent component);

        /// <summary>
        /// Any component that is a child of <see cref="IApplicationContext"/> entity can get the <see cref="IApplication"/> 
        /// instance for this application context. Actor components reuiqre access to <see cref="IApplication"/> to interacte with them
        /// e.g. close the application or maximize or minimize application, etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        T GetOwnerApplication<T>(IComponent component) where T : class, IApplication;

        /// <summary>
        /// Any component that is a child of <see cref="IApplicationContext"/> entity can get the <see cref="IApplicationEntity"/> 
        /// instance for this application context. Application entity holds the design time configuration data and can be used to
        /// launch, close, attach to application.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        IApplicationEntity GetApplicationEntity(IComponent component);

        /// <summary>
        /// Get the <see cref="IControlLocator"/> for a given control.
        /// Multiple <see cref="IControlLocator"/> can be added to Application Entity at design time.
        /// Based on the type of <see cref="IControlIdentity"/> which makes the request, the first available <see cref="IControlLocator"/>
        /// that can process this type will be returned.
        /// </summary>
        /// <param name="forControl"></param>
        /// <returns></returns>
        IControlLocator GetControlLocator(IControlIdentity forControl);

        /// <summary>
        /// Get the <see cref="ICoordinateProvider"/> for a given control.
        /// Multiple <see cref="ICoordinateProvider"/> can be added to Application Entity at design time.
        /// Based on the type of <see cref="IControlIdentity"/> which makes the request, the first available <see cref="ICoordinateProvider"/>
        /// that can process this type will be returned.
        /// </summary>
        /// <param name="forControl"></param>
        /// <returns></returns>
        ICoordinateProvider GetCoordinateProvider(IControlIdentity forControl);

        /// <summary>
        /// Ensures that the descendant components of an Entity correct have EntitManager and Parent.
        /// </summary>
        /// <param name="entityComponent"></param>
        void RestoreParentChildRelation(Entity entity);

        /// <summary>
        /// Get the names of all properties of a given type available on the Arguments instance.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        IEnumerable<string> GetPropertiesOfType(Type propertyType);
    }
}