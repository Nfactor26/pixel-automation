using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Native.Linux;

public class ProcessManager : IProcessManager
{
    public string GetCommandLineArguments(int processId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(int, string)> GetCommandLineArguments(string processName)
    {
        throw new NotImplementedException();
    }
}
