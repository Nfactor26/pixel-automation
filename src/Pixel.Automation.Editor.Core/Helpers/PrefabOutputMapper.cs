using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Editor.Core.Helpers
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
            mappingBuilder.AppendLine($"#r \"{assignFrom.Assembly.GetName().Name}.dll\"");
            mappingBuilder.AppendLine($"#r \"AutoMapper.dll\" {Environment.NewLine}");
            mappingBuilder.AppendLine($"using AutoMapper;");
            mappingBuilder.AppendLine($"using TestModel = {assignTo.Namespace};");
            mappingBuilder.AppendLine($"using PrefabModel = {assignFrom.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapOutput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            if (mappings.Any(m => !m.AssignFromType.Equals(m.AssignToType)))
            {
                mappingBuilder.AppendLine("     var configuration = new MapperConfiguration(cfg => ");
                mappingBuilder.AppendLine("     {");
                foreach (var mapping in mappings.Where(m => m.AssignToType != null && !m.AssignFromType.Equals(m.AssignToType)))
                {
                    GenerateMap(mappingBuilder, mapping);
                }
                mappingBuilder.AppendLine("     });");
                mappingBuilder.AppendLine("     var mapper = configuration.CreateMapper();");
                foreach (var mapping in mappings.Where(m => m.AssignToType != null &&  !m.AssignFromType.Equals(m.AssignToType)))
                {
                    AssignMap(mappingBuilder, mapping);
                }
            }
            foreach (var mapping in mappings)
            {
                if ((mapping.AssignToType != null) && (!mapping.AssignFromType.Equals(mapping.AssignToType)))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(mapping.AssignFrom) && !string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    {mapping.AssignTo} = prefabModel.{mapping.AssignFrom}");
                }
                else if(!string.IsNullOrEmpty(mapping.AssignFrom) && string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    //? = prefabModel.{mapping.AssignFrom};");
                }
            }
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapOutput(o as PrefabModel.{Constants.PrefabDataModelName})));");

            return mappingBuilder.ToString();
        }

        /// <summary>
        /// generate code to create automapper map configuration between source and destination types
        /// </summary>
        /// <param name="mappingBuilder"></param>
        /// <param name="mapping"></param>
        internal static void GenerateMap(StringBuilder mappingBuilder, PropertyMap mapping)
        {
            if (IsGenericIEnumerable(mapping.AssignFromType) && IsGenericIEnumerable(mapping.AssignToType))
            {
                var assignFromGenericArguments = mapping.AssignFromType.GetGenericArguments();
                var assignToGenericArguments = mapping.AssignToType.GetGenericArguments();
                if (assignFromGenericArguments.Count() == 1 && assignFromGenericArguments.Count() == 1)
                {
                    mappingBuilder.AppendLine($"        cfg.CreateMap<PrefabModel.{assignFromGenericArguments[0].Name}, TestModel.{assignToGenericArguments[0].Name}>();");
                }
            }
            else if (mapping.AssignFromType.IsArray && mapping.AssignToType.IsArray)
            {
                mappingBuilder.AppendLine($"        cfg.CreateMap<PrefabModel.{mapping.AssignFromType.GetElementType().Name}, TestModel.{mapping.AssignToType.GetElementType().Name}>();");
            }
            else if (!mapping.AssignFromType.IsGenericType && !mapping.AssignToType.IsGenericType)
            {
                mappingBuilder.AppendLine($"        cfg.CreateMap<PrefabModel.{mapping.AssignFromType.Name}, TestModel.{mapping.AssignToType.Name}>();");
            }
        }

        /// <summary>
        /// Generate code to assign source property to target property using automapper map
        /// </summary>
        /// <param name="mappingBuilder"></param>
        /// <param name="mapping"></param>
        internal static void AssignMap(StringBuilder mappingBuilder, PropertyMap mapping)
        {
            if (IsGenericIEnumerable(mapping.AssignFromType) && IsGenericIEnumerable(mapping.AssignToType))
            {
                var assignFromGenericArguments = mapping.AssignFromType.GetGenericArguments();
                var assignToGenericArguments = mapping.AssignToType.GetGenericArguments();
                if (assignFromGenericArguments.Count() == 1 && assignFromGenericArguments.Count() == 1)
                {
                    string fromType = $"{mapping.AssignFromType.Name.Split('`')[0]}<PrefabModel.{assignFromGenericArguments[0].Name}>";
                    string toType = $"{mapping.AssignToType.Name.Split('`')[0]}<TestModel.{assignToGenericArguments[0].Name}>";
                    mappingBuilder.AppendLine($"    {mapping.AssignTo} = mapper.Map<{fromType}, {toType}>(prefabModel.{mapping.AssignFrom});");
                }
            }
            else if (mapping.AssignFromType.IsArray && mapping.AssignToType.IsArray)
            {
                mappingBuilder.AppendLine($"    {mapping.AssignTo} = mapper.Map<{mapping.AssignFromType.GetElementType().Name}[], {mapping.AssignToType.GetElementType().Name}[]>(prefabModel.{mapping.AssignFrom});");
            }
            else if (!mapping.AssignFromType.IsGenericType && !mapping.AssignToType.IsGenericType)
            {
                mappingBuilder.AppendLine($"    {mapping.AssignTo} = mapper.Map<{mapping.AssignFromType.Name}, {mapping.AssignToType.Name}>(prefabModel.{mapping.AssignFrom});");
            }
        }

        internal static bool IsGenericIEnumerable(Type type)
        {
            return type.IsGenericType && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }
    }
}
