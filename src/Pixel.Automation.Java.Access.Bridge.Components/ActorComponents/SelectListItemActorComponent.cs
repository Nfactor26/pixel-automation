extern alias uiaComWrapper;

using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Enums;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using uiaComWrapper::System.Windows.Automation;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Select List Item", "Java", iconSource: null, description: "Select an item in list using index or text", tags: new string[] { "Select", "List", "Java" })]

    public class SelectListItemActorComponent : JABActorComponent
    {

        [DataMember]
        public SelectBy SelectBy { get; set; }

        [DataMember]
        [Description("Text or Index to be selected")]
        public Argument Input { get; set; } = new InArgument<string>();

        public SelectListItemActorComponent():base("Select List Item","SelectListItem")
        {

        }
        public override void Act()
        {         
            string selectText = ArgumentProcessor.GetValue<string>(this.Input);
            AccessibleContextNode dropDownControl = this.GetTargetControl();
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


            //Api based approach doesn't work. It makes the selection but UI is not updated
            //targetControl.AccessBridge.Functions.ClearAccessibleSelectionFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle);
            //targetControl.AccessBridge.Functions.AddAccessibleSelectionFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle, targetIndex);
        }
    }  
}
