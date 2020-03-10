using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Reflection;

namespace Pixel.Automation.Scripting.Components.Arguments
{
    public class ArgumentProcessor : IArgumentProcessor
    {
        IScriptEngine scriptEngine;
        object globalsObject;

        public ArgumentProcessor(IScriptEngine scriptEngine)
        {
            this.scriptEngine = scriptEngine;
        }

        public void SetGlobals(object globalsObject)
        {
            this.globalsObject = globalsObject;
        }

        public T GetValue<T>(Argument argument)
        {

            if (!typeof(T).IsAssignableFrom(argument.GetType().GetGenericArguments()[0]))
            {
                throw new InvalidOperationException($"Can't perform GetValue<{typeof(T)}> operation. Type : {typeof(T)} is not assignable from Type : {argument.GetType().GetGenericArguments()[0]}");
            }

            switch (argument.Mode)
            {
                case ArgumentMode.Default:
                    if (argument is InArgument<T>)
                    {
                        InArgument<T> inArgument = argument as InArgument<T>;
                        return inArgument.DefaultValue;
                    }
                    throw new ArgumentException($"{nameof(Argument)} must be of type InArgument<{typeof(T)}>");
              
                case ArgumentMode.DataBound:
                  
                    if (string.IsNullOrEmpty(argument.PropertyPath))
                    {
                        Log.Warning("{PropertyPath} is not configured for {@Argument}", nameof(argument.PropertyPath), argument);
                        Log.Warning("Returning {value} GetValue on Argument as it might be optional argument.", default);
                        return default;
                    }

                    //Look in variables declared in script engine first
                    if(scriptEngine.HasScriptVariable(argument.PropertyPath))
                    {
                        return scriptEngine.GetVariableValue<T>(argument.PropertyPath);
                    }

                    //Look in globals object
                    string[] nestedProperties = argument.PropertyPath.Split(new char[] { '.' });
                    object currentRoot = globalsObject;
                    PropertyInfo targetProperty = null;
                    foreach (var property in nestedProperties)
                    {
                        targetProperty = currentRoot.GetType().GetProperty(property);
                        if(targetProperty == null)
                        {
                            throw new ArgumentException($"{targetProperty} doesn't exist in DataModel. Property path is {argument.PropertyPath}");
                        }
                        var propertyValue = targetProperty.GetValue(currentRoot);
                        if(propertyValue == null)
                        {
                            return default(T);
                        }
                        currentRoot = propertyValue;
                    }                
                                                      
                    if (typeof(T).IsAssignableFrom(targetProperty.PropertyType))
                    {
                       return (T)currentRoot;                       
                    }
                   
                    throw new ArgumentException($"Failed to {nameof(GetValue)}<{typeof(T)}> for Argument : {argument.PropertyPath} in DataBound Mode." +
                        $"Only simple types and IEnumerable<T> are supported. Consider using scripted mode for complex types");
                
                case ArgumentMode.Scripted:
                    Type scriptDataType = typeof(ScriptArguments<>).MakeGenericType(globalsObject.GetType());
                    var scriptData = Activator.CreateInstance(scriptDataType, new[] { globalsObject });
                    
                    ScriptResult scriptResult = scriptEngine.ExecuteFileAsync(argument.ScriptFile, scriptData, null).Result;               
                    scriptResult = scriptEngine.ExecuteScriptAsync("GetValue(DataModel)", scriptData, scriptResult.CurrentState).Result;                    
                    return (T)scriptResult.ReturnValue;
                default:
                    throw new InvalidOperationException($"Argument mode : {argument.Mode} is not supported");
            }
        }

        public void SetValue<T>(Argument argument, T value)
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
                        return;
                    }

                    //Look in variables declared in script engine first
                    if (scriptEngine.HasScriptVariable(argument.PropertyPath))
                    {
                        scriptEngine.SetVariableValue<T>(argument.PropertyPath, value);
                        return;
                    }


                    string[] nestedProperties = argument.PropertyPath.Split(new char[] { '.' });
                    object currentRoot = globalsObject;
                    PropertyInfo targetProperty = null;
                    foreach (var property in nestedProperties)
                    {
                        targetProperty = currentRoot.GetType().GetProperty(property);
                        if (targetProperty == null)
                        {
                            throw new ArgumentException($"{targetProperty} doesn't exist in DataModel. Property path is {argument.PropertyPath}");
                        }
                        currentRoot = targetProperty.GetValue(currentRoot);                     
                    }

                    //PropertyInfo targetProperty = globalsObject.GetType().GetProperty(argument.PropertyPath);                 
                    if (targetProperty.PropertyType.IsAssignableFrom(typeof(T))) 
                    {
                        targetProperty.SetValue(globalsObject, value);
                        return;
                    }
                    throw new ArgumentException($"Failed to {nameof(SetValue)}<{typeof(T)}> for Argument : {argument.PropertyPath} in DataBound Mode." +
                        $"Only simple types and IEnumerable<T> targets are supported. Consider using scripted mode for complex types");

                case ArgumentMode.Scripted:

                    Type scriptDataType = typeof(SetValueScriptArguments<,>).MakeGenericType(globalsObject.GetType(), typeof(T));
                    var scriptData = Activator.CreateInstance(scriptDataType, new[] { globalsObject, value });

                    ScriptResult scriptResult = scriptEngine.ExecuteFileAsync(argument.ScriptFile, scriptData, null).Result;                   
                    scriptResult = scriptEngine.ExecuteScriptAsync("SetValue(DataModel,ExtractedValue)", scriptData, scriptResult.CurrentState).Result;                   
                    return;

                default:
                    throw new InvalidOperationException($"Argument mode : {argument.Mode} is not supported");
            }
        }

    }
}
