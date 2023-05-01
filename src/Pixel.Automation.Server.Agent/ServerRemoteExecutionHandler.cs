using Dawn;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;

namespace pixel_execution_managers.Handlers;

/// <summary>
/// Execution handler to run test cases on server machines
/// </summary>
internal class ServerRemoteExecutionHandler : ITestExecutionHandler
{
    private readonly ILogger logger = Log.ForContext<ServerRemoteExecutionHandler>();   
    private readonly IConfiguration config;
  
    public string Name => "server-remote";
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="config"></param>
    public ServerRemoteExecutionHandler(IConfiguration config)
    {
        this.config = Guard.Argument(config, nameof(config)).NotNull().Value;
    }

    /// </inheritdoc>    
    public async Task ExecuteTestAsync(string templateName)
    {
        if (Process.GetProcessesByName("pixel-run.exe").Any())
        {
            throw new Exception("pixel-run.exe is already running");
        }
        string executable = config["pixel_run_executable"];
        var process = Process.Start(new ProcessStartInfo()
        {
            WorkingDirectory = Path.GetDirectoryName(executable),
            FileName = executable,
            Arguments = $"run template \"{templateName}\""
        });       
        if(process != null)
        {
            logger.Information("pixel-run was started with process Id : {0}", process.Id);
        }
        else
        {
            throw new Exception("Process didn't start");
        }
        await Task.CompletedTask;
    }
}
