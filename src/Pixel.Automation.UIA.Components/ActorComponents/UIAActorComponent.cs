﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;
using System.IO;

namespace Pixel.Automation.UIA.Components
{

    /// <summary>
    /// Base component for actors based on UIA
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class UIAActorComponent : ActorComponent
    {
        /// <summary>
        /// Target control that needs to be interacted with. This is an optional component. If the actor is an immediate child of a <see cref="WinControlEntity"/>,
        /// target control will be automatically retrieved from parent ControlEntity. If the target control was previously looked up by any means , it can be specified as an 
        /// argument instead.
        /// </summary>
        [DataMember]
        [Display(Name ="Target Control", GroupName = "Control Details", Order = 10, Description = "[Optional] Target control that needs to be interacted with")]         
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

        
        /// <summary>
        /// Parent control entity that provides the target control to be interacted with.
        /// </summary>
        [Browsable(false)]
        public WinControlEntity ControlEntity
        {
            get
            {
                return this.Parent as WinControlEntity;

            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        protected UIAActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

        /// <summary>
        /// Get the AutomationElement identified using the control details
        /// </summary>
        /// <returns></returns>
        protected async Task<(string, AutomationElement)> GetTargetControl()
        {
            UIControl targetControl;
            if (this.TargetControl.IsConfigured())
            {
                targetControl = await ArgumentProcessor.GetValueAsync<UIControl>(this.TargetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = await this.ControlEntity.GetControl();
            }

            AutomationElement control = targetControl.GetApiControl<AutomationElement>();
            return (targetControl.ControlName, control);
        }

        /// <summary>
        /// Take a screen shot if capturing screenshot is enabled after Act method finishes
        /// </summary>
        /// <returns></returns>
        public override async Task OnCompletionAsync()
        {
            if (TraceManager.IsEnabled)
            {
                await CaptureScreenShotAsync();
            }
        }

        /// <summary>
        /// Capture screenshot of the active page
        /// </summary>
        /// <returns></returns>
        public async Task CaptureScreenShotAsync()
        {
            var ownerApplicationEntity = this.EntityManager.GetApplicationEntity(this);
            if (TraceManager.IsEnabled && ownerApplicationEntity.AllowCaptureScreenshot)
            {
                string imageFile = Path.Combine(this.EntityManager.GetCurrentFileSystem().TempDirectory, $"{Path.GetRandomFileName()}.jpeg");
                await ownerApplicationEntity.CaptureScreenShotAsync(imageFile);
                TraceManager.AddImage(Path.GetFileName(imageFile));
            }
        }

        /// <summary>
        /// Throw <see cref="ConfigurationException"/> if ControlEntity is missing.
        /// </summary>
        protected void ThrowIfMissingControlEntity()
        {
            if (this.ControlEntity == null)
            {
                throw new ConfigurationException($"Component with id : {this.Id} must be child of WinControlEntity");
            }
        }

    }
}
