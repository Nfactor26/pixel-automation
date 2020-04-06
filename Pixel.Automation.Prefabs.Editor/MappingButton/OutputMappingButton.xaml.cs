using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
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
            IScriptEditorFactory scriptEditorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
            IScriptEditorScreen scriptEditor = scriptEditorFactory.CreateScriptEditor();
            scriptEditor.OpenDocument(this.ScriptFile, generatedCode);
            await windowManager.ShowDialogAsync(scriptEditor);

        }

        private IEnumerable<PropertyMap> GenerateMapping(Type assignFrom, Type assignTo)
        {
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
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
                var propertyMap = new PropertyMap() { AssignFrom = property.Name, PropertyType = property.PropertyType };

                //Possible mappings can be for properties available in data model and variables available in script engine
                var possibleMappings = assignToProperties.Where(t => t.PropertyType.Equals(property.PropertyType)).Select(p => p.Name);
                possibleMappings = possibleMappings.Union(scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.PropertyType)).Select(p => p.PropertyName));

                //default item that should be picked in drop down
                propertyMap.AssignTo = possibleMappings.FirstOrDefault(p => p.Equals(property.Name)) ?? string.Empty;
                yield return propertyMap;
            }
            
            yield break;
        }



        private string GeneratedMappingCode(IEnumerable<PropertyMap> mappings)
        {

            StringBuilder mappingBuilder = new StringBuilder();
            mappingBuilder.AppendLine($"#r \"{this.PrefabVersion.PrefabAssembly}\" {Environment.NewLine}");
            mappingBuilder.AppendLine($"using TestModel = Pixel.Automation.Project.DataModels;");
            mappingBuilder.AppendLine($"using PrefabModel = {this.AssignFrom.Namespace};{Environment.NewLine}");
            mappingBuilder.AppendLine($"void MapOutput(PrefabModel.{Constants.PrefabDataModelName} prefabModel)");
            mappingBuilder.AppendLine("{");
            mappingBuilder.AppendLine($"    //Set values on Prefab data model from test data model and script variables");
            foreach (var mapping in mappings)
            {
                if (!string.IsNullOrEmpty(mapping.AssignTo))
                {
                    mappingBuilder.AppendLine($"    {mapping.AssignTo} = prefabModel.{mapping.AssignFrom}");
                }
                else
                {
                    mappingBuilder.AppendLine($"    //{mapping.AssignTo} = prefabModel.{mapping.AssignFrom}");
                }
            }
            mappingBuilder.AppendLine("}");
            mappingBuilder.AppendLine($"return (new Action<object>(o => MapOutput(o as PrefabModel.{Constants.PrefabDataModelName})));");

            return mappingBuilder.ToString();         
        }
    }
}

