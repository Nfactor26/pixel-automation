using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IControlLocator
    {
        //bool ControlExists(IControlIdentity controlIdentity);

        //T FindControl<T>(IControlIdentity controlIdentity);

        //IReadOnlyCollection<T> FindControls<T>(IControlIdentity controlIdentity);

        bool CanProcessControlOfType(IControlIdentity controlIdentity);
      
        //void PushSearchRoot(IControlIdentity controlIdentity);

        //IControlIdentity PopSearchRoot();

        //bool HasActiveSearchRoot(IControlIdentity controlIdentity);

        //void ClearSearchRoots();
    }

    public interface IControlLocator<out T,in U> : IControlLocator 
        where T : class
        where U : class       
    {
        T FindControl(IControlIdentity controlIdentity, U searchRoot);

        IEnumerable<T> FindAllControls(IControlIdentity controlIdentity, U searchRoot);

        //T FindAncestorControl(IControlIdentity controlIdentity, U searchRoot);

        //T FindDescendantControl(IControlIdentity controlIdentity, U searchRoot);

        //IReadOnlyCollection<T> FindAllDescendantControls(IControlIdentity controlIdentity, U searchRoot);

        //T FindSiblingControl(IControlIdentity controlIdentity, U searchRoot);

        //IReadOnlyCollection<T> FindAllSiblingControls(IControlIdentity controlIdentity, U searchRoot);
    }
}
