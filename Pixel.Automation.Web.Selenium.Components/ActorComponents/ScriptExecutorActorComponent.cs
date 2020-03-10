using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Components;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Execute JavaScript", "Selenium", iconSource: null, description: "Execute javascript in browser and return a value if any", tags: new string[] { "Javasscript", "Web" })]
    public class ScriptExecutorActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]   
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]       
        [DisplayName("Java Script")]
        [Category("Script Settings")]        
        public Argument Script { get; set; } = new InArgument<string>();

        [DataMember]      
        [DisplayName("Script Arguments")]
        [Description("In javascript use arguments[n] to acess the argument.If ScriptExecutor is a child component of WebControl , arguments[0] will represent the HtmlElement")]
        [Category("Script Settings")]       
        public Argument Arguments { get; set; } = new InArgument<object[]>() { Mode = ArgumentMode.Scripted };

        [DataMember]      
        [Description("Return value of the executed script")]          
        public Argument Result { get; set; } = new OutArgument<string>();

       
        public ScriptExecutorActorComponent() : base("Execute JavaScript", "ScriptExecutor")
        {

        }

        public override void Act()
        {
            string scriptResult = string.Empty;
            string jsCode = ArgumentProcessor.GetValue<string>(this.Script);

            if (this.Parent is ControlEntity)
            {
                UIControl targetControl;
                if (this.TargetControl.IsConfigured())
                {
                    targetControl = ArgumentProcessor.GetValue<UIControl>(this.TargetControl);
                }
                else
                {
                    ThrowIfMissingControlEntity();
                    targetControl = this.ControlEntity.GetControl();
                }

                IWebElement control = targetControl.GetApiControl<IWebElement>();
                var arguments = ArgumentProcessor.GetValue<object[]>(this.Arguments);
                List<object> allArguments = new List<object>();
                allArguments.Add(control);
                allArguments.AddRange(arguments);
                scriptResult = (this.ApplicationDetails.WebDriver as IJavaScriptExecutor).ExecuteScript(jsCode, allArguments.ToArray())?.ToString();

            }
            else
            {
                scriptResult = (this.ApplicationDetails.WebDriver as IJavaScriptExecutor).ExecuteScript(jsCode)?.ToString();
            }
          
            ArgumentProcessor.SetValue<string>(this.Result, scriptResult == null ? string.Empty : scriptResult);

            Log.Information("Execute javascript interaction completed");
        }  

    }
}
