using Pixel.Automation.Core.Interfaces;

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
                    Identifier = capturedData.PlaywrightSelector ?? capturedData.Selector
                };


                foreach (var frame in capturedData.FrameHierarchy)
                {
                    var details = frame.Split(new char[] { '|' });
                    FrameIdentity frameIdentity = new FrameIdentity()
                    {                       
                        Identifier = details[3]  //use playwright selector by default
                    };                    
                    controlIdentity.FrameHierarchy.Add(frameIdentity);
                }
                return controlIdentity;
            }
            throw new ArgumentException($"{typeof(T)} can't be processed in to WebControlIdentity");
        }
    }
}
