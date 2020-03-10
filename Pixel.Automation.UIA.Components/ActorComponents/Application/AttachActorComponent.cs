extern alias uiaComWrapper;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.UIA.Components.Enums;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;


namespace Pixel.Automation.UIA.Components.ActorComponents
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Attach Action", "UIA", iconSource: null, description: "Attach to target application", tags: new string[] { "Attach", "UIA" })]

    public class AttachActorComponent : ActorComponent
    {
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetApplicationDetails<WinApplication>(this);
            }
        }      
      

        Argument attachTarget = new InArgument<ApplicationWindow>();
        [DataMember]
        [Category("Input")]       
        [DisplayName("Attach To")]       
        [RefreshProperties(RefreshProperties.All)]
        public Argument AttachTarget
        {
            get => this.attachTarget;
            set
            {
                this.attachTarget = value;
                OnPropertyChanged("AttachTarget");
            }
        }

        AttachMode attachMode = AttachMode.AttachToExecutable;
        [DataMember]
        [Category("Input")]       
        [DisplayName("Attach Mode")]
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

        public AttachActorComponent() : base("Attach to window", "AttachActorComponent")
        {
           
        }


        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            Application targetApp = default;
            Process windowProcess = default;
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
                    Log.Information("Attached to executable : {$ExecutableName}", executableName);
                    break;
                case AttachMode.AttachToWindow:
                    applicationWindow = argumentProcessor.GetValue<ApplicationWindow>(this.attachTarget);
                    windowProcess = Process.GetProcessById(applicationWindow.ProcessId);
                    if (windowProcess == null)
                        throw new ArgumentException($"Failed to get process by id : {applicationWindow.ProcessId} for {applicationWindow}");
                    targetApp = Application.Attach(windowProcess);
                    ApplicationDetails.TargetApplication = targetApp;
                    Log.Information("Attached to application window : {$applicationWindow}", applicationWindow);
                    break;
                case AttachMode.AttachToAutomationElement:
                    AutomationElement targetElement = argumentProcessor.GetValue<AutomationElement>(this.attachTarget);
                    windowProcess = Process.GetProcessById(targetElement.Current.ProcessId);
                    if (windowProcess == null)
                        throw new ArgumentException($"Failed to get process by id : {applicationWindow.ProcessId} for {applicationWindow}");
                    targetApp = Application.Attach(windowProcess);
                    ApplicationDetails.TargetApplication = targetApp;
                    Log.Information("Attached to application window : {$applicationWindow}", applicationWindow);
                    break;
            }
                  
        }        
    }
}
