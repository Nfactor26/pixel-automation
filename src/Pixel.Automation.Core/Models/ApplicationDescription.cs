﻿using Dawn;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// ApplicationDescription stores the key aspects of an application which is being automated.
    /// </summary>  
    [DataContract]
    public class ApplicationDescription
    {
        /// <summary>
        /// Unique identifier for the application
        /// </summary>     
        public string ApplicationId
        {
            get => ApplicationDetails.ApplicationId;
        }

        /// <summary>
        /// Name of the application
        /// </summary>
        public string ApplicationName
        {
            get => ApplicationDetails.ApplicationName;
            set => ApplicationDetails.ApplicationName = value;
        }

        /// <summary>
        /// Type of application e.g. web or windows etc.
        /// </summary>
        [DataMember(Order = 10)]
        public string ApplicationType { get; set; }


        [DataMember(Order = 20)]
        public string[] SupportedPlatforms { get; set; }

        /// <summary>
        /// Details of the application. This will be based on the type e.g. web application will have preferred browser and url
        /// while a windows application will have a executable and working directory etc.
        /// </summary>
        [DataMember(Order = 30)]
        public IApplication ApplicationDetails { get; set; }

        /// <summary>
        /// Screens for the application. Screens are used to group control and prefabs.
        /// </summary>
        [DataMember(Order = 40)]
        public List<ApplicationScreen> Screens { get; private set; } = new();
       
        /// <summary>
        /// Indicates if the TestCase is deleted. Deleted test cases are not loaded in explorer.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1000)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public ApplicationDescription()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDetails">Wrapped instance of <see cref="IApplication"/></param>
        public ApplicationDescription(IApplication applicationDetails)
        {
            this.ApplicationDetails = applicationDetails;
        }
    }

    [DataContract]
    public class ApplicationScreen
    {
        [DataMember(Order = 10)]
        public string ScreenId { get; set; }

        [DataMember(Order = 20)]
        public string ScreenName { get; set; }

        [DataMember(Order = 30)]
        public List<string> AvailableControls { get; private set; } = new();

        [DataMember(Order = 40)]
        public List<string> AvailablePrefabs { get; private set; } = new();

        public ApplicationScreen()
        {

        }

        public ApplicationScreen(string screenName)
        {
            Guard.Argument(screenName, nameof(screenName)).NotNull().NotWhiteSpace();
            this.ScreenName = screenName;
            this.ScreenId = Guid.NewGuid().ToString();
        }
    }
}