﻿using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="ScriptExecutorActorComponent"/> to execute script
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Execute Script", "Appium", iconSource: null, description: "Execute script and return a value if any", tags: new string[] { "execute", "script" })]
public class ScriptExecutorActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScriptExecutorActorComponent>();

    /// <summary>
    /// Java script to be executed
    /// </summary>
    [DataMember]
    [Display(Name = "Script", GroupName = "Configuration", Order = 10, Description = "script to be executed")]     
    public Argument Script { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Specify additional arguments to be passed to javascript. These arguments can be accessed inside javascript using arguments[n].
    /// If <see cref="ScriptExecutorActorComponent"/> is a child of a <see cref="WebControlEntity"/> , arguments[0] can be used to access HtmlElement identifed by the control entity.
    /// </summary>
    [DataMember]
    [Display(Name = "Arguments", GroupName = "Configuration", Order = 20, Description = "[Optional] Additional arguments to be passed to script")]      
    public Argument Arguments { get; set; } = new InArgument<object[]>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.Scripted };

    /// <summary>
    /// Result argument will store the returned value from javascript execution
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10 , Description = "[Optional] Argument where the result of executing script will be stored")]       
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
    public override async Task ActAsync()
    {           
        string jsCode = await ArgumentProcessor.GetValueAsync<string>(this.Script);
        List<object> allArguments = new List<object>();

        if (this.Parent is IControlEntity)
        {
            var (name, control) = await GetTargetControl();
            allArguments.Add(control);
        }

        if (this.Arguments.IsConfigured())
        {
            var arguments = await ArgumentProcessor.GetValueAsync<object[]>(this.Arguments);
            allArguments.AddRange(arguments);
        }

        var scriptResult = (this.ApplicationDetails.Driver as IJavaScriptExecutor).ExecuteScript(jsCode, allArguments.ToArray())?.ToString();
        if(this.Result.IsConfigured())
        {
            await ArgumentProcessor.SetValueAsync<string>(this.Result, scriptResult == null ? string.Empty : scriptResult);
        }
        logger.Information("Javascript was executed successfully");
    }  

}
