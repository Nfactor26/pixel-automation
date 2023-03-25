using System;
using System.Diagnostics;
using Pixel.Windows.Automation.Text;
using Pixel.UIAComWrapperInternal;
using UIAutomationClient = Interop.UIAutomationClient;

namespace Pixel.Windows.Automation;

public class TextChildPattern : BasePattern
{
    private UIAutomationClient.IUIAutomationTextChildPattern _pattern;
    public static readonly AutomationPattern Pattern = TextChildPatternIdentifiers.Pattern;

    private TextChildPattern(AutomationElement el, UIAutomationClient.IUIAutomationTextChildPattern pattern, bool cached)
        : base(el, cached)
    {
        Debug.Assert(pattern != null);
        this._pattern = pattern;
    }

    public AutomationElement TextContainer
    {
        get
        {
            try
            {
                return AutomationElement.Wrap(this._pattern.TextContainer);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
    }

    public TextPatternRange TextRange
    {
        get
        {
            try
            {
                AutomationElement textContainer = this.TextContainer;
                TextPattern textPattern = (TextPattern)textContainer.GetCurrentPattern(TextPattern.Pattern);
                return TextPatternRange.Wrap(this._pattern.TextRange, textPattern);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
    }

    internal static object Wrap(AutomationElement el, object pattern, bool cached)
    {
        return (pattern == null) ? null : new TextChildPattern(el, (UIAutomationClient.IUIAutomationTextChildPattern)pattern, cached);
    }
}
