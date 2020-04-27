using Newtonsoft.Json;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Prefabs
{
    [DataContract]
    [Serializable]
    [Scriptable("InputMappingScript", "OutputMappingScript")]
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
        public PrefabVersion PrefabVersion { get; set; }

        [DataMember]
        [Browsable(false)]
        public string InputMappingScript { get; set; }

        [DataMember]
        [Browsable(false)]
        public string OutputMappingScript { get; set; }

        [IgnoreDataMember]
        [Browsable(false)]
        public Type DataModelType => this.EntityManager?.Arguments?.GetType();


        [IgnoreDataMember]
        [Browsable(false)]
        public Type PrefabDataModelType
        {
            get
            {
                if (prefabDataModel == null)
                {
                    LoadPrefab();
                }
                return prefabDataModel.GetType();
            }
        }

        [NonSerialized]
        private object prefabDataModel;

        [NonSerialized]
        private Entity prefabEntity;

        [IgnoreDataMember] // TODO : Revisit this. Doesn't work with overriden property. 
        [JsonIgnore]
        [Browsable(false)]
        public override List<IComponent> Components
        {
            get => base.Components;
            protected set => base.Components = value;
        }
               
        public override async void BeforeProcess()
        {
            this.LoadPrefab();
          
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
            var inputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.InputMappingScript);
            inputMappingAction.Invoke(prefabDataModel);       
          
            base.BeforeProcess();
        }

        private void LoadPrefab()
        {
            IPrefabLoader prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();        
            this.prefabEntity = prefabLoader.LoadPrefab(this.ApplicationId, this.PrefabId, this.PrefabVersion, this.EntityManager);
            this.prefabDataModel = this.prefabEntity.EntityManager.Arguments;
            this.components.Clear();
            this.components.Add(this.prefabEntity);          
        }

        public override async void OnCompletion()
        {           
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
            var outputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.OutputMappingScript);
            outputMappingAction.Invoke(prefabDataModel);

            this.Components.Clear();
            this.prefabDataModel = null;

            base.OnCompletion();
        }

        #region overridden methods

        public override Entity AddComponent(IComponent component)
        {           
            return this;
        }

        public override void ResolveDependencies()
        {
            var fileSystem = this.EntityManager.GetServiceOfType<IFileSystem>();
            this.InputMappingScript = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid().ToString()}.csx"));
            this.OutputMappingScript = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid().ToString()}.csx"));
        }

        #endregion overridden methods
    }
}
