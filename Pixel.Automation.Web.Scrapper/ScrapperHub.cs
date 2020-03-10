using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Web.Selenium.Components;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Scrapper
{
    [HubName("scrapper")]
    public class ScrapperHub : Hub
    {
        public static string ApplicationId { get; set; }

        static ConcurrentQueue<ScrapedControl> capturedControls = new ConcurrentQueue<ScrapedControl>();

        private readonly IScreenCapture screenCapture;

        public ScrapperHub(IScreenCapture screenCapture)
        {
            this.screenCapture = screenCapture;
        }

        public static IEnumerable<ScrapedControl> GetCapturedControls()
        {
            var capturedIdentities = capturedControls.ToList<ScrapedControl>();
            while (capturedControls.TryDequeue(out ScrapedControl result))
            {

            }

            return capturedIdentities;

        }

        public override Task OnConnected()
        {
            Log.Information("Session connected to Scrapper Hub");
            return base.OnConnected();
        }

        public void AddWebControlDetails(ScrapedData captureData)
        {          
            WebControlIdentity controlIdentity = new WebControlIdentity()
            {
                Name = Guid.NewGuid().ToString(),               
                FindByStrategy = "CssSelector",
                Identifier = captureData.Identifier,
                BoundingBox = new Rectangle(captureData.Left, captureData.Top, captureData.Width, captureData.Height)
            };


            foreach (var frame in captureData.FrameHierarchy)
            {
                var details = frame.Split(new char[] { '|' });
                FrameIdentity frameIdentity = new FrameIdentity()
                {
                    FindByStrategy = "Index",
                    Identifier =  details[2]  //use id by default
                };
                frameIdentity.AvilableIdentifiers?.Add(new ControlIdentifier("Id", details[0]));
                frameIdentity.AvilableIdentifiers?.Add(new ControlIdentifier("Name", details[1]));
                frameIdentity.AvilableIdentifiers?.Add(new ControlIdentifier("Index", details[2]));

                controlIdentity.FrameHierarchy.Add(frameIdentity);
            }


            Bitmap controlImage = screenCapture.CaptureArea(controlIdentity.BoundingBox);

            ScrapedControl scrapedControl = new ScrapedControl() { ControlData = controlIdentity, ControlImage = controlImage };
            capturedControls.Enqueue(scrapedControl);

            Log.Information("Recevied control with identifier : {identifier}", controlIdentity.Identifier);
        }

    }
}
