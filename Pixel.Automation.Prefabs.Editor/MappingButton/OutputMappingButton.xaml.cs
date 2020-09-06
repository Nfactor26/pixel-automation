using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Pixel.Automation.Prefabs.Editor
{
    /// <summary>
    /// Interaction logic for OutputMappingButton.xaml
    /// </summary>
    public partial class OutputMappingButton : MappingButton
    {        
        public OutputMappingButton()
        {
            InitializeComponent();            
        }

        private async void OpenMappingWindow(object sender, RoutedEventArgs e)
        {
            var propertyMappings = GenerateMapping(this.AssignFrom, this.AssignTo);
            string generatedCode = GeneratedMappingCode(propertyMappings);
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            IScriptEditorFactory editorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            IScriptEditorScreen scriptEditor = editorFactory.CreateScriptEditor();
            if (OwnerComponent.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
            {
                //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                editorFactory.AddProject(OwnerComponent.Id, new string[] { testCaseEntity.Tag }, EntityManager.Arguments.GetType());
            }
            else
            {
                editorFactory.AddProject(OwnerComponent.Id, Array.Empty<string>(), EntityManager.Arguments.GetType());
            }
            editorFactory.AddDocument(ScriptFile, OwnerComponent.Id, generatedCode);
            scriptEditor.OpenDocument(this.ScriptFile, OwnerComponent.Id, generatedCode);
            await windowManager.ShowDialogAsync(scriptEditor);

        }

        private IEnumerable<PropertyMap> GenerateMapping(Type assignFrom, Type assignTo)
        {
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var assignToProperties = assignTo.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();
            var assignFromProperties = assignFrom.GetProperties().Where(a =>
            {
                var attribute = a.GetCustomAttributes(typeof(ParameterUsageAttribute),false).FirstOrDefault();
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
                var propertyMap = new PropertyMap() { AssignFrom = property.Name, AssignToType = property.PropertyType };

                //Possible mappings can be for properties available in data model and variables available in script engine
                var possibleMappings = assignToProperties.Where(t => t.PropertyType.Equals(property.PropertyType)).Select(p => p.Name);
                possibleMappings = possibleMappings.Union(scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.AssignToType)).Select(p => p.PropertyName));

                //default item that should be picked in drop down
                propertyMap.AssignTo = possibleMappings.FirstOrDefault(p => p.Equals(property.Name)) ?? string.Empty;
                yield return propertyMap;
            }
            
            yield break;
        }



        private string GeneratedMappingCode(IEnumerable<PropertyMap> mappings)
        {

            StringBuilder mappingBuilder = new StringBuilder();
            mappingBuilder.AppendLine($"#r \"{this.PrefabVersion.DataModelAssembly}\"");
            mappingBuilder.AppendLine($"#r \"AutoMapper.dll\" {Environment.NewLine}");
            mappingBuilder.AppendLine($"using AutoMapper;");
            mappingBuilder.AppendLine($"using TestModel = Pixel.Automation.Project.DataModels;");
            mappingBuilder.AppendLine($"using PrefabModel = {this.AssignFrom.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapOutput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            if (mappings.Any(m => !m.AssignFromType.Equals(m.AssignToType)))
            {
                mappingBuilder.AppendLine("     var configuration = new MapperConfiguration(cfg => ");
                mappingBuilder.AppendLine("     {");
                foreach (var mapping in mappings.Where(m => !m.AssignToType.Equals(m.AssignFromType)))
                {
                    mappingBuilder.AppendLine($"        cfg.CreateMap<TestModel.{mapping.AssignFrom}, PrefabModel.{mapping.AssignTo}>();");
                }
                mappingBuilder.AppendLine("     });");
                mappingBuilder.AppendLine("     var mapper = configuration.CreateMapper();");
                foreach (var mapping in mappings.Where(m => !m.AssignToType.Equals(m.AssignFromType)))
                {
                    if (mapping.AssignToType.IsGenericType || mapping.AssignToType.IsArray)
                    {
                        mappingBuilder.AppendLine($"    //prefabModel.{mapping.AssignTo} = mapper.Map<FromType?,ToType?>({mapping.AssignFrom})");
                    }
                    else
                    {
                        mappingBuilder.AppendLine($"    prefabModel.{mapping.AssignTo} = mapper.Map<PrefabModel.{mapping.AssignTo}>({mapping.AssignFrom})");
                    }
                }
            }
            foreach (var mapping in mappings)
            {
                if (!mapping.AssignToType.Equals(mapping.AssignFromType))
                {
                    continue;
                }
                 if (!string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    {mapping.AssignTo} = prefabModel.{mapping.AssignFrom}");
                }
                else
                {
                    mappingBuilder.AppendLine($"    //? = prefabModel.{mapping.AssignFrom}");
                }
            }           
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapOutput(o as PrefabModel.{Constants.PrefabDataModelName})));");

            return mappingBuilder.ToString();         
        }
    }
}

