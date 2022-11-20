using System;
using System.Collections.Generic;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// Request data for retrieving specified controls belonging to an application
/// </summary>
public class GetControlsForApplicationRequest
{
    /// <summary>
    /// Application Id of the owner application of control
    /// </summary>
    public string ApplicationId { get; set; }

    /// <summary>
    /// Filter criteria to get only those controls that have been modified after this time
    /// </summary>
    public DateTime laterThan { get; set; }
}

/// <summary>
/// Request data for retrieving controls belonging to multiple applications
/// </summary>
public class GetControlDataForMultipleApplicationRequest
{
    public IEnumerable<GetControlsForApplicationRequest> ControlDataRequestCollection { get; set; }
}
