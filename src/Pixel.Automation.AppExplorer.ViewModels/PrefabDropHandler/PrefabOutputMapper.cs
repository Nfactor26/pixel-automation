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
    /// Target type is the output for prefab and source type is the model used within prefab.
    /// </summary>
    public class PrefabOutputMapper : IPrefabArgumentMapper
    {       
        public IEnumerable<PropertyMap> GenerateMapping(IScriptEngine scriptEngine, Type assignFrom, Type assignTo)
        {
            Guard.Argument(scriptEngine).NotNull();
            Guard.Argument(assignFrom).NotNull();
            Guard.Argument(assignTo).NotNull();

            var assignToProperties = assignTo.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();
            var assignFromProperties = assignFrom.GetProperties().Where(a =>
            {
                var attribute = a.GetCustomAttributes(typeof(ParameterUsageAttribute), false).FirstOrDefault();
                if (attribute != null)
                {
                    if (attribute is ParameterUsageAttribute parameterUsageAttribute)
                    {
                        if (parameterUsageAttribute.ParameterUsage == ParameterUsage.Output || parameterUsageAttribute.ParameterUsage == ParameterUsage.InOut)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });

            foreach (var property in assignFromProperties)
            {
                var propertyMap = new PropertyMap() { AssignFrom = property.Name, AssignFromType = property.PropertyType };

                //pick if the name type matches or type display name matches
                var possibleMappings = assignToProperties.Where(t => t.PropertyType.Equals(property.PropertyType) || t.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));
                
                var matchingMapping = possibleMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.Equals(property.Name))
                                        ?? possibleMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) && p.Name.Equals(property.Name));

                if (matchingMapping != null)
                {
                    propertyMap.AssignTo = matchingMapping.Name;
                    propertyMap.AssignToType = matchingMapping.PropertyType;
                }

                var possibleScriptVariableMappings = scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.AssignToType) || s.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));

                var matchingScriptMapping = possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.PropertyName.Equals(property.Name)) ?? possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) && p.PropertyName.Equals(property.Name));

                if (matchingScriptMapping != null)
                {
                    propertyMap.AssignTo = matchingScriptMapping.PropertyName;
                    propertyMap.AssignToType = matchingScriptMapping.PropertyType;
                }

                yield return propertyMap;
            }

            yield break;
        }

        public string GeneratedMappingCode(IEnumerable<PropertyMap> mappings, Type assignFrom, Type assignTo)
        {
            Guard.Argument(mappings).NotNull();
            Guard.Argument(assignFrom).NotNull();
            Guard.Argument(assignTo).NotNull();

            StringBuilder mappingBuilder = new StringBuilder();
            //mappingBuilder.AppendLine($"#r \"{assignFrom.Assembly.GetName().Name}.dll\"");
            mappingBuilder.AppendLine($"using TestModel = {assignTo.Namespace};");
            mappingBuilder.AppendLine($"using PrefabModel = {assignFrom.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapOutput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            foreach (var mapping in mappings)
            {
                if (!string.IsNullOrEmpty(mapping.AssignFrom) && !string.IsNullOrEmpty(mapping.AssignTo))
                {
                    if (IsSimpleType(mapping.AssignFromType) && mapping.AssignFromType == mapping.AssignToType)
                    {
                        mappingBuilder.AppendLine($"    {mapping.AssignTo} = prefabModel.{mapping.AssignFrom};");
                    }
                    else
                    {
                        mappingBuilder.AppendLine($"    // TODO: Manually map prefabModel.{mapping.AssignFrom} ({mapping.AssignFromType?.Name}) to {mapping.AssignTo} ({mapping.AssignToType?.Name}).");
                    }
                }
                else if(!string.IsNullOrEmpty(mapping.AssignFrom) && string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    // TODO: Manually map prefabModel.{mapping.AssignFrom} to a destination value.");
                }
            }
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapOutput(o as PrefabModel.{Constants.PrefabDataModelName})));");

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
