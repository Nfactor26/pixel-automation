using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        [DataMember(Order = 500)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember(Order = 510)]
        [Browsable(false)]    
        public string PrefabId { get; set; }

        private string inputMappingScriptFile;
        [DataMember(Order = 200)]
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
        [DataMember(Order = 200)]
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

        [NonSerialized]
        private IPrefabLoader prefabLoader;

        [NonSerialized]
        private object prefabDataModel;

        [NonSerialized]
        private Entity prefabEntity;

        [IgnoreDataMember]
        [Browsable(false)]
        public override List<IComponent> Components
        {
            get => base.Components;            
        }
               
        public PrefabEntity() : base("Prefab Entity", "PrefabEntity")
        {

        }

        public override async Task BeforeProcessAsync()
        {
            try
            {
                Debug.Assert(!this.Components.Any(), "There should be no child componets in a Prefab Entity");
                this.LoadPrefab();
                this.Components.Add(this.prefabEntity);
                IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
                var inputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.InputMappingScriptFile);
                inputMappingAction.Invoke(prefabDataModel);
                logger.Information($"Executed input mapping script : {this.InputMappingScriptFile} for Prefab : {this.PrefabId}");
            }
            catch
            {
                this.Components.Clear();
                throw;
            }
        }

        public void LoadPrefab()
        {
            if(this.prefabLoader == null)
            {
                this.prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();              
            }           
            this.prefabEntity = prefabLoader.GetPrefabEntity(this.ApplicationId, this.PrefabId, this.EntityManager);
            this.prefabDataModel = this.prefabEntity.EntityManager.Arguments;                 
            logger.Information($"Loaded Prefab : {this.PrefabId} with data model type : {this.prefabDataModel.GetType()}");
        }

        public override async Task OnCompletionAsync()
        {
            try
            {
                IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
                var outputMappingAction = await scriptEngine.CreateDelegateAsync<Action<object>>(this.OutputMappingScriptFile);
                outputMappingAction.Invoke(prefabDataModel);
                logger.Information($"Executed output mapping script : {this.OutputMappingScriptFile} for Prefab : {this.PrefabId}");             
            }
            finally
            {
                this.Components.Clear();
                this.prefabDataModel = null;
            }   
        }

        #region overridden methods

        public override Entity AddComponent(IComponent component)
        {           
            return this;
        }

        #endregion overridden methods

        public Type GetPrefabDataModelType()
        {
            if(this.prefabDataModel == null)
            {
                this.LoadPrefab();
            }
            return this.prefabDataModel.GetType();
        }
    }
}
