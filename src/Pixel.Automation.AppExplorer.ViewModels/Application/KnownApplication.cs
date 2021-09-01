using System;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    public class KnownApplication
    {
        /// <summary>
        /// DisplayName of the Known Application. This is used as a menuitem header for add application menu
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Any information associated with this application that should be shown as a tooltip
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Actual IApplication type that this application represents. Type information will be used to create
        /// instance of IApplication when application is being added.
        /// </summary>
        public Type UnderlyingApplicationType { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public KnownApplication()
        {

        }

        public KnownApplication(string displayName, string description, Type underlyingApplicationType)
        {
            this.DisplayName = displayName;
            this.Description = description;
            this.UnderlyingApplicationType = underlyingApplicationType;
        }
    }
}
