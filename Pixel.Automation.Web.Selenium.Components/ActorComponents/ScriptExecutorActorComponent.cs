using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="ScriptExecutorActorComponent"/> to execute java script in browser
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Execute JavaScript", "Selenium", iconSource: null, description: "Execute javascript in browser and return a value if any", tags: new string[] { "Javasscript", "Web" })]
    public class ScriptExecutorActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ScriptExecutorActorComponent>();

        /// <summary>
        /// Java script to be executed
        /// </summary>
        [DataMember]
        [Display(Name = "JavaScript", GroupName = "Configuration", Order = 20, Description = "javascript to be executed")]     
        public Argument Script { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// Specify additional arguments to be passed to javascript. These arguments can be accessed inside javascript using arguments[n].
        /// If <see cref="ScriptExecutorActorComponent"/> is a child of a <see cref="WebControlEntity"/> , arguments[0] can be used to access HtmlElement identifed by the control entity.
        /// </summary>
        [DataMember]
        [Display(Name = "Arguments", GroupName = "Configuration", Order = 30, Description = "[Optional] Additional arguments to be passed to javascript")]      
        public Argument Arguments { get; set; } = new InArgument<object[]>() { Mode = ArgumentMode.Scripted };

        /// <summary>
        /// Result argument will store the returned value from javascript execution
        /// </summary>
        [DataMember]
        [Display(Name = "Result", GroupName = "Output", Order = 10 , Description = "Argument where the result of executing javascript will be stored")]       
        public Argument Result { get; set; } = new OutArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScriptExecutorActorComponent() : base("Script Executor", "ScriptExecutor")
        {

        }

        /// <summary>
        /// Execute the configured javascript while passing any additional arguments and store the result of the javascript execution in to <b>Result</b> argument.
        /// If the parent entity is a a <see cref="WebControlEntity"/> or a <see cref="IWebElement"/> is configured using TargetElement argument, then the retrieved
        /// WebElement is passed as first parameter to javascript which can be accessed using  argument[0] and holds a javascript HtmlElement.
        /// </summary>
        public override void Act()
        {           
            string jsCode = ArgumentProcessor.GetValue<string>(this.Script);
            List<object> allArguments = new List<object>();

            if (this.Parent is IControlEntity)
            {
                IWebElement control = GetTargetControl(this.TargetControl);
                allArguments.Add(control);
            }

            if (this.Arguments.IsConfigured())
            {
                var arguments = ArgumentProcessor.GetValue<object[]>(this.Arguments);
                allArguments.AddRange(arguments);
            }

            var scriptResult = (this.ApplicationDetails.WebDriver as IJavaScriptExecutor).ExecuteScript(jsCode, allArguments.ToArray())?.ToString();
            ArgumentProcessor.SetValue<string>(this.Result, scriptResult == null ? string.Empty : scriptResult);

            logger.Information("javascript executed successfully.");
        }  

    }
}
