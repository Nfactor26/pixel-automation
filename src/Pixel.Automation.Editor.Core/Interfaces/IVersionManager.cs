using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager.Contracts;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Editor.Core.Interfaces;

public interface IVersionManager : IScreen
{
}

public interface IVersionManagerFactory
{
    IVersionManager CreateProjectVersionManager(AutomationProject automationProject);    
  
    IVersionManager CreateControlReferenceManager(IReferenceManager referenceManager);

    IVersionManager CreatePrefabReferenceManager(IReferenceManager referenceManager, Action<IEnumerable<PrefabReference>> OnPrefabsChanged);

    IVersionManager CreateAssemblyReferenceManager(IFileSystem projectFileSystem, IReferenceManager referenceManager);
}
