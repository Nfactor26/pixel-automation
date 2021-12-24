﻿using System.Runtime.Serialization;

namespace Pixel.Automation.Core
{
    [DataContract]
    public class ApplicationSettings : NotifyPropertyChanged
    {
        /// <summary>
        /// Path relative to working directory where application configuration (including controls and prefabs) are stored locally
        /// </summary>
        [DataMember]
        public string ApplicationDirectory { get; set; }

        /// <summary>
        /// Path relative to working directory where automation projects are stored locally
        /// </summary>
        [DataMember]
        public string AutomationDirectory { get; set; }

        private string persistenceServiceUri;
        /// <summary>
        /// Service end point for storing and retrieving application , process and test result data
        /// </summary>
        [DataMember]
        public string PersistenceServiceUri
        {
            get => this.persistenceServiceUri;
            set
            {
                this.persistenceServiceUri = value;
                if (string.IsNullOrEmpty(value))
                {
                    this.IsOfflineMode = true;
                }
                OnPropertyChanged();
            }
        }


        private string identityServiceUri;
        /// <summary>
        /// Service end point for OIDC based identity provider 
        /// </summary>
        [DataMember]
        public string IdentityServiceUri
        {
            get => this.identityServiceUri;
            set
            {
                this.identityServiceUri = value;               
                OnPropertyChanged();
            }
        }

      
        private bool isOfflineMode;
        /// <summary>
        /// If true, data won't be stored using persistence service . Local file syste will be used.
        /// </summary>
        [DataMember]
        public bool IsOfflineMode
        {
            get => this.isOfflineMode;
            set
            {
                this.isOfflineMode = value;
                OnPropertyChanged();
            }
        }      
                
        /// <summary>
        /// Default amound of delay in ms before actor is executed
        /// </summary>
        [DataMember]
        public int PreDelay { get; set; }

        /// <summary>
        /// Default amount of delay in ms after actor is executed
        /// </summary>
        [DataMember]
        public int PostDelay { get; set; }

        /// <summary>
        /// Default scaling factor that can be used to control Pre and Post Delay amount
        /// e.g. if delay factor is 3 and pre delay is 100, actual delay applied will be 300 ms before actor is executed.
        /// This can be changed per fixture or per test using execution speed setting.
        /// </summary>
        [DataMember]
        public int DelayFactor { get; set; }

        /// <summary>
        /// Default assembly references for code and script editor.
        /// These must come from .\\refs assembly if dll is present there.
        /// </summary>
        [DataMember]
        public string[] DefaultEditorReferences { get; set; }

        /// <summary>
        /// Default References for the code editor.       
        /// </summary>
        [DataMember]
        public string[] DefaultCodeReferences { get; set; }

        /// <summary>
        /// Default References for the script editor and script engine for runtime.       
        /// </summary>
        [DataMember]
        public string[] DefaultScriptReferences { get; set; }

        /// <summary>
        /// For script engine, MetaDataResolver will resolve only those assemblies which are white listed.
        /// </summary>
        [DataMember]
        public string[] WhiteListedReferences { get; set; }
    }
}
