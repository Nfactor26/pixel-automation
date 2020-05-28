using Ninject;
using Pixel.Automation.Test.Runner.Modules;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.ColoredConsole()
            .WriteTo.RollingFile("logs\\Pixel-Automation-{Date}.txt")
            .CreateLogger();


            var kernel = new StandardKernel(new CommonModule(), new DevicesModule(), new WindowsModule());

            var projectManager = kernel.Get<ProjectManager>();
            projectManager.LoadProject("NotePad", "1.0.0.0");
            projectManager.LoadTestCases();

            await projectManager.Setup();
            foreach(var testCase in projectManager.GetNextTestCaseToRun(null))
            {
                await projectManager.RunTestCaseAsync(testCase);
            }
            await projectManager.TearDown();
        }
    }
}
