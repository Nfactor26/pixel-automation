using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class ScriptStatus
    {
        public string ScriptName { get; set; }

        public bool IsValid { get; private set; }

        public string Diagnostics { get; private set; }

        public void UpdateStatus(bool isValid, string errors)
        {
            this.IsValid = isValid;
            this.Diagnostics = errors;
        }
    }
}
