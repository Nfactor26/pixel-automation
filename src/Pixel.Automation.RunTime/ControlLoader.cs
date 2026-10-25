using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.RunTime
{
    public class ControlLoader : IControlLoader
    {
        protected readonly ILogger logger = Log.ForContext<ControlLoader>();
        protected readonly IFileSystem fileSystem;        
        protected readonly ApplicationSettings applicationSettings;
        protected readonly Dictionary<string, ControlDescription> Controls = new Dictionary<string, ControlDescription>();
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="fileSystem"></param>
        public ControlLoader(ApplicationSettings applicationSettings, IFileSystem fileSystem)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
        }

        /// <inheritdoc/>      
        public ControlDescription GetControl(string applicationId, string controlId, string version)
        {
            if (Controls.ContainsKey(controlId))
            {
                logger.Debug("Control with applicationId {0} && controlId : {1} is available in cache.", applicationId, controlId);
                return Controls[controlId].Clone() as ControlDescription;
            }
            var control = LoadControl(applicationId, controlId, version);
            this.Controls.Add(controlId, control);
            return control.Clone() as ControlDescription;
        }

        /// <inheritdoc/>      
        public void RemoveFromCache(string controlId)
        {
            if(this.Controls.ContainsKey(controlId))
            {
                this.Controls.Remove(controlId);
                logger.Debug("Control with Id : {0} removed from cache", controlId);
            }
        }

        /// <summary>
        /// Load control with given applicationId and controlId from disk. Version of control is determined from ControlReferences file.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private ControlDescription LoadControl(string applicationId, string controlId, string version)
        {           
            var controlFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationId, Constants.ControlsDirectory, controlId, version,  $"{controlId}.dat");
            logger.Debug("Control with applicationId {0} && controlId : {1} and version : {2} has been loaded.", applicationId, controlId, version);
            return this.fileSystem.LoadFile<ControlDescription>(controlFile);         
        }
    }
}
