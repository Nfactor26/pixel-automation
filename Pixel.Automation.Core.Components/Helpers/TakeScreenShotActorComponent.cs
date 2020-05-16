using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Helpers
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Take ScreenShot", "Utility", iconSource: null, description: "Take a screenshot of desktop/given area", tags: new string[] { "ScreenShot", "Utility" })]
    public class TakeScreenShotActorComponent : ActorComponent
    {
        //TODO : Consider breaking this down in to target directory and filename fields
        [DataMember]      
        [Description("Absolute path of the directory where screenshot should be saved")]
        [Display(Name = "Save Location", GroupName = "Input", Order = 10)]
        public InArgument<string> SaveLocation { get; set; } = new InArgument<string>();
       

        public TakeScreenShotActorComponent() : base("Take ScreenShot", "TakeScreenShot")
        {

        }

        public override void Act()
        {
            IApplication targetApplication = this.EntityManager.GetApplicationDetails(this);
            string applicationName = targetApplication.ApplicationName;
            var argumentProcessor = this.ArgumentProcessor;
            string saveLocation = argumentProcessor.GetValue<string>(this.SaveLocation);       
            if (string.IsNullOrEmpty(saveLocation))
            {
                throw new ArgumentException("SaveLocation can't be empty for TaksScreenShot Actor");
            }

            //TODO : Need a better default location where images should be saved.
            IScreenCapture sreenCapture = this.EntityManager.GetServiceOfType<IScreenCapture>();
            using (Bitmap screenShot = sreenCapture.CaptureDesktop())
            {
                string saveAt = Path.Combine(saveLocation, this.GetRootEntity().Id.ToString(), applicationName);
                if (!Directory.Exists(saveAt))
                {
                    Directory.CreateDirectory(saveAt);
                }
                screenShot.Save($"{saveAt}\\{this.Id}.png");
            }         
           
        }

    }
}

