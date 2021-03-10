using Caliburn.Micro;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Pixel.Automation.Web.Scrapper
{
    public class BrowserScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        public string DisplayName
        {
            get
            {
                return "Browser - Any";
            }
        }

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


        string targetApplicationId = string.Empty;

        private readonly IEventAggregator eventAggregator;
        private readonly IScreenCapture screenCapture;
        private IHost host;

        public BrowserScrapper(IEventAggregator eventAggregator, IScreenCapture screenCapture)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnPublishedThread(this);
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
            try
            {
                host = CreateHostBuilder(null).Build();
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.ConfigureServices(a => a.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(IScreenCapture), this.screenCapture)))
                 .ConfigureKestrel(serverOptions =>
                 {
                     serverOptions.ListenLocalhost(9143);
                 })
                 .UseStartup<Startup>();
             });

        public async Task StopCapture()
        {
            //stop server after a while so that message is sent
            Task stopServerTask = new Task(() =>
            {
                Thread.Sleep(2000);
                host?.StopAsync();
            });
            stopServerTask.Start();

            await eventAggregator.PublishOnUIThreadAsync(ScrapperHub.GetCapturedControls());
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
            ScrapperHub.ApplicationId = targetApplicationId;
            NotifyOfPropertyChange(() => CanToggleScrapper);
            await Task.CompletedTask;
        }
    }
}
