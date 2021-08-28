using Caliburn.Micro;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Image.Scrapper
{
    public class ImageControlScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        private readonly ILogger logger = Log.ForContext<ImageControlScrapper>();

        private readonly IEventAggregator eventAggregator;
        private readonly IScreenCapture screenCapture;
        private readonly IWindowManager windowManager;
        private readonly IImageCaptureViewModel imageCaptureViewModel;

        bool isCapturing;
        public bool IsCapturing
        {
            get => isCapturing;
            set
            {
                isCapturing = value;
                NotifyOfPropertyChange(() => IsCapturing);
            }
        }
      
        public string DisplayName => "Image Capture";

        string targetApplicationId = string.Empty;
       
        public ImageControlScrapper(IEventAggregator eventAggregator, IWindowManager windowManager, IScreenCapture screenCapture, IImageCaptureViewModel imageCaptureViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnUIThread(this);         
            this.imageCaptureViewModel = imageCaptureViewModel;
            this.windowManager = windowManager;
            this.screenCapture = screenCapture;
        }


        public IEnumerable<object> GetCapturedControls()
        {
            throw new NotImplementedException();
        }


        public async Task ToggleCapture()
        {         
            if (IsCapturing)
            {
                await StartCapture();
            }
            else
            {
                await StopCapture();
            }
        }


        public async Task StartCapture()
        {           
            System.Windows.Application.Current.MainWindow.Hide();
            Thread.Sleep(1000);

            using Bitmap desktopScreenShot = screenCapture.CaptureDesktop();
            imageCaptureViewModel.Initialize(desktopScreenShot);       
            var result = await windowManager.ShowDialogAsync(imageCaptureViewModel);
            
            if(result.HasValue && result.Value==true)
            {
                var controlDetails = imageCaptureViewModel.GetCapturedImageControl();
                ScrapedControl scrapedControl = new ScrapedControl()
                {
                    ControlData = controlDetails,
                    ControlImage = CropImage(desktopScreenShot, controlDetails.BoundingBox)
                };
                await eventAggregator.PublishOnUIThreadAsync(new List<ScrapedControl>() { scrapedControl });
            }                
            System.Windows.Application.Current.MainWindow.Show();
            Thread.Sleep(200);

            await StopCapture();
        }

        Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        public async Task StopCapture()
        {
            IsCapturing = false;
            NotifyOfPropertyChange(() => IsCapturing);
            await Task.CompletedTask;
        }

        public bool CanToggleScrapper
        {
            get
            {
                return !(string.IsNullOrEmpty(this.targetApplicationId));
            }
        }

        public async Task HandleAsync(RepositoryApplicationOpenedEventArgs message, CancellationToken cancellationToken)
        {
            targetApplicationId = message.ApplicationId;
            NotifyOfPropertyChange(() => CanToggleScrapper);
            await Task.CompletedTask;
        }
    }
}
