using System.ComponentModel;
using System.Runtime.Serialization;
using BaseApplication = Pixel.Automation.Core.Application;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    [DisplayName("Windows App")]
    [Description("WPF or Win32 based applications using UIA")]
    class MockApplication : BaseApplication
    {
        string executablePath;
        /// <summary>
        /// Path of the executable file
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Description("Path of the executable file")]
        public string ExecutablePath
        {
            get => executablePath;
            set => executablePath = value;
        }       
    }
}
