using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class AssemblyReferences
    {
        [DataMember]
        public List<string> References { get; private set; } = new List<string>();               
     

        public string[] GetReferencesOrDefault()
        {
            if(References.Count < 1)
            {
                //References.AddRange(new [] { ".\\Core\\Pixel.Automation.Core.dll", ".\\Core\\Pixel.Automation.RunTime.dll", ".\\Components\\Pixel.Automation.Core.Components.dll", ".\\Components\\Pixel.Automation.Window.Management.dll", ".\\Components\\Pixel.Automation.Web.Selenium.Components.dll", ".\\Components\\Pixel.Automation.Input.Devices.dll", ".\\Components\\WebDriver.dll", ".\\Components\\WebDriver.Support.dll" });
                References.AddRange(new[] { ".\\Pixel.Automation.Core.dll", ".\\Pixel.Automation.RunTime.dll", 
                    ".\\Pixel.Automation.Core.Components.dll", ".\\Pixel.Automation.Window.Management.dll", 
                    ".\\Pixel.Automation.Web.Selenium.Components.dll", 
                    ".\\Pixel.Automation.Input.Devices.dll", ".\\WebDriver.dll", ".\\WebDriver.Support.dll" });
            }
            return References.ToArray();
        }

        public void AddReferences(IEnumerable<string> references)
        {
            foreach(var reference in references)
            {
                if(!References.Contains(reference))
                {
                    References.Add(reference);
                }
            }
        }
      
    }   
    
}
