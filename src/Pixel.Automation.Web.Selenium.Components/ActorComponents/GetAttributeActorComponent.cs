using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="GetAttributeActorComponent"/> to retrieve the value of any attribute from a web control.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Attribute", "Selenium", iconSource: null, description: "Get the value of user defined attribute of a WebElement", tags: new string[] { "Attribute", "Attribute value", "Value", "Get", "Web" })]
    public class GetAttributeActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<GetAttributeActorComponent>();

        /// <summary>
        /// Name of the attribute whose value needs to be retrieved
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Attribute", GroupName = "Configuration", Order = 20, Description = "Name of the attribute whose value needs to be retrieved")]     
        public string AttributeName { get; set; }

        /// <summary>
        /// Argument where the value of the attribute will be stored
        /// </summary>
        [DataMember]
        [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Store the result in specified Argument")]       
        public Argument Result { get; set; } = new OutArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public GetAttributeActorComponent() : base("Get Attribute", "GetAttribute")
        {

        }

        /// <summary>
        /// Retrieve the value of configured attribute from <see cref="IWebElement"/>
        /// </summary>
        public override void Act()
        {
            IWebElement control = GetTargetControl();
            string extractedValue = control.GetAttribute(this.AttributeName);
            ArgumentProcessor.SetValue<string>(Result, extractedValue);

            logger.Information($"Retrived  attribue : {this.AttributeName} from control.");

        }

        public override string ToString()
        {
            return "Get Attribute Actor";
        }
    }
}
