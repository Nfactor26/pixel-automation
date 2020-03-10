using Newtonsoft.Json;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Prefabs
{
    [DataContract]
    [Serializable]
    public class PrefabEntity : Entity
    {
        [DataMember]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember]
        [Browsable(false)]
        public string PrefabId { get; set; }  

        [DataMember]
        [Display(Name = "Prefab Version", Order = 20, GroupName = "Prefab Details")]
        public Version PrefabVersion { get; set; }

        private UseVersion useVersion = UseVersion.Specific;
        [DataMember]
        [Display(Name = "Version To Use", Order = 10, GroupName = "Prefab Details")]
        public UseVersion UseVersion
        {
            get => useVersion;
            set
            {
                if(useVersion != value)
                {
                    useVersion = value;
                    switch (useVersion)
                    {
                        case UseVersion.Deployed:
                            this.SetDispalyAttribute(nameof(PrefabVersion), false);
                            break;
                        case UseVersion.Specific:
                            this.SetDispalyAttribute(nameof(PrefabVersion), true);
                            break;
                    }                   
                    this.prefabDataModel = null;
                }
                OnPropertyChanged(nameof(UseVersion));
                OnPropertyChanged(nameof(PrefabVersion));
            }
        }

        [DataMember]
        [Browsable(false)]
        public List<PropertyMap> InputMapping { get; set; } = new List<PropertyMap>();

        [DataMember]
        [Browsable(false)]
        public List<PropertyMap> OutputMapping { get; set; } = new List<PropertyMap>();


        [IgnoreDataMember]
        [Browsable(false)]
        public Type DataModelType  => this.EntityManager?.Arguments?.GetType() ;

       
        [IgnoreDataMember]
        [Browsable(false)]
        public Type PrefabDataModelType
        {
            get
            {
                if(prefabDataModel == null)
                {
                    LoadPrefab();
                }
                return prefabDataModel.GetType();
            }
        }

        [NonSerialized]
        private object prefabDataModel;

        [IgnoreDataMember] // TODO : Revisit this. Doesn't work with overriden property. 
        [JsonIgnore]
        [Browsable(false)]
        public override List<IComponent> Components
        {
            get => base.Components;
            set => base.Components = value;
        }
               
        public override void BeforeProcess()
        {
            this.LoadPrefab();
         
            object parentDataModel = this.EntityManager.Arguments;
            foreach (var mapping in InputMapping)
            {
                if (string.IsNullOrEmpty(mapping.AssignFrom) || string.IsNullOrEmpty(mapping.AssignTo))
                    continue;
                MethodInfo assignMethod = this.GetType().GetMethod(nameof(AssignFromSourceToTarget), BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo assignMethodWithClosedType = assignMethod.MakeGenericMethod(mapping.PropertyType);
                var value = assignMethodWithClosedType.Invoke(this, new object[] { parentDataModel, this.prefabDataModel, mapping.AssignFrom , mapping.AssignTo });
            }

            base.BeforeProcess();
        }

        private void LoadPrefab()
        {
            IPrefabLoader prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();
            Version prefabVersion = (useVersion == UseVersion.Specific) ? this.PrefabVersion : null;
            var prefabEntity = prefabLoader.LoadPrefab(this.ApplicationId, this.PrefabId, prefabVersion, this.EntityManager);
            this.prefabDataModel = prefabEntity.EntityManager.Arguments;
            this.components.Clear();
            this.components.Add(prefabEntity);           

        }

        public override void OnCompletion()
        {
            object parentDataModel = this.EntityManager.Arguments;

            foreach (var mapping in OutputMapping)
            {
                if (string.IsNullOrEmpty(mapping.AssignFrom) || string.IsNullOrEmpty(mapping.AssignTo))
                    continue;
                MethodInfo assignMethod = this.GetType().GetMethod(nameof(AssignFromSourceToTarget), BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo assignMethodWithClosedType = assignMethod.MakeGenericMethod(mapping.PropertyType);
                var value = assignMethodWithClosedType.Invoke(this, new object[] { this.prefabDataModel, parentDataModel, mapping.AssignFrom, mapping.AssignTo });
            }

            this.Components.Clear();
            this.prefabDataModel = null;

            base.OnCompletion();
        }     
    
        private void AssignFromSourceToTarget<T>(object sourceObject, object targetObject, string sourceProperty, string targetProperty)
        {
            T valueFromSource = GetPropertyValue<T>(sourceObject, sourceProperty);
            SetPropertyValue<T>(targetObject, targetProperty, valueFromSource);
        }

        private T GetPropertyValue<T>(object sourceObject, string propertyName)
        {
            PropertyInfo sourceProperty = sourceObject.GetType().GetProperty(propertyName);
            if (sourceProperty == null)
            {
                throw new ArgumentException($"Property : {propertyName} doesn't exist on DataModel");
            }
           
            //when type can be directly assigned
            if (typeof(T).IsAssignableFrom(sourceProperty.PropertyType))
            {
                T targetPropertyValue = (T)sourceProperty.GetValue(sourceObject);
                return targetPropertyValue;
            }
            throw new Exception($"Data type : {typeof(T)} is not assignable from property {propertyName} on DataModel");
        }

        private void SetPropertyValue<T>(object targetObject, string propertyName, T valueToSet)
        {
            PropertyInfo targetProperty = targetObject.GetType().GetProperty(propertyName);
            if(targetProperty == null)
            {
                throw new ArgumentException($"Property : {propertyName} doesn't exist on DataModel");
            }
            if (targetProperty.PropertyType.IsAssignableFrom(typeof(T)))    //when type can be directly assigned
            {
                targetProperty.SetValue(targetObject, valueToSet);
                return;
            }

            throw new Exception($"Data type : {typeof(T)} is not assignable to property {propertyName} on DataModel");
        }


        #region overridden methods

        public override Entity AddComponent(IComponent component)
        {           
            return this;
        }

        #endregion overridden methods
    }
}
