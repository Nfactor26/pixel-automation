namespace Pixel.Automation.Core
{
    public static class Constants
    {   
        /// <summary>
        /// Data model name for the auto generated Prefab data model
        /// </summary>
        public static readonly string PrefabDataModelName = "PrefabDataModel";
       
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
