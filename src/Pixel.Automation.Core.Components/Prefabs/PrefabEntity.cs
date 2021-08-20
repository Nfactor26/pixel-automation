using Newtonsoft.Json;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Prefabs
{
    [DataContract]
    [Serializable]
    [Scriptable(nameof(InputMappingScriptFile), nameof(OutputMappingScriptFile))]
    [Initializer(typeof(ScriptFileInitializer))]
    public class PrefabEntity : Entity
    {
        private readonly ILogger logger = Log.ForContext<PrefabEntity>();
        [DataMember]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember]
        [Browsable(false)]    
        public string PrefabId { get; set; }

        private string inputMappingScriptFile;
        [DataMember]
        [DisplayName("Input Mapping Script")]
        public string InputMappingScriptFile
        {
            get => this.inputMappingScriptFile;
            set
            {
                this.inputMappingScriptFile = value;
                OnPropertyChanged();
            }
        }

        private string outputMappingScriptFile;
        [DataMember]
        [DisplayName("Output Mapping Script")]
        public string OutputMappingScriptFile
        {
            get => this.outputMappingScriptFile;
            set
            {
                this.outputMappingScriptFile = value;
                OnPropertyChanged();
            }
           
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public Type DataModelType => this.EntityManager?.Arguments?.GetType();


        [IgnoreDataMember]
        [Browsable(false)]
        public Type PrefabDataModelType
        {
            get
            {
                if (this.prefabLoader == null)
                {
                    this.prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();
                }
                return this.prefabLoader.GetPrefabDataModelType(this.ApplicationId, this.PrefabId, this.EntityManager);
            }
        }

        [NonSerialized]
        private IPrefabLoader prefabLoader;

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
               
        public PrefabEntity() : base("Prefab Entity", "PrefabEntity")
        {

        }

        public override async Task BeforeProcessAsync()
        {
            this.LoadPrefab();
          
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var inputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.InputMappingScriptFile);
            inputMappingAction.Invoke(prefabDataModel);
            logger.Information($"Executed input mapping script : {this.InputMappingScriptFile} for Prefab : {this.PrefabId}");
        }

        private void LoadPrefab()
        {
            if(this.prefabLoader == null)
            {
                this.prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();              
            }           
            this.prefabEntity = prefabLoader.GetPrefabEntity(this.ApplicationId, this.PrefabId, this.EntityManager);
            this.prefabDataModel = this.prefabEntity.EntityManager.Arguments;
            this.components.Clear();
            this.components.Add(this.prefabEntity);
            logger.Information($"Loaded Prefab : {this.PrefabId} with data model type : {this.prefabDataModel.GetType()}");
        }

        public override async Task OnCompletionAsync()
        {
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var outputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.OutputMappingScriptFile);
            outputMappingAction.Invoke(prefabDataModel);
            logger.Information($"Executed output mapping script : {this.OutputMappingScriptFile} for Prefab : {this.PrefabId}");

            this.Components.Clear();
            this.prefabDataModel = null;       
        }

        #region overridden methods

        public override Entity AddComponent(IComponent component)
        {           
            return this;
        }

        #endregion overridden methods
    }
}
