using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Id of the application
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the application
        /// </summary>
        string applicationName;
        [DataMember(IsRequired = true)]
        [Display(Name = "Application Name", Order = 20, Description = "Name of the application")]      
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

        /// <summary>
        /// Process Id of the launched browser application
        /// </summary>
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

        /// <summary>
        /// Handle of the launched browser window
        /// </summary>
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

        /// <summary>
        /// Browser to use for test execution
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Preferred Browser", Order = 30, Description = "Browser to use")]
        public Browsers PreferredBrowser { get; set; }

        /// <summary>
        /// Web application url
        /// </summary>
        [DataMember(IsRequired = true)]    
        [Display(Name = "Target Url", Order = 40, Description = "Web application url")]
        public Uri TargetUri { get; set; } = new Uri("https://www.bing.com");


        [NonSerialized]
        IWebDriver webDriver;

        /// <summary>
        /// <see cref="IWebDriver"/> used to interact with the browser
        /// </summary>
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

        /// <summary>
        /// Dispose webdriver
        /// </summary>
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
