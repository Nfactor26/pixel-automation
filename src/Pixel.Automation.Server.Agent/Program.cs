// See https://aka.ms/new-console-template for more information
using Dawn;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pixel.Automation.Server.Agent;
using pixel_execution_managers.Handlers;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

Log.Logger = new LoggerConfiguration()         
             .WriteTo.File("logs/pixel-agent-.txt", restrictedToMinimumLevel: LogEventLevel.Verbose,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Thread:{ThreadId}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
             .WriteTo.Console(LogEventLevel.Information, "{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}")
             .CreateLogger();
var logger = Log.ForContext<Program>();

using IHost host = Host.CreateDefaultBuilder(args).Build();

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, false)
    .AddEnvironmentVariables()
    .Build();

if ( args.Length != 3)
{
    throw new ArgumentException("Expected 3 parameters - agentName, group and hubEndpoint");
}

string? executable = config.GetValue<string?>("pixel_run_executable");
if (string.IsNullOrEmpty(executable))
{
    throw new Exception("Executable path is not configured.");
}
if (!File.Exists(executable))
{
    throw new Exception($"Executable doesn't exist at path {executable}");
}

var agent = new {
    Name = args[0],
    Group = args[1],
    MachineName = Environment.MachineName,
    OSDescription = RuntimeInformation.OSDescription,
    RegisteredHadlers = new string[] { "server-remote" }
};

var connection = new HubConnectionBuilder()
    .WithUrl(args[2]).WithAutomaticReconnect(new ConnectionRetryPolicy())
    .Build();
await connection.StartAsync();
await connection.InvokeAsync("RegisterAgent", agent);

connection.On("CanExecuteNew", () =>
{
    bool canExecuteNew =  !Process.GetProcessesByName("pixel-run.exe").Any();
    logger.Information("Replied {0} to CanExecuteNew query", canExecuteNew);
    return canExecuteNew;
});

connection.On<string, string>("ExecuteTemplate", async (template, handler) =>
{
    try
    {
        Guard.Argument(template, nameof(template)).NotNull().NotEmpty();
        Guard.Argument(handler, nameof(handler)).NotNull().NotEmpty();
        logger.Information("Received request to execute template {0} with handler {1}", template, handler);   
        if(!handler.Equals("server-remote"))
        {
            throw new Exception($"Agent doesn't have a handler for type : {handler}");
        }
        var executionHadler = new ServerRemoteExecutionHandler(config);
        await executionHadler.ExecuteTestAsync(template);      
    }
    catch (Exception ex)
    {
        logger.Error(ex, "An error occured while trying to process requeste to execute template {0} with handler {1}", template, handler);        
    }
});

connection.Closed += OnConnectionClosed;
connection.Reconnected += OnReconnected;

async Task OnReconnected(string arg)
{
    try
    {
        await connection.InvokeAsync("AgentReconnected", agent);
        logger.Information("Agent is reconnected now - {0}", arg);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "There was an error while processing reconnected event");
    }
}

async Task OnConnectionClosed(Exception ex)
{
    logger.Error(ex, "Agent is disconencted now");
    await Task.CompletedTask;
}

await host.RunAsync();

logger.Information("Agent is shutting down");
await connection.InvokeAsync("DeRegisterAgent", agent);
