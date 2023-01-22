using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Common;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Post process the data from a control scrapper to build required <see cref="ControlIdentity"/>
/// </summary>
public class AppiumControlIdentityBuilder : IControlIdentityBuilder
{
    /// <summary>
    /// For appium based application, we can either get a native control or a web control based on 
    /// whether we are automating a native application, browser or webview
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controlData"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IControlIdentity CreateFromData<T>(T controlData)
    {
        if (controlData is AppiumNativeControlIdentity control)   
        {
            return control;
        }
        else if (controlData is ScrapedControlData capturedData)
        {
            var controlIdentity = new AppiumWebControlIdentity()
            {
                Name = "1",
                FindByStrategy = "CssSelector",
                Identifier = capturedData.Selector
            };

            foreach (var frame in capturedData.FrameHierarchy)
            {
                var details = frame.Split(new char[] { '|' });
                FrameIdentity frameIdentity = new FrameIdentity()
                {
                    FindByStrategy = "Index",
                    Identifier = details[2]  //use index by default
                };
                frameIdentity.AvilableIdentifiers.Add(new ControlIdentifier("Id", details[0]));
                frameIdentity.AvilableIdentifiers.Add(new ControlIdentifier("Name", details[1]));
                frameIdentity.AvilableIdentifiers.Add(new ControlIdentifier("Index", details[2]));
                controlIdentity.FrameHierarchy.Add(frameIdentity);
            }
            return controlIdentity;
        }
        throw new ArgumentException($"{typeof(T)} can't be processed in to WebControlIdentity");
    }
}
