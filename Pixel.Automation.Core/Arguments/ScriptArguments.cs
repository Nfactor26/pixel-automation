using System;

namespace Pixel.Automation.Core.Arguments
{
    public abstract class ScriptArguments
    {
        public EntityManager EntityManager { get; internal set; }

        public abstract object GetModelData();
        
        public abstract Type GetModelType();

    }

    public class ScriptArguments<T> : ScriptArguments where T : new()
    {        

        public T Model { get; }     

        public ScriptArguments(T model) 
        {
            this.Model = model;                     
        }


        public override object GetModelData()
        {
            return this.Model;
        }

        public override Type GetModelType()
        {
            return typeof(T);
        }
    }


    public static class ScriptArgumentsExtension
    {
        public static ScriptArguments ToScriptArguments<T>(this T t, EntityManager entityManager) where T : new()
        {
            var argumentType = t.GetType();
            var scriptArgumentsType = typeof(ScriptArguments<>).MakeGenericType(argumentType);
            ScriptArguments scriptArguments = Activator.CreateInstance(scriptArgumentsType, t) as ScriptArguments;
            scriptArguments.EntityManager = entityManager;
            return scriptArguments;          
        }
    }
}
