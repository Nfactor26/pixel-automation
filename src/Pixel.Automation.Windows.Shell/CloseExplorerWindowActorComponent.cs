using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace Pixel.Automation.Windows.Shell;

[DataContract]
[Serializable]
[ToolBoxItem("Close Explorer Window", "Windows Shell", iconSource: null, description: "Closes an open Windows Explorer window matching the given path", tags: ["Explorer", "Close", "Window"])]
public class CloseExplorerWindowActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<CloseExplorerWindowActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the Explorer window to close")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    public CloseExplorerWindowActorComponent() : base("Close Explorer Window", "CloseExplorerWindow")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The specified path is null or empty.");
        }

        //TODO: This might not be the most reliable way to find the Explorer window for a given path since multiple windows can have similar titles.
        // Explore alternatives like iterating the shell windows using COM interfaces if needed.
       
        // Explorer windows typically have their path in the window title
        var explorerWindows = windowManager.FindAllDesktopWindows(path, Pixel.Automation.Core.Enums.MatchType.Contains, true);
        var matchingWindow = explorerWindows.FirstOrDefault(w => w.WindowTitle.Contains(path, System.StringComparison.OrdinalIgnoreCase));
        if (matchingWindow == null)
        {
            throw new InvalidOperationException($"No open Explorer window found for path: {path}");
        }
        // Close the window by sending a close command
        PostMessage(matchingWindow.HWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        logger.Information($"Closed Explorer window for path: {path}");
    }

    const uint WM_CLOSE = 0x0010;

    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}


