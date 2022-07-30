using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.Core
{
    public static class ArgumentExtensions
    {       
        private static readonly string setValueTemplate = "void SetValue({0} argumentValue){1}{{{1}}}";
        private static readonly string setValueAction = "return ((Action<{0}>)SetValue);";
        private static readonly string getValueTemplate = "{0} GetValue(){1}{{{1}    return default;{1}}}";
        private static readonly string getValueDelegate = "return ((Func<{0}>)GetValue);";
        private static readonly string predicateScriptTemplate = "bool IsMatch(IComponent current, {0} argument){1}{{{1}    return false;{1}}}";
        private static readonly string predicateDelegate = "return ((Func<IComponent, {0}, bool>)IsMatch);";

        /// <summary>
        /// Get the value of the argument
        /// </summary>
        /// <param name="argument">Argument whose value needs to be retrieved</param>
        /// <param name="argumentProcessor"><see cref="IArgumentProcessor"/> instance that can retrieve the value of argument</param>
        /// <returns></returns>
        public static async Task<object> GetValue(this Argument argument, IArgumentProcessor argumentProcessor)
        {
            MethodInfo getValueMethod = argumentProcessor.GetType().GetMethod("GetValueAsync");
            MethodInfo getValueMethodWithClosedType = getValueMethod.MakeGenericMethod(argument.GetArgumentType());
            var value = (Task)getValueMethodWithClosedType.Invoke(argumentProcessor, new object[] { argument });
            await value.ConfigureAwait(false);
            var resultProperty = value.GetType().GetProperty("Result");
            return resultProperty.GetValue(value);
        }

        /// <summary>
        /// Set value of the argument
        /// </summary>
        /// <param name="argument">Argument whose value needs to be set</param>
        /// <param name="argumentProcessor"><see cref="IArgumentProcessor"/> instance that can set the value of argument</param>
        /// <param name="value">Value to be set</param>
        public static async Task SetValue(this Argument argument, IArgumentProcessor argumentProcessor, object value)
        {
            MethodInfo setValueMethod = argumentProcessor.GetType().GetMethod("SetValueAsync");
            MethodInfo setValueMethodWithClosedType = setValueMethod.MakeGenericMethod(argument.GetArgumentType());
            var task = (Task)setValueMethodWithClosedType.Invoke(argumentProcessor, new[] { argument, value });
            await task.ConfigureAwait(false);
        }

        public static string GenerateInitialScript(this Argument argument)
        {
            StringBuilder result = new StringBuilder();
            result.Append(argument.GetArgumentType().GetRequiredImportsForType(Enumerable.Empty<Assembly>(), Enumerable.Empty<string>()));
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
                result.Append($"using {typeof(IComponent).Namespace};{Environment.NewLine}");
                result.Append(GeneratePredicateScript(argument));
                return result.ToString();
            }
            throw new ArgumentException($"parameter : {nameof(argument)} is neither InArgument nor OutArgument");
        }
             

        private static string GenerateSetValueScript(Argument argument)
        {
            var setValueParsedScript = string.Format(setValueTemplate, argument.ArgumentType, Environment.NewLine);
            var setValueParsedAction = string.Format(setValueAction, argument.ArgumentType);
            return $"{setValueParsedScript}{Environment.NewLine}{setValueParsedAction}";
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
