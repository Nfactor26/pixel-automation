using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Select List", "Selenium", iconSource: null, description: "Select an item in list using text/value/index", tags: new string[] { "Select", "List", "Web" })]
    public class SelectListItemActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]             
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]
        public SelectBy SelectBy { get; set; }

        [DataMember]       
        [Description("Text/Value/Index to be selected")]
        public Argument Input { get; set; } = new InArgument<string>();

        public SelectListItemActorComponent() : base("Select List Item", "SelectListItem")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            string selectText = ArgumentProcessor.GetValue<string>(this.Input);

            SelectElement selectElement = new SelectElement(control);
            switch (SelectBy)
            {
                case SelectBy.Text:
                    selectElement.SelectByText(selectText);
                    break;
                case SelectBy.Value:
                    selectElement.SelectByValue(selectText);
                    break;
                case SelectBy.Index:
                    int index = int.Parse(selectText);
                    selectElement.SelectByIndex(index);
                    break;
            }

            Log.Information("SelectList interaction completed");
        }

        public override string ToString()
        {
            return "Selenium.SelectList";
        }

    }

    public enum SelectBy
    {
        Text,
        Value,
        Index
    }
}
