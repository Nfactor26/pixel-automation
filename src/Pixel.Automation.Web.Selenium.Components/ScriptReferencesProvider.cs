
using OpenQA.Selenium;
using Pixel.Automation.Core.Interfaces;
using System.Reflection;

namespace Pixel.Automation.Web.Selenium.Components;


/// <inheritdoc/>
internal class ScriptReferencesProvider : IScriptReferencesProvider
{
    /// <inheritdoc/>
    public IEnumerable<string> GetAssemblyReferences()
    {
        return
        [
            Assembly.GetExecutingAssembly().Location,
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                Path.GetFileName(typeof(IWebDriver).Assembly.Location))
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetImports()
    {
        return
        [
            "Pixel.Automation.Web.Selenium.Components",
            "OpenQA.Selenium",                                                                                                           
            "OpenQA.Selenium.Appium"
        ];
    }
}
