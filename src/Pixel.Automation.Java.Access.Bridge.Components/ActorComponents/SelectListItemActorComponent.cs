extern alias uiaComWrapper;

using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using uiaComWrapper::System.Windows.Automation;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Use <see cref="SelectListItemActorComponent"/> to select an option in a control e.g. combobox or a list control.
    /// Option can be specified using text, value or index of the option. Text and value can be used interchangeably.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Select List", "Java", iconSource: null, description: "Select an item in list using index or text", tags: new string[] { "Select", "List", "Java" })]
    public class SelectListItemActorComponent : JABActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SelectListItemActorComponent>();

        /// <summary>
        /// Specify whether to select by display text, index of the option.
        /// SelectBy.Text and SelectBy.Value can be used interchangeably.
        /// </summary>
        [DataMember]
        [Display(Name = "Select By", GroupName = "Configuration", Order = 10, Description = "Specify whether to selecy by display text, value or index of the option." +
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
        /// Select the configured option contained in a combo box or similar control
        /// </summary>
        public override async Task ActAsync()
        {
            string selectText = await ArgumentProcessor.GetValueAsync<string>(this.Option);
            AccessibleContextNode dropDownControl = await this.GetTargetControl();
            AccessibleContextNode optionControl = null;
            var options = dropDownControl.FindAll(TreeScope.Descendants, new JavaControlIdentity() { Role = "label" });

            switch (SelectBy)
            {
                case SelectBy.Index:
                    int index = int.Parse(selectText);
                    foreach (var option in options)
                    {
                        index--;
                        if (index == 0)
                        {
                            optionControl = option;
                            break;
                        }
                    }
                    if (optionControl == null)
                    {
                        throw new ArgumentException($"Option with index {int.Parse(selectText)} is not available for selection.");
                    }
                    break;
                case SelectBy.Text:
                case SelectBy.Value:
                    foreach (var option in options)
                    {
                        if (option.GetInfo().name.Equals(selectText))
                        {
                            optionControl = option;
                            break;
                        }
                    }
                    if (optionControl == null)
                    {
                        throw new ArgumentException($"Option {selectText} is not available for selection");
                    }
                    break;
            }

            //open the drop down 
            AccessibleActionsToDo actionsToDo = new AccessibleActionsToDo()
            {
                actions = new AccessibleActionInfo[32],
                actionsCount = 1
            };
            actionsToDo.actions[0] = new AccessibleActionInfo() { name = "togglePopup" };
            dropDownControl.AccessBridge.Functions.RequestFocus(dropDownControl.JvmId, dropDownControl.AccessibleContextHandle);
            dropDownControl.AccessBridge.Functions.DoAccessibleActions(dropDownControl.JvmId, dropDownControl.AccessibleContextHandle, ref actionsToDo, out int failure);

            //click the option. This only highlights option but doesn't perform actual selection
            actionsToDo.actions[0] = new AccessibleActionInfo() { name = "click" };
            optionControl.AccessBridge.Functions.DoAccessibleActions(optionControl.JvmId, optionControl.AccessibleContextHandle, ref actionsToDo, out failure);

            Thread.Sleep(500);

            //simulate enter key press to make actual selection
            var keyboard = this.EntityManager.GetServiceOfType<ISyntheticKeyboard>();
            keyboard.KeyPress(SyntheticKeyCode.RETURN);

            logger.Information($"Option {selectText} was selected.");

            //Api based approach doesn't work. It makes the selection but UI is not updated
            //targetControl.AccessBridge.Functions.ClearAccessibleSelectionFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle);
            //targetControl.AccessBridge.Functions.AddAccessibleSelectionFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle, targetIndex);
        }

    }
}
