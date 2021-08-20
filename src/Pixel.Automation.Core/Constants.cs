namespace Pixel.Automation.Core
{
    public static class Constants
    {   
        /// <summary>
        /// Data model name for the auto generated Prefab data model
        /// </summary>
        public static readonly string PrefabDataModelName = "PrefabDataModel";

        /// <summary>
        /// Name of the prefab template file  used in prefab project editor
        /// </summary>
        public static readonly string PrefabTemplateFileName = "Template.dat";

        /// <summary>
        /// Name of Prefab entity file which contains the automation workflow
        /// </summary>
        public static readonly string PrefabEntityFileName = "Prefab.dat";

        /// <summary>
        /// Namespace prefix for prefab projects
        /// </summary>
        public static readonly string PrefabNameSpace = "Pixel.Automation.Prefabs";

        /// <summary>
        /// Data model name for the data model associated with the automation process.
        /// This data model acts as a global for scripts within Environment Setup and Environment TearDown
        /// </summary>
        public static readonly string ProcessDataModelName = "ProcessDataModel";
       
        /// <summary>
        /// Default name for the Initialization script for configuring ProcessDaaModel used at design time.
        /// A custom initialization script can be provided as a command line argument to test runner process.
        /// </summary>
        public static readonly string InitializeEnvironmentScript = "InitializeEnvironment.csx";
    }
}
