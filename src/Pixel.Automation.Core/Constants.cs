namespace Pixel.Automation.Core
{
    public static class Constants
    {
        /// <summary>
        /// Directory name where the controls belonging to application are stored locally
        /// </summary>
        public static readonly string ControlsDirectory = "Controls";

        /// <summary>
        /// Directory name where the prefabs belonging to application are stored locally
        /// </summary>
        public static readonly string PrefabsDirectory = "Prefabs";

        /// <summary>
        /// Directory name where the scripts belonging to a automation or prefab projects are stored
        /// </summary>
        public static readonly string ScriptsDirectory = "Scripts";

        /// <summary>
        /// Directory name where the C# project belonging to a automation or prefab projects that holds custom data models are stored.
        /// This project is compiled and the assembly is loaded for the project which makes the data model avaialbe for these projects in scripting.
        /// </summary>
        public static readonly string DataModelDirectory = "DataModel";

        /// <summary>
        /// Directory name where the output of the data model project is stored.
        /// References folder is populated after project is published and used only by published projects.
        /// An active project will used data model assemblies from the temporary directory.
        /// </summary>
        public static readonly string ReferencesDirectory = "References";

        /// <summary>
        /// Directory name where the test fixture and test cases are stored.
        /// </summary>
        public static readonly string TestCasesDirectory = "TestCases";

        /// <summary>
        /// Directory name where the test data is stored.
        /// </summary>
        public static readonly string TestDataDirectory = "TestData";

        /// <summary>
        /// Directory name where the temporary files ( output of data model project) for a automation or prefab project is stored.
        /// </summary>
        public static readonly string TemporaryDirectory = "Temp";

        /// <summary>
        /// Data model name for the auto generated Prefab data model
        /// </summary>
        public static readonly string PrefabDataModelName = "PrefabDataModel";

        /// <summary>
        /// Name of the prefab template file  used in prefab project editor
        /// </summary>
        public static readonly string PrefabTemplateFileName = "Template.proc";

        /// <summary>
        /// Name of Prefab entity file which contains the automation workflow
        /// </summary>
        public static readonly string PrefabProcessFileName = "Prefab.proc";

        /// <summary>
        /// Namespace prefix for prefab projects
        /// </summary>
        public static readonly string PrefabNameSpace = "Pixel.Automation.Prefabs";

        /// <summary>
        /// Data model name for the data model associated with the automation project.
        /// This data model acts as a global for scripts within Environment Setup and Environment TearDown
        /// </summary>
        public static readonly string AutomationProcessDataModelName = "ProcessDataModel";

        /// <summary>
        /// Name of entity file for an automaiton project which contains the automation workflow
        /// </summary>
        public static readonly string AutomationProcessFileName = "Automation.proc";

        /// <summary>
        /// Name of the file which store details of the controls that have been used in a automation or prefab project.
        /// </summary>
        public static readonly string ControlRefrencesFileName = "ControlReferences.ref";

        /// <summary>
        /// Name of the file which store details of the prefabs that have been used in a automation project.
        /// </summary>
        public static readonly string PrefabRefrencesFileName = "PrefabReferences.ref";

        /// <summary>
        /// Default name for the Initialization script for configuring ProcessDaaModel used at design time.
        /// A custom initialization script can be provided as a command line argument to test runner process.
        /// </summary>
        public static readonly string InitializeEnvironmentScript = "InitializeEnvironment.csx";

        /// <summary>
        /// Meta data file name for applications
        /// </summary>
        public static readonly string ApplicationsMeta = "Applications.meta";

        /// <summary>
        /// Meta data file name for projects
        /// </summary>
        public static readonly string ProjectsMeta = "Projects.meta";

        /// <summary>
        /// Meta data file name for project versions
        /// </summary>
        public static readonly string VersionsMeta = "Versions.meta";
    }
}
