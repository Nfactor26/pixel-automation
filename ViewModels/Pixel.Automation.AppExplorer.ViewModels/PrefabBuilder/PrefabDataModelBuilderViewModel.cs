using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelBuilderViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<PrefabDataModelBuilderViewModel>();

        private readonly PrefabDescription prefabDescription;
        private readonly ICodeGenerator codeGenerator;
        private readonly IScriptEngine scriptEngine;
        private readonly IArgumentExtractor argumentExtractor;
        private readonly IPrefabFileSystem fileSystem;
        private readonly ICompositeTypeExtractor compositeTypeExtractor;
        private readonly object currentDataModel;
        private List<Argument> arguments = new List<Argument>();
        private string generatedCode;

        public BindableCollection<ParameterDescription> RequiredProperties { get; set; } = new BindableCollection<ParameterDescription>();

        public bool HasProperties { get => RequiredProperties.Count() > 0; }

        public BindableCollection<ParameterUsage> ArgumentUsage { get; } = new BindableCollection<ParameterUsage>() { ParameterUsage.Input, ParameterUsage.Output, ParameterUsage.InOut };
               
        public PrefabDataModelBuilderViewModel(PrefabDescription prefabDescription, ICodeGenerator codeGenerator,
            IPrefabFileSystem fileSystem, IScriptEngine scriptEngine, ICompositeTypeExtractor compositeTypeExtractor, IArgumentExtractor argumentExtractor)
        {
            Guard.Argument(prefabDescription).NotNull();
            Guard.Argument(codeGenerator).NotNull();
            Guard.Argument(fileSystem).NotNull();
            Guard.Argument(compositeTypeExtractor).NotNull();
            Guard.Argument(argumentExtractor).NotNull();
            Guard.Argument(scriptEngine).NotNull();

            this.prefabDescription = prefabDescription;
            this.codeGenerator = codeGenerator;
            this.scriptEngine = scriptEngine;
            this.fileSystem = fileSystem;
            this.argumentExtractor = argumentExtractor;
            this.compositeTypeExtractor = compositeTypeExtractor;

            var rootEntity = prefabDescription.PrefabRoot as Entity;
            currentDataModel = rootEntity.EntityManager.Arguments;        
        }            

        public override bool TryProcessStage(out string errorDescription)
        {
            ClearErrors("");
            if(!RequiredProperties.Any(a => a.IsRequired))
            {
                errorDescription = string.Empty;
                logger.Information("None of the properties were marked as required proerties");               
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

            logger.Information($"Generated {requiredTypes.Count()} types required by Prefab Data Model");
           
            logger.Information("Start creating Prefab data model");

            var imports = RequiredProperties.Where(r => r.IsRequired).Select(s => s.NameSpace)?
                .Distinct().Except(new[] { prefabDescription.NameSpace }) ?? new List<string>();
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
                logger.Information($"Created prefab data model class : {Constants.PrefabDataModelName} with {RequiredProperties.Count()} properties");
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
            logger.Information($"Mirror type created for type {targetType.GetDisplayName()}");
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
            logger.Information($"Activate screen is {nameof(PrefabDataModelBuilderViewModel)}");
            ClearErrors("");          
            RequiredProperties.Clear();
            arguments.AddRange(argumentExtractor.ExtractArguments(prefabDescription.PrefabRoot as Entity));
            BuildPropertyDescription(scriptEngine, currentDataModel);
            MarkRequiredProperties();
            await base.OnActivateAsync(cancellationToken);
        }

        private void BuildPropertyDescription(IScriptEngine scriptEngine, object dataModel)
        {
            var properties = dataModel.GetType().GetProperties();
            logger.Information($"Identified {properties.Count()} properties on data model : {dataModel.GetType().Name}");
            foreach (var property in properties)
            {
                RequiredProperties.Add(new ParameterDescription(property.Name, property.PropertyType));
            }

            var scriptVariables = scriptEngine.GetScriptVariables();
            logger.Information($"Identified {scriptVariables.Count()} variables declared in script engine");
            foreach (var scriptVariable in scriptVariables)
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
                            Debug.Assert(false, "Argument PropertyPath points to a non-existent property / variable");
                            logger.Warning($"Argument PropertyPath points to a non-existent property / variable");
                            continue;
                        }
                        targetProperty.IsRequired = true;

                        if (argument.GetType().Name.Equals(typeof(InArgument<>).Name))
                        {
                            targetProperty.Usage = ParameterUsage.Input;
                            logger.Information($"{targetProperty.PropertyName} marked as Input parameter");
                        }

                        if (argument.GetType().Name.Equals(typeof(OutArgument<>).Name))
                        {
                            targetProperty.Usage = ParameterUsage.Output;
                            logger.Information($"{targetProperty.PropertyName} marked as Output parameter");
                        }
                    }
                }
            }
            NotifyOfPropertyChange(() => this.RequiredProperties);
            logger.Information("Identifying properties as Input/Output properties completed");
        }


        public override void OnPreviousScreen()
        {
            ClearErrors("");
            RequiredProperties.Clear();
            arguments.Clear();
            base.OnPreviousScreen();
        }

    }
}
