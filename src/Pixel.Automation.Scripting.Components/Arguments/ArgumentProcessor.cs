using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components.Arguments
{
    ///<inheritdoc/>
    public class ArgumentProcessor : IArgumentProcessor
    {
        private IScriptEngine scriptEngine;
        private object globalsObject;

        public void Initialize(IScriptEngine scriptEngine, object globalsObject)
        {          
            this.scriptEngine = Guard.Argument<IScriptEngine>(scriptEngine).NotNull().Value;
            this.globalsObject = Guard.Argument(globalsObject).NotNull().Value;
        }
     
        public async Task<T> GetValueAsync<T>(Argument argument)
        {

            if (!typeof(T).IsAssignableFrom(argument.GetType().GetGenericArguments()[0]))
            {
                throw new InvalidOperationException($"Can't perform GetValue<{typeof(T)}> operation. Type : {typeof(T)} is not assignable from Type : {argument.GetType().GetGenericArguments()[0]}");
            }

            switch (argument.Mode)
            {
                case ArgumentMode.Default:
                    if (argument is IDefaultValueProvider<T> inArgument)
                    {                      
                        return inArgument.GetDefaultValue();
                    }
                    throw new ArgumentException($"{nameof(Argument)} must be of type InArgument<{typeof(T)}>");
              
                case ArgumentMode.DataBound:
                  
                    if (string.IsNullOrEmpty(argument.PropertyPath))
                    {
                        Log.Warning("{PropertyPath} is not configured for {@Argument}", nameof(argument.PropertyPath), argument);
                        Log.Warning("Returning {value} GetValue on Argument as it might be optional argument.", default);
                        return default;
                    }

                    string[] nestedProperties = argument.PropertyPath.Split(new char[] { '.' });
                    object result = default;
                    //Look in variables declared in script engine first
                    if (scriptEngine.HasScriptVariable(nestedProperties[0]))
                    {
                        if(nestedProperties.Length == 1)
                        {
                            return scriptEngine.GetVariableValue<T>(nestedProperties[0]);
                        }                    
                        else
                        {
                            var retrievedValue = scriptEngine.GetVariableValue<object>(nestedProperties[0]);
                            result = GetNestedPropertyValue(retrievedValue, nestedProperties.Skip(1));
                        }

                    }
                    else  //Look in globals object        
                    {                                 
                        result = GetNestedPropertyValue(globalsObject, nestedProperties);
                    }                     
                                                      
                    if (typeof(T).IsAssignableFrom(result.GetType()))
                    {
                       return (T)result;                       
                    }
                   
                    throw new ArgumentException($"Failed to {nameof(GetValueAsync)}<{typeof(T)}> for Argument : {argument.PropertyPath} in DataBound Mode." +
                        $"Only simple types and IEnumerable<T> are supported. Consider using scripted mode for complex types");
                
                case ArgumentMode.Scripted:
                    var fn = await scriptEngine.CreateDelegateAsync<Func<T>>(argument.ScriptFile);
                    return fn();                        
               
                default:
                    throw new InvalidOperationException($"Argument mode : {argument.Mode} is not supported");
            }
        }

        public async Task SetValueAsync<T>(Argument argument, T value)
        {
            if (!argument.GetType().GetGenericArguments()[0].IsAssignableFrom(value.GetType()))
            {
                throw new InvalidOperationException($"Can't perform SetValue<{typeof(T)}> operation on Property {argument.PropertyPath}. Type : {argument.GetType().GetGenericArguments()[0]} is not assignable from Type : {value.GetType()}");
            }

            switch (argument.Mode)
            {
                case ArgumentMode.DataBound:
                    if (string.IsNullOrEmpty(argument.PropertyPath))
                    {
                        Log.Warning("{PropertyPath} is not configure for {@Argument}", nameof(argument.PropertyPath), argument);
                        Log.Warning("Skipping operation SetValue on Argument as it might be optional argument.");
                        break;
                    }
                  
                    string[] nestedProperties = argument.PropertyPath.Split(new char[] { '.' });
                  
                    //Look in variables declared in script engine first
                    if (scriptEngine.HasScriptVariable(nestedProperties[0]))
                    {
                        if (nestedProperties.Length == 1)
                        {
                            scriptEngine.SetVariableValue<T>(nestedProperties[0], value);                           
                        }
                        else
                        {
                            var retrievedValue = scriptEngine.GetVariableValue<object>(nestedProperties[0]);
                            if (!TrySetNestedPropertyValue<T>(retrievedValue, value, nestedProperties.Skip(1)))
                            {
                                throw new InvalidOperationException($"Failed to {nameof(SetValueAsync)}<{typeof(T)}> for Argument : {argument.PropertyPath} in DataBound Mode. {typeof(T)} could not be assigned to property {nestedProperties.Last()}. Verify types are compatible.");
                            }                            
                        }                       
                    }                              
                    else 
                    {
                        if (!TrySetNestedPropertyValue<T>(globalsObject, value, nestedProperties))
                        {
                            throw new InvalidOperationException($"Failed to {nameof(SetValueAsync)}<{typeof(T)}> for Argument : {argument.PropertyPath} in DataBound Mode. {typeof(T)} could not be assigned to property {nestedProperties.Last()}. Verify types are compatible.");
                        }                      
                    }
                    break;
                case ArgumentMode.Scripted:
                    var fn = await scriptEngine.CreateDelegateAsync<Action<T>>(argument.ScriptFile);
                    fn(value);
                    break;

                default:
                    throw new InvalidOperationException($"Argument mode : {argument.Mode} is not supported");
            }
        }

        private object GetNestedPropertyValue(object root, IEnumerable<string> nestedProperties)
        {           
            PropertyInfo targetProperty = null;
            foreach (var property in nestedProperties)
            {
                targetProperty = root.GetType().GetProperty(property);
                if (targetProperty == null)
                {
                    throw new ArgumentException($"Could not find property {property} in type {root.GetType()}");
                }
                var propertyValue = targetProperty.GetValue(root);
                if (propertyValue == null)
                {
                    return null;
                }
                root = propertyValue;
            }
            return root;
        }

        private bool TrySetNestedPropertyValue<T>(object root, T value, IEnumerable<string> nestedProperties)
        {       
            PropertyInfo targetProperty = null;
            foreach (var property in nestedProperties)
            {
                root = targetProperty?.GetValue(root) ?? root;
                targetProperty = root.GetType().GetProperty(property);
                if (targetProperty == null)
                {
                    throw new ArgumentException($"Could not find property {targetProperty} in type {root.GetType()}");
                }
               
            }
           
            if (targetProperty.PropertyType.IsAssignableFrom(typeof(T)))
            {
                targetProperty.SetValue(root, value);
                return true;
            }

            return false;
        }

    }
}
