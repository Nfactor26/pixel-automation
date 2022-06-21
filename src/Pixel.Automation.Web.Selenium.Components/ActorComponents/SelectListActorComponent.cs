using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components
{

    /// <summary>
    /// Use <see cref="SelectListItemActorComponent"/> to select an option in list web control.
    /// Option can be specified using display text, value or index of the option
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Select List", "Selenium", iconSource: null, description: "Select an item in list using text/value/index", tags: new string[] { "Select", "List", "Web" })]
    public class SelectListItemActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SelectListItemActorComponent>();

        /// <summary>
        /// Specify whether to select by display text, value or index of the option
        /// </summary>
        [DataMember]
        [Display(Name = "Select By", GroupName = "Configuration", Order = 20, Description = "Specify whether to selecy by display text, value or index of the option")]      
        public SelectBy SelectBy { get; set; } = SelectBy.Value;

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
        public override async Task ActAsync()
        {
            IWebElement control = await GetTargetControl();
            string selectText = await ArgumentProcessor.GetValueAsync<string>(this.Option);

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

            logger.Information($"Option {selectText} was selected.");
        }

        public override string ToString()
        {
            return "Select List Actor";
        }

    }  
}
