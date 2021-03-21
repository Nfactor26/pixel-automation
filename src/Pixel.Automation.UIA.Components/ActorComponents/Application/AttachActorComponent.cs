extern alias uiaComWrapper;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.UIA.Components.Enums;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;


namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="AttachActorComponent"/> to attach to an existing application which needs to be automated.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Attach", "UIA", "Application", iconSource: null, description: "Attach to target application", tags: new string[] { "Attach", "UIA" })]
    public class AttachActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<AttachActorComponent>();

        /// <summary>
        /// Owner application details
        /// </summary>
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
            }
        }      
      

        Argument attachTarget = new InArgument<ApplicationWindow>();        
        /// <summary>
        /// Details of the target application to be attached to. Details will vary based on AttachMode.
        /// For e.g. if AttachMode is AttachToExecutable, you need to specify the name of the executable. However, if AttachMode is AttachToWindow, 
        /// you need to lookup ApplicationWindow first and setup this as an argument.
        /// </summary>
        [DataMember]       
        [Display(Name = "Attach To", GroupName = "Configuration", Order = 10, Description = "Details of the target application to be attached to")]       
        [RefreshProperties(RefreshProperties.All)]
        public Argument AttachTarget
        {
            get => this.attachTarget;
            set
            {
                this.attachTarget = value;
                OnPropertyChanged();
            }
        }

        AttachMode attachMode = AttachMode.AttachToExecutable;
       /// <summary>
       /// Indicaets how the application to be attached to is identifed.
       /// </summary>
        [DataMember]         
        [Display(Name = "Attach Mode", GroupName = "Configuration", Order = 30, Description = "Indicates how the application to be attached to is identified.")]
        public AttachMode AttachMode
        {
            get => this.attachMode;
            set
            {
                switch(value)
                {
                    case AttachMode.AttachToExecutable:
                        this.AttachTarget = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };
                        break;
                    case AttachMode.AttachToWindow:
                        this.AttachTarget = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };
                        break;
                    case AttachMode.AttachToAutomationElement:
                        this.AttachTarget = new InArgument<AutomationElement>() { Mode = ArgumentMode.DataBound };
                        break;
                }
                this.attachMode = value;               
            }
        }     

        /// <summary>
        /// Default constructor
        /// </summary>
        public AttachActorComponent() : base("Attach to window", "AttachActorComponent")
        {
           
        }

        /// <summary>
        /// Locate the target app based on how AttachMode is configured and point setup owner application to use the identified application as 
        /// Target application
        /// </summary>
        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            Application targetApp;
            Process windowProcess;
            ApplicationWindow applicationWindow = default;

            switch (this.attachMode)
            {
                case AttachMode.AttachToExecutable:
                    string executableName = argumentProcessor.GetValue<string>(this.attachTarget);
                    if (string.IsNullOrEmpty(executableName))
                    {
                        throw new ArgumentException($"{nameof(this.attachTarget)} can't be null or empty");                  
                    }
                    targetApp = Application.Attach(executableName);
                    ApplicationDetails.TargetApplication = targetApp;
                    logger.Information("Attached to executable : {$ExecutableName}", executableName);
                    break;

                case AttachMode.AttachToWindow:
                    applicationWindow = argumentProcessor.GetValue<ApplicationWindow>(this.attachTarget);
                    windowProcess = Process.GetProcessById(applicationWindow.ProcessId);
                    if (windowProcess == null)
                    {
                        throw new ArgumentException($"Failed to get process by id : {applicationWindow.ProcessId} for {applicationWindow}");
                    }
                    targetApp = Application.Attach(windowProcess);
                    ApplicationDetails.TargetApplication = targetApp;
                    logger.Information("Attached to application window : {$applicationWindow}", applicationWindow);
                    break;

                case AttachMode.AttachToAutomationElement:
                    AutomationElement targetElement = argumentProcessor.GetValue<AutomationElement>(this.attachTarget);
                    windowProcess = Process.GetProcessById(targetElement.Current.ProcessId);
                    if (windowProcess == null)
                    {
                        throw new ArgumentException($"Failed to get process by id : {applicationWindow.ProcessId} for {applicationWindow}");
                    }
                    targetApp = Application.Attach(windowProcess);
                    ApplicationDetails.TargetApplication = targetApp;
                    logger.Information("Attached to application window : {$applicationWindow}", applicationWindow);
                    break;
            }
                  
        }

        public override string ToString()
        {
            return "Attach To Actor";
        }
    }
}
