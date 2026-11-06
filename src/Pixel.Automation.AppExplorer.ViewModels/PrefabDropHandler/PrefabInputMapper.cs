using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Text;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Helper class to generate code for mappping between source and target type for a prefab input script.
    /// Source type is the input for prefab and target type is the model used within prefab.
    /// </summary>
    public class PrefabInputMapper : IPrefabArgumentMapper
    {
        public  IEnumerable<PropertyMap> GenerateMapping(IScriptEngine scriptEngine, Type assignFrom, Type assignTo)
        {
            Guard.Argument(scriptEngine).NotNull();
            Guard.Argument(assignFrom).NotNull();
            Guard.Argument(assignTo).NotNull();

            var assignFromProperties = assignFrom.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();

            //Get all the properties with attribute ParameterUsage.Input or ParameterUsage.InOut declared in assignTo Type.
            //These are the properties that needs to be mapped to source proerties from assignFrom Type.
            var assignToProperties = assignTo.GetProperties().Where(a =>
            {
                var attribute = a.GetCustomAttributes(typeof(ParameterUsageAttribute), false).FirstOrDefault();
                if (attribute != null)
                {
                    if (attribute is ParameterUsageAttribute parameterUsageAttribute)
                    {
                        if (parameterUsageAttribute.ParameterUsage == ParameterUsage.Input || parameterUsageAttribute.ParameterUsage == ParameterUsage.InOut)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });

            foreach (var property in assignToProperties)
            {
                var propertyMap = new PropertyMap() { AssignTo = property.Name, AssignToType = property.PropertyType, AssignFrom = string.Empty };

                //pick if the name type matches or type display name matches
                var possibleMappings = assignFromProperties.Where(t => t.PropertyType.Equals(property.PropertyType) || t.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));

                var matchingMapping = possibleMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.Equals(property.Name))
                                           ?? possibleMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) && p.Name.Equals(property.Name));

                if (matchingMapping != null)
                {
                    propertyMap.AssignFrom = matchingMapping.Name;
                    propertyMap.AssignFromType = matchingMapping.PropertyType;
                }

                //pick if the name type matches or name matches for variables declared in script engine
                var possibleScriptVariableMappings = scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.AssignToType) || s.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));

                var matchingScriptMapping = possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.PropertyName.Equals(property.Name)) ?? possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) && p.PropertyName.Equals(property.Name));

                if (matchingScriptMapping != null)
                {
                    propertyMap.AssignFrom = matchingScriptMapping.PropertyName;
                    propertyMap.AssignFromType = matchingScriptMapping.PropertyType;
                }

                yield return propertyMap;
            }

            yield break;
        }

        public  string GeneratedMappingCode(IEnumerable<PropertyMap> mappings, Type assignFrom, Type assignTo)
        {
            Guard.Argument(mappings).NotNull();
            Guard.Argument(assignFrom).NotNull();
            Guard.Argument(assignTo).NotNull();

            StringBuilder mappingBuilder = new StringBuilder();
            //mappingBuilder.AppendLine($"#r \"{assignTo.Assembly.GetName().Name}.dll\"");
            mappingBuilder.AppendLine($"using TestModel = {assignFrom.Namespace};");
            mappingBuilder.AppendLine($"using PrefabModel = {assignTo.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapInput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            foreach (var mapping in mappings)
            {
                if (!string.IsNullOrEmpty(mapping.AssignFrom) && !string.IsNullOrEmpty(mapping.AssignTo))
                {
                    if (IsSimpleType(mapping.AssignFromType) && mapping.AssignFromType == mapping.AssignToType)
                    {
                        mappingBuilder.AppendLine($"    prefabModel.{mapping.AssignTo} = {mapping.AssignFrom};");
                    }
                    else
                    {
                        mappingBuilder.AppendLine($"    // TODO: Manually map {mapping.AssignFrom} ({mapping.AssignFromType?.Name}) to prefabModel.{mapping.AssignTo} ({mapping.AssignToType?.Name}).");
                    }
                }
                else if(string.IsNullOrEmpty(mapping.AssignFrom) && !string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    // TODO: Manually map a value to prefabModel.{mapping.AssignTo}.");
                }
            }
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"{Environment.NewLine}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapInput(o as PrefabModel.{Constants.PrefabDataModelName})));");

            return mappingBuilder.ToString();
        }

        private static bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
                || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid);
        }

    }
}
