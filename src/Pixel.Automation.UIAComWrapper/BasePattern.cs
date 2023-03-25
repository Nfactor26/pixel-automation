using System.Diagnostics;

namespace Pixel.Windows.Automation;

public class BasePattern
{
    internal AutomationElement _el;
    internal bool _cached;

    internal BasePattern(AutomationElement el, bool cached)
    {
        Debug.Assert(el != null);
        this._el = el;
        this._cached = cached;
    }
}
