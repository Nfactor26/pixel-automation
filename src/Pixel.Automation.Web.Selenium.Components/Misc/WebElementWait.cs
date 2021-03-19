using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Pixel.Automation.Web.Selenium.Components
{
    //public class WebElementWait : DefaultWait<IWebElement>
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="WebElementWait"/> class.
    //    /// </summary>
    //    /// <param name="element">The WebElement instance used to wait.</param>
    //    /// <param name="timeout">The timeout value indicating how long to wait for the condition.</param>
    //    public WebElementWait(IWebElement element, TimeSpan timeout) : base(element, new SystemClock())
    //    {
    //        this.Timeout = timeout;
    //        this.IgnoreExceptionTypes(typeof(NotFoundException));
    //    }
    //}

    public class WebElementWait : DefaultWait<ISearchContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebElementWait"/> class.
        /// </summary>
        /// <param name="element">The WebElement instance used to wait.</param>
        /// <param name="timeout">The timeout value indicating how long to wait for the condition.</param>
        public WebElementWait(ISearchContext element, TimeSpan timeout) : base(element, new SystemClock())
        {
            this.Timeout = timeout;
            this.IgnoreExceptionTypes(typeof(NotFoundException));
        }
    }
}
