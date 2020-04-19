using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
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
        private readonly PrefabDescription prefabDescription;
        private readonly ICodeGenerator codeGenerator;
        private readonly IPrefabFileSystem fileSystem;
        private readonly ICompositeTypeExtractor compositeTypeExtractor;
        private readonly object currentDataModel;
        private List<Argument> arguments = new List<Argument>();
        private string generatedCode;

        public BindableCollection<ParameterDescription> RequiredProperties { get; set; } = new BindableCollection<ParameterDescription>();

        public bool HasProperties { get => RequiredProperties.Count() > 0; }

        public BindableCollection<ParameterUsage> ArgumentUsage { get; } = new BindableCollection<ParameterUsage>() { ParameterUsage.Input, ParameterUsage.Output, ParameterUsage.InOut };
               
        public PrefabDataModelBuilderViewModel(PrefabDescription prefabDescription, ICodeGenerator codeGenerator,
            IPrefabFileSystem fileSystem, IScriptEngine scriptEngine,
            ICompositeTypeExtractor compositeTypeExtractor, IArgumentExtractor argumentExtractor)
        {
            Guard.Argument(prefabDescription).NotNull();
            Guard.Argument(codeGenerator).NotNull();
            Guard.Argument(fileSystem).NotNull();
            Guard.Argument(compositeTypeExtractor).NotNull();
            Guard.Argument(argumentExtractor).NotNull();
            Guard.Argument(scriptEngine).NotNull();

            this.prefabDescription = prefabDescription;
            this.codeGenerator = codeGenerator;
            this.fileSystem = fileSystem;
            this.compositeTypeExtractor = compositeTypeExtractor;

            var rootEntity = prefabDescription.PrefabRoot as Entity;
            currentDataModel = rootEntity.EntityManager.Arguments;
        
            arguments.AddRange(argumentExtractor.ExtractArguments(rootEntity));   
        
            BuildPropertyDescription(scriptEngine, currentDataModel);
            MarkRequiredProperties();
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

            IEnumerable<Type> requiredTypes = new List<Type>();          
            foreach(var property in RequiredProperties.Where(r => r.IsRequired))
            {
                //We need to generate only complex types defined in Data model assembly
                if(property.PropertyType.Assembly.Equals(this.currentDataModel.GetType().Assembly))
                {
                    var typeDependencies = compositeTypeExtractor.GetCompositeTypes(property.PropertyType);
                    requiredTypes = requiredTypes.Union(typeDependencies);
                }                        
            }
            foreach(var type in requiredTypes)
            {
                MirrorTargetType(type);
            }

            var imports = RequiredProperties.Where(r => r.IsRequired).Select(s => s.NameSpace).Distinct().Except(new[] { "Pixel.Automation.Project.DataModels" });
            imports = imports.Append(typeof(ParameterUsage).Namespace); 
            imports = imports.Append(typeof(ParameterUsageAttribute).Namespace);

            var classBuilder = codeGenerator.CreateClassGenerator(Constants.PrefabDataModelName, prefabDescription.NameSpace, imports);
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

        private void MirrorTargetType(Type targetType)
        {
            var imports = targetType.GetProperties().Select(s => s.PropertyType.Namespace).Distinct();
            string generatedCode = codeGenerator.GenerateClassForType(targetType, prefabDescription.NameSpace, imports);
            fileSystem.CreateOrReplaceFile(fileSystem.DataModelDirectory, $"{targetType.GetDisplayName()}.cs", generatedCode);
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
