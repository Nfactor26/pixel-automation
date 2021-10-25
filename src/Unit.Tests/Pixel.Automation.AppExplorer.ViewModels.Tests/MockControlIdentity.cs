﻿using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    class MockControlIdentity : IControlIdentity
    {
        public string ControlImage { get; set; }
        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public Rectangle BoundingBox { get; set; }
        public int RetryAttempts { get; set; }
        public int RetryInterval { get; set; }
        public Pivots PivotPoint { get; set; }
        public double XOffSet { get; set; }
        public double YOffSet { get; set; }
        public LookupType LookupType { get; set; }
        public IControlIdentity Next { get; set; }

        public object Clone()
        {
            return new MockControlIdentity()
            {
                Name = this.Name,
                ControlImage = this.ControlImage,
                ApplicationId = this.ApplicationId,
                BoundingBox = this.BoundingBox,
                RetryAttempts = this.RetryAttempts,
                RetryInterval = this.RetryInterval,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                LookupType = this.LookupType
            };
        }

        public string GetControlName()
        {
            return this.Name;
        }
    }
}
