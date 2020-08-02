namespace Pixel.Automation.Core
{
    public class ApplicationSettings
    {
        /// <summary>
        /// Path relative to working directory where application configuration (including controls and prefabs) are stored locally
        /// </summary>
        public string ApplicationDirectory { get; set; }

        /// <summary>
        /// Path relative to working directory where automation projects are stored locally
        /// </summary>
        public string AutomationDirectory { get; set; }

        /// <summary>
        /// Service end point for storing and retrieving application , process and test result data
        /// </summary>
        public string PersistenceServiceUri { get; set; }

        /// <summary>
        /// If true, data won't be stored using persistence service . Local file syste will be used.
        /// </summary>
        public bool IsOfflineMode { get; set; }

    }
}
