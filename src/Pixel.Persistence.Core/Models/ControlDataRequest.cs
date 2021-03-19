using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Persistence.Core.Models
{
    /// <summary>
    /// Request data for retrieving specified controls belonging to an application
    /// </summary>
    public class GetControlDataForApplicationRequest
    {
        /// <summary>
        /// Application Id of the owner application of control
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Collection of control id whose data needs to be retrieved
        /// </summary>
        public IEnumerable<string> ControlIdCollection { get; set; }
    }

    /// <summary>
    /// Request data for retrieving controls belonging to multiple applications
    /// </summary>
    public class GetControlDataForMultipleApplicationRequest
    {
        public IEnumerable<GetControlDataForApplicationRequest> ControlDataRequestCollection { get; set; }
    }
}
