using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelBuilderViewModel : StagedSmartScreen
    {

        private readonly ICodeGenerator codeGenerator;
        private List<Argument> arguments = new List<Argument>();
        private string generatedCode;


        public BindableCollection<ParameterDescription> RequiredProperties { get; set; } = new BindableCollection<ParameterDescription>();

        public bool HasProperties { get => RequiredProperties.Count() > 0; }

        public BindableCollection<ParameterUsage> ArgumentUsage { get; } = new BindableCollection<ParameterUsage>() { ParameterUsage.Input, ParameterUsage.Output, ParameterUsage.InOut };



        public PrefabDataModelBuilderViewModel(Entity rootEntity, ICodeGenerator codeGenerator)
        {
            Guard.Argument(rootEntity).NotNull();
            Guard.Argument(codeGenerator).NotNull();

            this.codeGenerator = codeGenerator;
            var entityDataModel = rootEntity.EntityManager.Arguments;

            IScriptEngine scriptEngine = rootEntity.EntityManager.GetServiceOfType<IScriptEngine>();

            ExtractArguments(rootEntity);            
            BuildPropertyDescription(scriptEngine, entityDataModel);
            MarkRequiredProperties();
        }

        private void ExtractArguments(Entity entity)
        {
            var allComponents = entity.GetAllComponents();
            foreach(var component in allComponents)
            {
                if(component is GroupEntity groupEntity && (groupEntity.GroupActor != null))
                {
                    AddArgumentsForComponent(groupEntity.GroupActor);
                    continue;
                }
                AddArgumentsForComponent(component);
            }      
            
            void AddArgumentsForComponent(IComponent component)
            {
                foreach (var argument in GetArguments(component))
                {
                    if (argument != null)
                    {
                        arguments.Add(argument);
                    }
                }
            }

            IEnumerable<Argument> GetArguments(IComponent component)
            {
                var componentProperties = component.GetType().GetProperties();
                foreach (var property in componentProperties)
                {
                    if (property.PropertyType.Equals(typeof(Argument)) || property.PropertyType.IsSubclassOf(typeof(Argument)))
                    {
                        yield return property.GetValue(component) as Argument;
                    }
                }
            }
        }

        private void BuildPropertyDescription(IScriptEngine scriptEngine, object dataModel)
        {
            var properties = dataModel.GetType().GetProperties();
            foreach (var property in properties)
            {
                RequiredProperties.Add(new ParameterDescription(property.Name, property.PropertyType));
            }
            
            var scriptVariables = scriptEngine.GetScriptVariables();
            foreach(var scriptVariable in scriptVariables)
            {
                RequiredProperties.Add(new ParameterDescription(scriptVariable.PropertyName, scriptVariable.PropertyType));
            }
        }

        private void MarkRequiredProperties()
        {
            foreach (var argument in arguments)
            {
                if (argument.Mode == ArgumentMode.DataBound)
                {
                    if (!string.IsNullOrEmpty(argument.PropertyPath))
                    {
                        var targetProperty = RequiredProperties.FirstOrDefault(p => p.PropertyName.Equals(argument.PropertyPath));
                        if (targetProperty == null)
                        {
                            //todo : this should not happen. Show as validation error. 
                            continue;
                        }
                        targetProperty.IsRequired = true;

                        if (argument.GetType().Name.Equals(typeof(InArgument<>).Name))
                        {
                            targetProperty.Usage = ParameterUsage.Input;
                        }

                        if (argument.GetType().Name.Equals(typeof(OutArgument<>).Name))
                        {
                            targetProperty.Usage = ParameterUsage.Output;
                        }
                    }
                }
            }
            NotifyOfPropertyChange(() => this.RequiredProperties);
        }
  

        public override bool TryProcessStage(out string errorDescription)
        {
            ClearErrors("");
            if(!RequiredProperties.Any(a => a.IsRequired))
            {
                errorDescription = string.Empty;
                return true;
            }

            var imports = RequiredProperties.Where(r => r.IsRequired).Select(s => s.NameSpace).Distinct();
            imports = imports.Append(typeof(ParameterUsage).Namespace); 
            imports = imports.Append(typeof(ParameterUsageAttribute).Namespace);

            var classBuilder = codeGenerator.CreateClassGenerator("DataModel", "Prefab", imports);
            foreach (var property in RequiredProperties.Where(r => r.IsRequired))
            {
                classBuilder.AddProperty(property.PropertyName, property.PropertyType);
                classBuilder.AddAttribute(property.PropertyName,typeof(ParameterUsage), new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("", $"ParameterUsage.{property.Usage}") });
            }
            if (!classBuilder.HasErrors(out errorDescription))
            {
                this.generatedCode = classBuilder.GetGeneratedCode();
                return true;
            }
            AddOrAppendErrors("", errorDescription);
            return false;
        }

        public override object GetProcessedResult()
        {
            return generatedCode ?? string.Empty;
        }


        public void OnParameterUsageValueChanged(ParameterDescription parameterDescription, SelectionChangedEventArgs e)
        {
            if(Enum.TryParse<ParameterUsage>(e.AddedItems[0]?.ToString(), out ParameterUsage usage))
            {
                parameterDescription.Usage = usage;
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ClearErrors("");
            await base.OnActivateAsync(cancellationToken);
        }

    }
}
