using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.RunTime
{
    public class ControlLoader : IControlLoader
    {
        protected readonly ILogger logger = Log.ForContext<PrefabLoader>();
        protected readonly IFileSystem fileSystem;
        protected readonly IReferenceManager referenceManager;
        protected readonly ApplicationSettings applicationSettings;
        protected readonly Dictionary<string, ControlDescription> Controls = new Dictionary<string, ControlDescription>();
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationSettings"></param>
        /// <param name="fileSystem"></param>
        public ControlLoader(ApplicationSettings applicationSettings, IFileSystem fileSystem, IReferenceManager referenceManager)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.referenceManager = Guard.Argument(referenceManager).NotNull().Value;
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
                logger.Information("Control with Id : {0} removed from cache", controlId);
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
            logger.Information("Control with applicationId {0} && controlId : {1} and version : {2} has been loaded.", applicationId, controlId, controlVersionInUse);
            return this.fileSystem.LoadFile<ControlDescription>(controlFile);
        }

        /// <summary>
        /// Get the ControlReferences file that contains details of controls used in a automation project
        /// </summary>
        /// <returns></returns>
        protected virtual ControlReferences GetControlReferences()
        {
           return referenceManager.GetControlReferences();
        }
    }
}
