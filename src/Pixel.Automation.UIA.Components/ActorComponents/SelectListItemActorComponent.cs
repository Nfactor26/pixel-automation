extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.UIA.Components.Enums;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{

    /// <summary>
    /// Use <see cref="SelectListItemActorComponent"/> to select an option in a control e.g. combobox or a list control.
    /// Option can be specified using text, value or index of the option. Text and value can be used interchangeably.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Select List", "UIA", iconSource: null, description: "Select an item in list using text/value/index", tags: new string[] { "Select", "List", "UIA" })]
    public class SelectListItemActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SelectListItemActorComponent>();

        /// <summary>
        /// Indicates whether the control should be selected, added to existing selection or removed from existing selection
        /// </summary>
        [DataMember]
        [Display(Name = "Selection Mode", GroupName = "Configuration", Order = 10, Description = "Indicates whether the control should be selected, added to existing selection or" +
            "removed from existing selection")]
        public SelectMode SelectionMode { get; set; } = SelectMode.Select;

        /// <summary>
        /// Specify whether to select by display text, index of the option.
        /// SelectBy.Text and SelectBy.Value can be used interchangeably.
        /// </summary>
        [DataMember]
        [Display(Name = "Select By", GroupName = "Configuration", Order = 20, Description = "Specify whether to selecy by display text, value or index of the option." +
            "SelectBy.Text and SelectBy.Value can be used interchangeably.")]
        public SelectBy SelectBy { get; set; } = SelectBy.Text;

        /// <summary>
        /// Option to be selected
        /// </summary>
        [DataMember]
        [Display(Name = "Option", GroupName = "Configuration", Order = 30, Description = "Option to be selected")]
        public Argument Option { get; set; } = new InArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public SelectListItemActorComponent() : base("Select List Item", "SelectListItem")
        {

        }

        /// <summary>
        /// Select the configured option contained in a select (or similar) element
        /// </summary>
        public override void Act()
        {
            var control = GetTargetControl();
            if(control.IsControlCollapsed())
            {
                control.Expand();
            }
            var searchCondition = ConditionFactory.FromControlType(uiaComWrapper.System.Windows.Automation.ControlType.ListItem)
                .OrControlType(uiaComWrapper.System.Windows.Automation.ControlType.TreeItem)
                .OrControlType(uiaComWrapper.System.Windows.Automation.ControlType.DataItem)
                .OrControlType(uiaComWrapper.System.Windows.Automation.ControlType.MenuItem);
            var childControls = control.FindAll(TreeScope.Descendants, searchCondition);
        
            logger.Information($"Located {childControls.ToList().Count}  options in control.");

            string selectText = ArgumentProcessor.GetValue<string>(this.Option);
            switch (SelectBy)
            {
                case SelectBy.Text:
                case SelectBy.Value:
                    foreach(AutomationElement item in childControls)
                    {
                        if(item.Current.Name.Equals(selectText) || item.Current.AutomationId.Equals(selectText))
                        {
                            SelectControl(item);
                        }
                    }
                    break;
                case SelectBy.Index:
                    int index = int.Parse(selectText);
                    if(childControls.Count >= index)
                    {
                        SelectControl(childControls.ToList().ElementAt(index - 1));
                        return;
                    }
                    throw new IndexOutOfRangeException($"Control doesn't have enough options. Desired option is {index}");                   
            }
            if(control.IsControlExpanded())
            {
                control.Collapse();
            }
            logger.Information($"Option {selectText} was selected.");
        }

        private void SelectControl(AutomationElement control)
        {
            switch (this.SelectionMode)
            {
                case SelectMode.Select:
                    control.Select();                  
                    break;
                case SelectMode.AddToSelection:
                    control.AddToSelection();                 
                    break;
                case SelectMode.RemoveFromSelection:
                    control.RemoveFromSelection();                   
                    break;
            }
        }

        public override string ToString()
        {
            return "Select List Actor";
        }

    }
}
