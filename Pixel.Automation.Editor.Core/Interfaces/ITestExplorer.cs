using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core
{
    public interface ITestExplorer : IDisposable
    {
        void SetActiveInstance(object instance);

        void CloseActiveInstance();

        bool HasTestCaseOpenForEdit();
    }
}
