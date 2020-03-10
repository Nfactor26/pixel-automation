extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Property", "UIA", iconSource: null, description: "Get any of the property of Automation Element identified using ControlIdentity", tags: new string[] { "GetProperty", "UIA" })]
    public class GetPropertyActorComponent : UIAActorComponent
    {        

        string propertyToFetch;
        [DataMember]      
        public string PropertyToFetch
        {
            get
            {
                return propertyToFetch;
            }
            set
            {
                propertyToFetch = value;
                OnPropertyChanged("PropertyToFetch");
            }
        }

        [DataMember]        
        public Argument Output { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public GetPropertyActorComponent() : base("Get Property", "GetProperty")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            string result = string.Empty;
            switch (this.propertyToFetch)
            {
                case "Name":
                    result = control.Current.Name;
                    break;
                case "AutomationId":
                    result = control.Current.AutomationId;
                    break;
                case "NativeHandle":
                    result = control.Current.NativeWindowHandle.ToString();
                    break;
                case "ProcessId":
                    result = control.Current.ProcessId.ToString();
                    break;
                case "HelpText":
                    result = control.Current.HelpText;
                    break;
                case "IsEnabled":
                    result = control.Current.IsEnabled.ToString();
                    break;
                case "IsOffScreen":
                    result = control.Current.IsOffscreen.ToString();
                    break;
                case "HasKeyboardFocus":
                    result = control.Current.HasKeyboardFocus.ToString();
                    break;
                default:
                    throw new ArgumentException($"{this.propertyToFetch} is not a valid AutomationElement property or is not supported");


            }
            ArgumentProcessor.SetValue<string>(Output, result);
        }

        public override string ToString()
        {
            return "GetProperty";
        }
    }


    //class PropertiesItemsSource : IItemsSource
    //{
    //    public ItemCollection GetValues()
    //    {
    //        ItemCollection strategies = new ItemCollection();
    //        strategies.Add("AutomationId");
    //        strategies.Add("Name");
    //        strategies.Add("IsEnabled");
    //        strategies.Add("IsOffScreen");
    //        strategies.Add("ProcessId");
    //        strategies.Add("NativeHandle");
    //        strategies.Add("HelpText");         
    //        strategies.Add("HasKeyboardFocus");
    //        return strategies;
    //    }
    //}
}
