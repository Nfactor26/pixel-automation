﻿using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IAutomationEditor : IEditor, IDisposable
    {
        AutomationProject CurrentProject
        {
            get;
        }

        /// <summary>
        /// Load project from the current workspace
        /// </summary>
        /// <param name="project"></param>
        Task DoLoad(AutomationProject project, VersionInfo versionInfo = null);


        /// <summary>
        /// Open script editor that can be used to modify the initialization script for the automation project.
        /// </summary>
        /// <returns></returns>
        Task EditScriptAsync();

    }
}