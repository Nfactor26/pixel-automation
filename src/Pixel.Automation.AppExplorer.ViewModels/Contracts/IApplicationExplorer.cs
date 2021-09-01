﻿using Caliburn.Micro;
using Pixel.Automation.AppExplorer.ViewModels.Application;

namespace Pixel.Automation.AppExplorer.ViewModels.Contracts
{
    /// <summary>
    /// Any view that depends on the active applicaton such as control explorer and prefab explorer should implement this interface
    /// </summary>
    public interface IApplicationAware : IScreen
    {
        void SetActiveApplication(ApplicationDescriptionViewModel applicationDescription);
    }
}
