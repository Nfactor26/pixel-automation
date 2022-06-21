using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="GetValueActorComponent"/> to retrieve the value of  a web control.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Value", "Selenium", iconSource: null, description: "Get the value attribute of a WebElement", tags: new string[] { "Value", "Get", "Web" })]
    public class GetValueActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<GetValueActorComponent>();

        /// <summary>
        /// Argument where the value of the attribute will be stored
        /// </summary>
        [DataMember]
        [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Argument where the value of the attribute will be stored")]      
        public Argument Result { get; set; } = new OutArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public GetValueActorComponent() : base("Get Value", "GetValue")
        {

        }

        /// <summary>
        /// Get the value  of <see cref="IWebElement"/>.
        /// If value is not available, text is returned instead.
        /// </summary>
        public override async Task ActAsync()
        {
            IWebElement control = await GetTargetControl();
            string extractedValue = control.GetAttribute("value");
            if (string.IsNullOrEmpty(extractedValue))
            {
                extractedValue = control.Text;
            }
            await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);

            logger.Information("Retrived value of control");
        }

        public override string ToString()
        {
            return "Selenium.GetValue";
        }
    }
}
