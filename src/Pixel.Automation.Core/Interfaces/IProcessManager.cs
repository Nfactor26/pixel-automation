using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// IProcessManager defined the contract to perform various operations related to a <see cref="System.Diagnostics.Process"/> e.g. 
    /// get command line arguments, etc.
    /// </summary>
    public interface IProcessManager
    {
        /// <summary>
        /// Get parent process for a given processId
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        //Process GetParentProcess(int processId);

        /// <summary>
        /// Get all the child processes for a given processId
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        //IEnumerable<Process> GetChildProcesses(int processId);

        /// <summary>
        /// Get command line arguments for a given processId
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        string GetCommandLineArguments(int processId);

        /// <summary>
        /// Get (processId, commandline arguments) for all processes with a given processName
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        IEnumerable<(int, string)> GetCommandLineArguments(string processName);

    }
}