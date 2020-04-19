using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Extensions;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Pixel.Automation.Prefabs.Editor
{
    /// <summary>
    /// Interaction logic for InputMappingButton.xaml
    /// </summary>
    public partial class InputMappingButton : MappingButton
    {
        public InputMappingButton()
        {
            InitializeComponent();            
        }

        private  async void OpenMappingWindow(object sender, RoutedEventArgs e)
        {
            var propertyMappings = GenerateMapping(this.AssignFrom, this.AssignTo).ToList();
            string generatedCode = GeneratedMappingCode(propertyMappings);
           
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            IScriptEditorFactory scriptEditorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            IScriptEditorScreen scriptEditor = scriptEditorFactory.CreateScriptEditor();
            scriptEditor.OpenDocument(this.ScriptFile, generatedCode);
            await windowManager.ShowDialogAsync(scriptEditor);          

        }

        private IEnumerable<PropertyMap> GenerateMapping(Type assignFrom, Type assignTo)
        {
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
            var assignFromProperties = assignFrom.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();
            var assignToProperties = assignTo.GetProperties().Where(a =>
            {
                var attribute = a.GetCustomAttributes(typeof(ParameterUsageAttribute),false).FirstOrDefault();
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

                //pick if the name type matches or name matches
                var possibleMappings = assignFromProperties.Where(t => t.PropertyType.Equals(property.PropertyType) || t.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));
                var matchingMapping = possibleMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.Equals(property.Name))
                                           ?? possibleMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) && p.Name.Equals(property.Name));
                if (matchingMapping != null)
                {
                    propertyMap.AssignFrom = matchingMapping.Name;
                    propertyMap.AssignFromType = matchingMapping.PropertyType;
                }

                var possibleScriptVariableMappings = scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.AssignToType) || s.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()));
                var matchingScriptMapping = possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.Equals(property.PropertyType) && p.PropertyName.Equals(property.Name))
                                         ?? possibleScriptVariableMappings.FirstOrDefault(p => p.PropertyType.GetDisplayName().Equals(property.PropertyType.GetDisplayName()) 
                                         && p.PropertyName.Equals(property.Name));
                if (matchingScriptMapping != null)
                {
                    propertyMap.AssignFrom = matchingScriptMapping.PropertyName;
                    propertyMap.AssignFromType = matchingScriptMapping.PropertyType;
                }

                yield return propertyMap;
            }

            yield break;
        }

      

        private string GeneratedMappingCode(IEnumerable<PropertyMap> mappings)
        {
            StringBuilder mappingBuilder = new StringBuilder();
            mappingBuilder.AppendLine($"#r \"{this.PrefabVersion.PrefabAssembly}\"");
            mappingBuilder.AppendLine($"#r \"AutoMapper.dll\" {Environment.NewLine}");
            mappingBuilder.AppendLine($"using AutoMapper;");
            mappingBuilder.AppendLine($"using TestModel = Pixel.Automation.Project.DataModels;");
            mappingBuilder.AppendLine($"using PrefabModel = {this.AssignTo.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapInput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            if(mappings.Any(m => (m.AssignFromType != null) && (!m.AssignFromType.Equals(m.AssignToType))))
            {
                mappingBuilder.AppendLine("     var configuration = new MapperConfiguration(cfg => ");
                mappingBuilder.AppendLine("     {");
                foreach(var mapping in mappings.Where(m => !m.AssignToType.Equals(m.AssignFromType)))
                {
                    mappingBuilder.AppendLine($"        cfg.CreateMap<TestModel.{mapping.AssignFrom}, PrefabModel.{mapping.AssignTo}>();");
                }
                mappingBuilder.AppendLine("     });");
                mappingBuilder.AppendLine("     var mapper = configuration.CreateMapper();");
                foreach (var mapping in mappings.Where(m => !m.AssignToType.Equals(m.AssignFromType)))
                {
                    if(mapping.AssignToType.IsGenericType || mapping.AssignToType.IsArray)
                    {
                        mappingBuilder.AppendLine($"    //prefabModel.{mapping.AssignTo} = mapper.Map<FromType?,ToType?>({mapping.AssignFrom});");
                    }
                    else
                    {
                        mappingBuilder.AppendLine($"    prefabModel.{mapping.AssignTo} = mapper.Map<PrefabModel.{mapping.AssignTo}>({mapping.AssignFrom});");
                    }
                }
            }          
            foreach (var mapping in mappings)
            {
                if((mapping.AssignFromType != null) && (!mapping.AssignToType.Equals(mapping.AssignFromType)))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(mapping.AssignFrom))
                {
                    mappingBuilder.AppendLine($"    prefabModel.{mapping.AssignTo} = {mapping.AssignFrom};");
                }
                else
                {
                    mappingBuilder.AppendLine($"    //prefabModel.{mapping.AssignTo} = default;");
                }
            }
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"{Environment.NewLine}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapInput(o as PrefabModel.{Constants.PrefabDataModelName})));");  
            
            return  mappingBuilder.ToString();
        }
    }
}
