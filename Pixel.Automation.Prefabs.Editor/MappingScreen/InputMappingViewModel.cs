using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pixel.Automation.Prefabs.Editor
{
    public class InputMappingViewModel : Screen
    {
        private Type assignFrom;
        private Type assignTo;
        private IScriptEngine scriptEngine;

        public BindableCollection<PropertyMapViewModel> Mappings { get; set; } = new BindableCollection<PropertyMapViewModel>();

        public InputMappingViewModel(EntityManager entityManager, IEnumerable<PropertyMap> existingMapping, Type assignFrom, Type assignTo)
        {
            this.assignFrom = assignFrom;
            this.assignTo = assignTo;
            scriptEngine = entityManager.GetServiceOfType<IScriptEngine>();
            if (existingMapping.Count() >0 )
            {
                RefreshMapping(existingMapping, assignFrom);
            }
            else
            {
                GenerateMapping(assignFrom, assignTo);
            }
        }    


        /// <summary>
        /// Data model might have changed , lookup for new possible drop down values that should be available in PropertyMapViewModel for a given property
        /// </summary>
        /// <param name="existingMapping"></param>
        /// <param name="assignFrom"></param>      
        private void RefreshMapping(IEnumerable<PropertyMap> existingMapping, Type assignFrom)
        {
            var assignFromProperties = assignFrom.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();          
            foreach (var propertyMap in existingMapping)
            {
                //Possible mappings can be for properties available in data model and variables available in script engine
                var possibleMappings = assignFromProperties.Where(t => t.PropertyType.Equals(propertyMap.PropertyType)).Select(p => p.Name);
                possibleMappings = possibleMappings.Union(scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.PropertyType)).Select(p => p.PropertyName));
               
                var propertyMapViewModel = new PropertyMapViewModel() { PropertyMap = propertyMap, PossibleMaps = new BindableCollection<string>(possibleMappings) };                
                Mappings.Add(propertyMapViewModel);
            }
        }


        /// <summary>
        /// Map properties from AssignFrom to AssignTo if property type and name matches
        /// </summary>
        /// <param name="assignFrom"></param>
        /// <param name="assignTo"></param>
        private void GenerateMapping(Type assignFrom, Type assignTo)
        {           
            var assignFromProperties = assignFrom.GetProperties();
            var scriptVariables = scriptEngine.GetScriptVariables();
            var assignToProperties = assignTo.GetProperties().Where(a =>
            {
                var attribute = a.GetCustomAttributes(typeof(ParameterUsageAttribute)).FirstOrDefault();
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
                var propertyMap = new PropertyMap() { AssignTo = property.Name, PropertyType = property.PropertyType, AssignFrom = string.Empty };
             
                var possibleMappings = assignFromProperties.Where(t => t.PropertyType.Equals(property.PropertyType)).Select(p => p.Name);
                possibleMappings = possibleMappings.Union(scriptVariables.Where(s => s.PropertyType.Equals(propertyMap.PropertyType)).Select(p => p.PropertyName));

                propertyMap.AssignFrom = possibleMappings.FirstOrDefault(p => p.Equals(property.Name)) ?? string.Empty;
                var propertyMapViewModel = new PropertyMapViewModel() { PropertyMap = propertyMap, PossibleMaps = new BindableCollection<string>(possibleMappings) };
                Mappings.Add(propertyMapViewModel);
            }
        }

        //TODO : Clear mapping and generate mapping again. Feature is required probably because for some reason prefab data model has changed and user needs to redo mapping
        public void Reload()
        {
            this.Mappings.Clear();
            GenerateMapping(this.assignFrom, this.assignTo);
        }

        public IEnumerable<PropertyMap> GetConfiguredMapping()
        {
            return Mappings.Select(s => s.PropertyMap);
        }

        public async void Save()
        {
            await this.TryCloseAsync(true);
        }

        public async void Cancel()
        {
           await this.TryCloseAsync(false);
        }
    }
}
