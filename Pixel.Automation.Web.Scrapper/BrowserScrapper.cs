using Caliburn.Micro;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Scrapper
{
    public class BrowserScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        IDisposable signalR;
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
                if (isCapturing)
                {
                    StartCapture();
                }
                else
                {
                    StopCapture();
                }

            }
        }

        string targetApplicationId = string.Empty;

        private readonly IEventAggregator eventAggregator;
        private readonly IScreenCapture screenCapture;

        public BrowserScrapper(IEventAggregator eventAggregator, IScreenCapture screenCapture)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.screenCapture = screenCapture;
        }

        public IEnumerable<object> GetCapturedControls()
        {
            throw new NotImplementedException();
        }

        public void StartCapture()
        {
            try
            {
                GlobalHost.DependencyResolver = new DefaultDependencyResolver(); //without this restarting server fails silently since dependency resolver is disposed as well 
                GlobalHost.DependencyResolver.Register(typeof(ScrapperHub), () => new ScrapperHub(screenCapture));
                signalR = WebApp.Start<Startup>("http://localhost:9143/");             
            }
            catch (Exception ex)
            {
               Log.Error(ex,ex.Message);                
            }
        }

        public void StopCapture()
        {
            var connectionManager = GlobalHost.DependencyResolver.Resolve<IConnectionManager>();
            var scrapperHub =  connectionManager.GetHubContext<ScrapperHub>();
            scrapperHub.Clients.All.onServerClosing(); //this will stop scrapping if active on the client

            //stop server after a while so that message is sent
            Task stopServerTask = new Task(() =>
            {
                Thread.Sleep(2000);
                signalR?.Dispose();               
                signalR = null;       
            });
            stopServerTask.Start();
             
            eventAggregator.PublishOnUIThreadAsync(ScrapperHub.GetCapturedControls());
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
        }
    }
}
