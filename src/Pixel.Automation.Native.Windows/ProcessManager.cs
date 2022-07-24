using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Management;

namespace Pixel.Automation.Native.Windows
{
    /// <summary>
    /// ProcessManager is the implementation of <see cref="IProcessManager"/> contract for windows operating system.
    /// </summary>
    public class ProcessManager : IProcessManager
    {     
        /// <inheritdoc />       
        public string GetCommandLineArguments(int processId)
        {
            string wmiQuery = string.Format("select CommandLine from Win32_Process where ProcessId='{0}'", processId);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            foreach (ManagementObject retObject in retObjectCollection)
            {
                return retObject["CommandLine"].ToString();
            }
            return string.Empty;
        }

        /// <inheritdoc />
        public IEnumerable<(int, string)> GetCommandLineArguments(string processName)
        {
            string wmiQuery = string.Format("select ProcessId,CommandLine from Win32_Process where Name='{0}'", processName);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            foreach (ManagementObject retObject in retObjectCollection)
            {
                yield return (Convert.ToInt32(retObject["ProcessId"]), retObject["CommandLine"].ToString());
            }
        }
    }
}
