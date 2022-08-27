using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Controls
{
    /// <summary>
    /// <see cref="UIControl"/> wraps a ui framework specific control e.g. AutomationElement from UIA or IWebElement from WebDriver
    /// and helps to retrieve it's bounding box or a clickable point inside the control.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class UIControl
    {
        protected object TargetControl { get; set; }

        /// <summary>
        /// Get the underlying control represented by the automation framework in use e.g. IWebElement for Webdriver, Automation element for UIA , etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public virtual T GetApiControl<T>()
        {
            if (this.TargetControl is T requiredControl)
            {
                return requiredControl;
            }
            throw new InvalidCastException($"{typeof(T)} is incompatible with {TargetControl.GetType()}");
        }

        /// <summary>
        /// Get the bounding box of the control. Bounding box can be used to show a highglight rectangle around control.
        /// </summary>
        /// <returns></returns>
        public abstract Task<BoundingBox> GetBoundingBoxAsync();

        /// <summary>
        /// Get a clickable point on the control. Bounding box of the control is used along with the configured pivot and offset to locate this point.
        /// Clickable point can be used with different mouse operations e.g. click, mouse over, etc.
        /// </summary>
        /// <returns></returns>
        public abstract Task<(double,double)> GetClickablePointAsync();

        /// <summary>
        /// Check if a UI Control is visible
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> IsVisibleAsync();

        /// <summary>
        /// Check if a UI Control is visible
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> IsHiddenAsync();

        /// <summary>
        /// Check if a UI Control is enabled
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> IsEnabledAsync();

        /// <summary>
        /// Check if a UI Control is disabled
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> IsDisabledAsync();

        /// <summary>
        /// Check if a control has checked state
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> IsCheckedAsync();
    }
}
