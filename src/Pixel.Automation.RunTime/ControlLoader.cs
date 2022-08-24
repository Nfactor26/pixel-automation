using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.RunTime
{
    public class ControlLoader : IControlLoader
    {
        protected readonly ILogger logger = Log.ForContext<PrefabLoader>();
        protected readonly IFileSystem fileSystem;
        protected readonly ApplicationSettings applicationSettings;
        protected readonly Dictionary<string, ControlDescription> Controls = new Dictionary<string, ControlDescription>();
        protected ControlReferences controlReferences;

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
        public ControlDescription GetControl(string applicationId, string controlId)
        {
            if (Controls.ContainsKey(controlId))
            {
                logger.Debug("Control with applicationId {0} && controlId : {1} is available in cache.", applicationId, controlId);
                return Controls[controlId];
            }
            var control = LoadControl(applicationId, controlId);
            this.Controls.Add(controlId, control);
            return control;
        }

        /// <inheritdoc/>      
        public void RemoveFromCache(string controlId)
        {
            if(this.Controls.ContainsKey(controlId))
            {
                this.Controls.Remove(controlId);
            }
        }

        /// <summary>
        /// Load control with given applicationId and controlId from disk. Version of control is determined from ControlReferences file.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private ControlDescription LoadControl(string applicationId, string controlId)
        {
            var controlReferences = this.GetControlReferences();
            var controlVersionInUse = controlReferences.GetControlVersionInUse(applicationId, controlId);
            var controlFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationId, Constants.ControlsDirectory, controlId, controlVersionInUse.ToString(),  $"{controlId}.dat");
            logger.Information("Control with applicationId {0} && controlId : {1} and version : {2} is available in cache.", applicationId, controlId, controlVersionInUse);
            return this.fileSystem.LoadFile<ControlDescription>(controlFile);
        }

        /// <summary>
        /// Get the ControlReferences file that contains details of controls used in a automation project
        /// </summary>
        /// <returns></returns>
        protected virtual ControlReferences GetControlReferences()
        {
            if(this.controlReferences == null)
            {             
                this.controlReferences = this.fileSystem.LoadFile<ControlReferences>(this.fileSystem.ControlReferencesFile);
            }
            return this.controlReferences;
        }
    }
}
