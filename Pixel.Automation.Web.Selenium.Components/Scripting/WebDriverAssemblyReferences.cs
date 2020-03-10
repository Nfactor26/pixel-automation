using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Web.Selenium.Components.Scripting
{
    public class WebDriverAssemblyReferences : IScriptReferencesProvider
    {
        public List<Assembly> GetAssembliesToReference()
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(typeof(WebApplication).Assembly); //this assembly
            assemblies.Add(typeof(IWebDriver).Assembly);  //WebDriver.dll
            assemblies.Add(typeof(WebDriverWait).Assembly); //WebDriver.Support.dll
            return assemblies;
        }
    }
}
