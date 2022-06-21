using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="AttachApplicationActorComponent"/> to attach to an existing application process which needs to be automated.
    /// Not all application types support attach behavior and will throw an exception if attach behavior is not supported by application type.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Attach", "Application", iconSource: null, description: "Attach to target application", tags: new string[] { "Attach" })]
    public class AttachApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<AttachApplicationActorComponent>();
      
        /// <summary>
        /// Owner application entity
        /// </summary>   
        [Browsable(false)]
        public IApplicationEntity ApplicationEntity
        {
            get
            {
                return this.EntityManager.GetApplicationEntity(this);
            }
        }

        AttachMode attachMode = AttachMode.AttachToExecutable;
        /// <summary>
        /// Indicates how the target process to be attached to is identifed.
        /// </summary>
        [DataMember]
        [Display(Name = "Attach Mode", GroupName = "Configuration", Order = 10, Description = "Indicates how the application to be attached to is identified.")]
        [RefreshProperties(RefreshProperties.All)]
        public AttachMode AttachMode
        {
            get => this.attachMode;
            set
            {
                switch (value)
                {
                    case AttachMode.AttachToExecutable:
                        if (!this.AttachTarget.ArgumentType.Equals(typeof(string).Name))
                        {
                            this.AttachTarget = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };
                        }
                        break;
                    case AttachMode.AttachToWindow:
                        if (!this.AttachTarget.ArgumentType.Equals(typeof(ApplicationWindow).Name))
                        {
                            this.AttachTarget = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };
                        }
                        break;
                }
                this.attachMode = value;
                OnPropertyChanged();
            }
        }


        Argument attachTarget = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };
        /// <summary>
        /// Details of the target application to be attached to. Details will vary based on AttachMode.
        /// For e.g. if AttachMode is AttachToExecutable, you need to specify the name of the executable. However, if AttachMode is AttachToWindow, 
        /// you need to lookup ApplicationWindow first and setup this as an argument.
        /// </summary>
        [DataMember]
        [Display(Name = "Attach To", GroupName = "Configuration", Order = 20, Description = "Details of the target application to be attached to")]
        public Argument AttachTarget
        {
            get => this.attachTarget;
            set
            {
                this.attachTarget = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AttachApplicationActorComponent() : base("Attach Application", "AttachApplication")
        {

        }

        /// <summary>
        /// Locate the target app based on how AttachMode is configured and  setup owner application to use the identified application process as 
        /// Target application for automation. 
        /// </summary>
        /// <exception cref="NotSupportedException">Throws NotSupportedException if ApplcationType doesn't support attach to behavior</exception>
        public override async Task ActAsync()
        {
            var applicationEntity = this.ApplicationEntity;
            if (!applicationEntity.CanUseExisting)
            {
                throw new NotSupportedException($"Application type doesn't support attach to an existing process. Application must be launched using Launch Actor for automation.");
            }
            ApplicationProcess targetApp;

            switch (this.attachMode)
            {
                case AttachMode.AttachToExecutable:
                    string executableName = await this.ArgumentProcessor.GetValueAsync<string>(this.attachTarget);
                    if (string.IsNullOrEmpty(executableName))
                    {
                        throw new ArgumentException($"{nameof(this.attachTarget)} can't be null or empty");
                    }
                    targetApp = ApplicationProcess.Attach(executableName);
                    applicationEntity.UseExisting(targetApp);
                    logger.Information("Attached to executable : {$ExecutableName}", executableName);
                    break;

                case AttachMode.AttachToWindow:
                    var applicationWindow = await this.ArgumentProcessor.GetValueAsync<ApplicationWindow>(this.attachTarget);
                    var windowProcess = Process.GetProcessById(applicationWindow.ProcessId);
                    if (windowProcess == null)
                    {
                        throw new ArgumentException($"Failed to get process by id : {applicationWindow.ProcessId} for {applicationWindow}");
                    }
                    targetApp = ApplicationProcess.Attach(windowProcess);
                    applicationEntity.UseExisting(targetApp);
                    logger.Information("Attached to application window : {$applicationWindow}", applicationWindow);
                    break;
            }
            await Task.CompletedTask;
        }

        public override string ToString()
        {
            return "Attach Actor";
        }
    }
}
