using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Common;
using System.Drawing;

namespace Pixel.Automation.Web.Playwright.Components
{
    public class WebControlIdentityBuilder : IControlIdentityBuilder
    {
        public IControlIdentity CreateFromData<T>(T controlData)
        {
            if(controlData is ScrapedControlData capturedData)
            {
                WebControlIdentity controlIdentity = new WebControlIdentity()
                {
                    Name = "1",                  
                    Identifier = capturedData.Identifier,
                    BoundingBox = new Rectangle(capturedData.Left, capturedData.Top, capturedData.Width, capturedData.Height)
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
}
