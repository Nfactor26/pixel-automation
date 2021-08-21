﻿using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// ApplicationDescription stores the key aspects of an application which is being automated.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationDescription : NotifyPropertyChanged
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

        /// <summary>
        /// Details of the application. This will be based on the type e.g. web application will have preferred browser and url
        /// while a windows application will have a executable and working directory etc.
        /// </summary>
        [DataMember(Order = 20)]
        public IApplication ApplicationDetails { get; set; }

        /// <summary>
        /// Identifier of the controls belonging to this application
        /// </summary>
        [DataMember(Order = 30)]
        public List<string> AvailableControls { get; private set; } = new List<string>();

        /// <summary>
        /// Identifier of the prefabs belonging to this application
        /// </summary>
        [DataMember(Order = 40)]
        public List<string> AvailablePrefabs { get; private set; } = new List<string>();

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
}