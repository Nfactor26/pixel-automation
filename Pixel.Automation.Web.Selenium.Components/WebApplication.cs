using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]  
    [DisplayName("Browser App")]
    [Description("Browser based web applications using Selenium")]
    [ControlLocator(typeof(WebControlLocatorComponent))]
    public class WebApplication : NotifyPropertyChanged,  IApplication, IDisposable
    {
        #region IApplication

        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString();

        string applicationName;
        [DataMember(IsRequired = true, Order = 20)]
        public string ApplicationName
        {
            get
            {
                return applicationName;
            }
            set
            {
                applicationName = value;
                OnPropertyChanged();
            }
        }       

        [Browsable(false)]
        public int ProcessId
        {
            get
            {
                if (this.launchedInstance != null && !this.launchedInstance.HasExited)
                {
                    return this.launchedInstance.Id;
                }
                return 0;
            }
        }

        [Browsable(false)]
        public IntPtr Hwnd
        {
            get
            {
                if (this.launchedInstance != null && !this.launchedInstance.HasExited)
                {
                    return this.launchedInstance.MainWindowHandle;
                }
                return IntPtr.Zero;
            }
        }

        #endregion IApplication

        [NonSerialized]
        Process launchedInstance;

        internal protected void SetProcessDetails(Process launchedInstance)
        {
            this.launchedInstance = launchedInstance;
        }

        [DataMember(IsRequired = true, Order = 30)]
        public Browsers PreferredBrowser { get; set; }

        [DataMember(IsRequired = true, Order = 40)]    
        public Uri TargetUri { get; set; } = new Uri("https://www.bing.com");


        [NonSerialized]
        IWebDriver webDriver;
        [BrowsableAttribute(false)]
        [IgnoreDataMember]
        public IWebDriver WebDriver
        {
            get
            {
                return webDriver;
            }

            set
            {
                this.webDriver = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                if (this.webDriver != null)
                {
                    this.webDriver.Dispose();
                    this.webDriver = null;
                }
            }
        }


        public override string ToString()
        {
            return "Web Application";
        }

    }
}
