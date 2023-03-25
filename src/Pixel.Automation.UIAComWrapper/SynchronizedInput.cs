using System;
using System.Diagnostics;
using Pixel.UIAComWrapperInternal;
using UIAutomationClient = Interop.UIAutomationClient;

namespace Pixel.Windows.Automation;

public class SynchronizedInputPattern : BasePattern
{
    private UIAutomationClient.IUIAutomationSynchronizedInputPattern _pattern;
    public static readonly AutomationEvent InputReachedTargetEvent = SynchronizedInputPatternIdentifiers.InputReachedTargetEvent;
    public static readonly AutomationEvent InputReachedOtherElementEvent = SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent;
    public static readonly AutomationEvent InputDiscardedEvent = SynchronizedInputPatternIdentifiers.InputDiscardedEvent;
    public static readonly AutomationPattern Pattern = SynchronizedInputPatternIdentifiers.Pattern;

    private SynchronizedInputPattern(AutomationElement el, UIAutomationClient.IUIAutomationSynchronizedInputPattern pattern, bool cached)
        : base(el, cached)
    {
        Debug.Assert(pattern != null);
        this._pattern = pattern;
    }

    public void Cancel()
    {
        try
        {
            this._pattern.Cancel();
        }
        catch (System.Runtime.InteropServices.COMException e)
        {
            Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
        }
    }

    public void StartListening(SynchronizedInputType type)
    {
        try
        {
            this._pattern.StartListening((UIAutomationClient.SynchronizedInputType)type);
        }
        catch (System.Runtime.InteropServices.COMException e)
        {
            Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
        }
    }

    internal static object Wrap(AutomationElement el, object pattern, bool cached)
    {
        return (pattern == null) ? null : new SynchronizedInputPattern(el, (UIAutomationClient.IUIAutomationSynchronizedInputPattern)pattern, cached);
    }
}
