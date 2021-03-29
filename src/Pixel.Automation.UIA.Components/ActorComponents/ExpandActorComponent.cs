﻿extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="ExpandActorComponent"/> to expand the state of a control.
    /// Control must support <see cref="ExpandCollapsePattern"/>.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Expand", "UIA", iconSource: null, description: "Trigger ExpandCollapsePattern pattern on AutomationElement to expand it", tags: new string[] { "Expand", "UIA" })]
    public class ExpandActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ExpandActorComponent>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExpandActorComponent() : base("Expand", "Expand")
        {

        }

        /// <summary>
        /// Expand the state of  control that supports ExpandCollasePattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ExpandCollapsePattern is not supported</exception>
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            if(control.IsControlExpanded())
            {
                logger.Information("Control is already expanded.");
                return;
            }
            control.Expand();
            logger.Information("Control was expanded.");
        }

        public override string ToString()
        {
            return "Expand Actor";
        }
    }
}