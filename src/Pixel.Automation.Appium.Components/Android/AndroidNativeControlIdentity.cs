﻿using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components.Android;

/// <summary>
/// ControlIdentity capturing details of an android native control
/// </summary>
[DataContract]
[Serializable]
[ContainerEntity(typeof(AppiumControlEntity))]
public class AndroidNativeControlIdentity : AppiumNativeControlIdentity
{
    /// </inheritdoc>
    [Display(Name = "(Android) Find By", Order = 10, GroupName = "Search Strategy")]
    [Description("FindBy strategy used to search for a control")]
    [DataMember(IsRequired = true, Order = 210)]
    public override string FindByStrategy { get => base.FindByStrategy; set => base.FindByStrategy = value; }

    /// </inheritdoc>
    public override object Clone()
    {
        var clone = new AndroidNativeControlIdentity()
        {
            Name = this.Name,
            Index = this.Index,
            ApplicationId = this.ApplicationId,
            PivotPoint = this.PivotPoint,
            XOffSet = this.XOffSet,
            YOffSet = this.YOffSet,
            FindByStrategy = this.FindByStrategy,
            Identifier = this.Identifier,
            RetryAttempts = this.RetryAttempts,
            RetryInterval = this.RetryInterval,
            SearchTimeout = this.SearchTimeout,
            SearchScope = this.SearchScope,
            AvilableIdentifiers = new List<ControlIdentifier>(this.AvilableIdentifiers),
            Next = this.Next?.Clone() as AndroidNativeControlIdentity
        };        
        return clone;
    }
}
