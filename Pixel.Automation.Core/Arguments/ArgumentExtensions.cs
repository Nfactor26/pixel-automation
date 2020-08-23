using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pixel.Automation.Core
{
    public static class ArgumentExtensions
    {
        private static readonly string referenceTemplate = "#r \"{0}\"";
        private static readonly string usingTemplate = "using {0};";
        private static readonly string setValueTemplate = "void SetValue({0} argumentValue){1}{{{1}}}{}";
        private static readonly string setValueAction = "return ((Action<{0}>)SetValue);";
        private static readonly string getValueTemplate = "{0} GetValue(){1}{{{1}    return default;{1}}}";
        private static readonly string getValueDelegate = "return ((Func<{0}>)GetValue);";
        private static readonly string predicateScriptTemplate = "bool IsMatch(IComponent current, {0} argument){1}{{{1}    return false;{1}}}";
        private static readonly string predicateDelegate = "return ((Func<IComponent,{0},bool>)IsMatch);";

        public static object GetValue(this Argument argument, IArgumentProcessor argumentProcessor)
        {
            MethodInfo getValueMethod = argumentProcessor.GetType().GetMethod("GetValue");
            MethodInfo getValueMethodWithClosedType = getValueMethod.MakeGenericMethod(argument.GetArgumentType());
            var value = getValueMethodWithClosedType.Invoke(argumentProcessor, new object[] { argument });
            return value;
        }

        public static void SetValue(this Argument argument, IArgumentProcessor argumentProcessor, object value)
        {
            MethodInfo setValueMethod = argumentProcessor.GetType().GetMethod("SetValue");
            MethodInfo setValueMethodWithClosedType = setValueMethod.MakeGenericMethod(argument.GetArgumentType());
            setValueMethodWithClosedType.Invoke(argumentProcessor, new[] { argument, value });
        }

        public static string GenerateInitialScript(this Argument argument)
        {
            StringBuilder result = new StringBuilder();
            result.Append(GetRequiredImportsForArgument(argument));
            if(argument.GetType().Name.Equals(typeof(InArgument<>).Name))
            {
                result.Append(GenerateGetValueScript(argument));
                return result.ToString();
            }
            else if(argument.GetType().Name.Equals(typeof(OutArgument<>).Name))
            {
                result.Append(GenerateSetValueScript(argument));
                return result.ToString();
            }
            else if(argument.GetType().Name.Equals(typeof(PredicateArgument<>).Name))
            {
                result.Append(GeneratePredicateScript(argument));
                return result.ToString();
            }
            throw new ArgumentException($"parameter : {nameof(argument)} is neither InArgument nor OutArgument");
        }

        private static string GetRequiredImportsForArgument(Argument argument)
        {
            StringBuilder result = new StringBuilder();
            var dllReferences = GetDllReferences(argument);
            foreach(var reference in dllReferences)
            {
                result.Append(string.Format(referenceTemplate, reference));
                result.Append(Environment.NewLine);
            }

            var usingDirectives = GetUsingDirectives(argument);
            foreach (var usingDirective in usingDirectives)
            {
                result.Append(string.Format(usingTemplate, usingDirective));
                result.Append(Environment.NewLine);
            }
            result.Append(Environment.NewLine);
            return result.ToString();
        }

        private static IEnumerable<string> GetDllReferences(Argument argument)
        {
            List<string> distinctReferences = new List<string>();

            string containedInDll = argument.GetArgumentType().Assembly.Location;

            //include those in application directory but not in Temp folder i.e. dynamically compiled dll's by project.
            if (containedInDll.StartsWith(Environment.CurrentDirectory) && !containedInDll.Contains("Temp"))
            {
                distinctReferences.Add(Path.GetFileName(containedInDll));
            }
            if (argument.GetArgumentType().IsGenericType)
            {
                foreach (var type in argument.GetArgumentType().GenericTypeArguments)
                {
                    containedInDll = type.Assembly.Location;
                    if (containedInDll.StartsWith(Environment.CurrentDirectory) && !containedInDll.Contains("Temp") && !distinctReferences.Contains(Path.GetFileName(containedInDll)))
                    {
                        distinctReferences.Add(Path.GetFileName(containedInDll));
                    }
                }
            }
            return distinctReferences;

        }

        private static IEnumerable<string> GetUsingDirectives(Argument argument)
        {
            List<string> distinceDirectives = new List<string>();
            distinceDirectives.Add(typeof(Component).Namespace);
            distinceDirectives.Add(typeof(IComponent).Namespace);
            distinceDirectives.Add(argument.GetArgumentType().Namespace);
         
            if (argument.GetArgumentType().IsGenericType)
            {
                foreach (var type in argument.GetArgumentType().GenericTypeArguments)
                {
                    if(!distinceDirectives.Contains(type.Namespace))
                    {
                        distinceDirectives.Add(type.Namespace);
                    }
                }
            }
            return distinceDirectives;
        }


        private static string GenerateSetValueScript(Argument argument)
        {
            var setValueParseScript = string.Format(setValueTemplate, argument.ArgumentType, Environment.NewLine);
            var setValueParsedAction = string.Format(setValueAction, argument.ArgumentType);
            return $"{setValueParseScript}{Environment.NewLine}{setValueParsedAction}";
        }

        private static string GenerateGetValueScript(Argument argument)
        {
            string getValueParsedScript = string.Format(getValueTemplate, argument.ArgumentType, Environment.NewLine);
            string getValueParsedDelegate = string.Format(getValueDelegate, argument.ArgumentType);
            return $"{getValueParsedScript}{Environment.NewLine}{getValueParsedDelegate}";
        }
        private static string GeneratePredicateScript(Argument argument)
        {
            string predicateParsedScript = string.Format(predicateScriptTemplate, argument.ArgumentType, Environment.NewLine);
            string predicateParsedDelegate = string.Format(predicateDelegate, argument.ArgumentType);
            return $"{predicateParsedScript}{Environment.NewLine}{predicateParsedDelegate}";
        }
    }
}
