﻿using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    public interface IControlEditor
    {
        void Initialize(IControlIdentity rootControl);

        void RemoveFromControlHierarchy(IControlIdentity controlToRemove);

        void InsertIntoControlHierarchy(IControlIdentity controlIdentity);
    }
}
