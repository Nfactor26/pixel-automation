using System;
using System.Runtime.InteropServices;
using Pixel.Windows.Automation;
using Pixel.Windows.Automation.Text;
using NUnit.Framework;

namespace UIAComWrapperTests;

/// <summary>
/// ScenarioTests: intended to contain tests that manipulate UI
/// and are therefore less reliable than pure unit tests.
///</summary>
[TestFixture]
public class ScenarioTests
{
    [Test]
    public void ExpandCollapsePatternTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            // Find a well-known combo box
            AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
            Assert.That(combo is not null);

            ExpandCollapsePattern expando = (ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern);
            Assert.That(expando.Current.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed));
            expando.Expand();
            System.Threading.Thread.Sleep(100 /* ms */);
            Assert.That(expando.Current.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Expanded));
            expando.Collapse();
            System.Threading.Thread.Sleep(100 /* ms */);
            Assert.That(expando.Current.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed));
        }
    }

    [Test]
    public void ExpandCollapsePatternCachedTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            CacheRequest req = new CacheRequest();
            req.Add(ExpandCollapsePattern.Pattern);
            req.Add(ExpandCollapsePattern.ExpandCollapseStateProperty);
            using (req.Activate())
            {
                // Find a well-known combo box
                AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
                Assert.That(combo is not null);

                ExpandCollapsePattern expando = (ExpandCollapsePattern)combo.GetCachedPattern(ExpandCollapsePattern.Pattern);
                Assert.That(expando.Cached.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed));
            }
        }
    }

    [Test]
    public void NoClickablePointTest()
    {
        // Launch a notepad and position it
        using (AppHost host1 = new AppHost("notepad.exe", ""))
        {
            TransformPattern transformPattern1 = (TransformPattern)host1.Element.GetCurrentPattern(TransformPattern.Pattern);
            transformPattern1.Move(0, 0);
            transformPattern1.Resize(400, 300);

            System.Windows.Point pt1 = host1.Element.GetClickablePoint();

            // Launch a second notepad and position it on top
            using (AppHost host2 = new AppHost("notepad.exe", ""))
            {
                TransformPattern transformPattern2 = (TransformPattern)host2.Element.GetCurrentPattern(TransformPattern.Pattern);
                transformPattern2.Move(0, 0);
                transformPattern2.Resize(400, 300);

                // Now try it again for host1
                try
                {
                    System.Windows.Point pt1again = host1.Element.GetClickablePoint();
                    Assert.Fail("expected exception");
                }
                catch (NoClickablePointException)
                {
                }
            }
        }
    }

    [Test]
    public void RangeValuePatternTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL main.cpl ,2"))
        {
            // Find a well-known slider
            AutomationElement slider = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "101"));
            Assert.That(slider is not null);

            RangeValuePattern range = (RangeValuePattern)slider.GetCurrentPattern(RangeValuePattern.Pattern);
            double originalValue = range.Current.Value;
            try
            {
                Assert.That(range.Current.SmallChange >= 0);
                Assert.That(range.Current.LargeChange >= 0);
                Assert.That(originalValue >= range.Current.Minimum);
                Assert.That(originalValue <= range.Current.Maximum);
                Assert.That(range.Current.IsReadOnly == false);
                range.SetValue(range.Current.Minimum);
                System.Threading.Thread.Sleep(100 /* ms */);
                Assert.That(range.Current.Value, Is.EqualTo(range.Current.Minimum));

                range.SetValue(range.Current.Maximum);
                System.Threading.Thread.Sleep(100 /* ms */);
                Assert.That(range.Current.Value, Is.EqualTo(range.Current.Maximum));

                double midpoint = (range.Current.Maximum + range.Current.Minimum) / 2;
                range.SetValue(midpoint);
                System.Threading.Thread.Sleep(100 /* ms */);
                Assert.That(range.Current.Value, Is.EqualTo(midpoint));
            }
            finally
            {
                range.SetValue(originalValue);
            }
        }
    }

    public void RangeValuePatternCachedTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL main.cpl ,2"))
        {
            CacheRequest req = new CacheRequest();
            req.Add(RangeValuePattern.Pattern);
            req.Add(RangeValuePattern.IsReadOnlyProperty);
            req.Add(RangeValuePattern.MaximumProperty);
            req.Add(RangeValuePattern.MinimumProperty);
            req.Add(RangeValuePattern.SmallChangeProperty);
            req.Add(RangeValuePattern.LargeChangeProperty);
            using (req.Activate())
            {
                // Find a well-known slider
                AutomationElement slider = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "101"));
                Assert.That(slider is not null);

                RangeValuePattern range = (RangeValuePattern)slider.GetCachedPattern(RangeValuePattern.Pattern);
                double originalValue = range.Cached.Value;
                Assert.That(range.Cached.SmallChange >= 0);
                Assert.That(range.Cached.LargeChange >= 0);
                Assert.That(originalValue >= range.Cached.Minimum);
                Assert.That(originalValue <= range.Cached.Maximum);
                Assert.That(range.Cached.IsReadOnly == false);
            }
        }
    }

    [Test]
    [Ignore("")]
    public void TextPatternTest()
    {
        System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

        // Fragile -- I'm open to a better way of doing this.
        using (AppHost host = new AppHost("xpsrchvw.exe", "..\\..\\..\\UiaComWrapperTests\\bin\\debug\\test.xps"))
        {
            AutomationElement mainContent = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, true));
            TextPattern text = (TextPattern)mainContent.GetCurrentPattern(TextPattern.Pattern);
            Assert.That(text.SupportedTextSelection, Is.EqualTo(SupportedTextSelection.Single));

            TextPatternRange range1 = text.DocumentRange;
            Assert.That(range1 is not null);
            Assert.That(text, Is.EqualTo(range1.TextPattern));
            TextPatternRange range2 = range1.Clone();
            Assert.That(range2 is not null);
            Assert.That(range1.Compare(range2));
            Assert.That (0 == range1.CompareEndpoints(TextPatternRangeEndpoint.Start, range2, TextPatternRangeEndpoint.Start));
            Assert.That(0 == range1.CompareEndpoints(TextPatternRangeEndpoint.End, range2, TextPatternRangeEndpoint.End));

            string keyString = "Constitution of the United States";
            TextPatternRange range3 = range1.FindText(keyString, false, true);
            Assert.That(range3 is not null);
            string foundString = range3.GetText(-1);
            Assert.That(foundString, Is.EqualTo(keyString));
            range3.Select();
            TextPatternRange[] selectedRanges = text.GetSelection();
            Assert.That(selectedRanges.Length, Is.EqualTo(1));
            TextPatternRange selectedRange = selectedRanges[0];
            Assert.That(range3.Compare(selectedRange));

            // Test attributes.  Casts will fail if types are wrong
            System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)range3.GetAttributeValue(TextPattern.CultureAttribute);
            string fontName = (string)range3.GetAttributeValue(TextPattern.FontNameAttribute);
            bool hiddenValue = (bool)range3.GetAttributeValue(TextPattern.IsItalicAttribute);
            Assert.That(range3.GetAttributeValue(TextPattern.IsHiddenAttribute), Is.EqualTo(AutomationElement.NotSupported));

            TextPatternRange range5 = range1.FindAttribute(TextPattern.IsItalicAttribute, true, false /* backward */);
            Assert.That(range5 is not null);
            Assert.That(range5.GetText(-1), Is.EqualTo("Note"));

            range5.ExpandToEnclosingUnit(TextUnit.Line);
            string line5 = range5.GetText(-1);
            Assert.That(line5, Is.EqualTo("Preamble Note "));

            System.Windows.Rect[] rects = range3.GetBoundingRectangles();
            Assert.That(rects.Length, Is.EqualTo(1));
            Assert.That(rects[0].Width > 0);
            Assert.That(rects[0].Height > 0);
        }
    }

    [Test]
    public void TogglePatternTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL main.cpl ,2"))
        {
            // Find a well-known checkbox
            AutomationElement checkbox = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "109"));
            Assert.That(checkbox is not null);

            TogglePattern toggle = (TogglePattern)checkbox.GetCurrentPattern(TogglePattern.Pattern);
            ToggleState originalState = toggle.Current.ToggleState;
            toggle.Toggle();
            // Slight wait for effect
            System.Threading.Thread.Sleep(100 /* ms */);
            ToggleState currentState = toggle.Current.ToggleState;
            Assert.That(currentState, Is.Not.EqualTo(originalState));

            // Put it back
            while (currentState != originalState)
            {
                toggle.Toggle();
                System.Threading.Thread.Sleep(100 /* ms */);
                currentState = toggle.Current.ToggleState;
            }
        }
    }

    public void TogglePatternCachedTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL main.cpl"))
        {
            CacheRequest req = new CacheRequest();
            req.Add(TogglePattern.Pattern);
            req.Add(TogglePattern.ToggleStateProperty);
            using (req.Activate())
            {
                // Find a well-known checkbox
                AutomationElement checkbox = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "114"));
                Assert.That(checkbox is not null);

                TogglePattern toggle = (TogglePattern)checkbox.GetCachedPattern(TogglePattern.Pattern);
                ToggleState originalState = toggle.Cached.ToggleState;
                Assert.That(originalState == ToggleState.On || originalState == ToggleState.Off);
            }
        }
    }

    [Test]
    public void TransformPatternTest()
    {
        // Launch a notepad and position it
        using (AppHost host = new AppHost("notepad.exe", ""))
        {
            TransformPattern transformPattern = (TransformPattern)host.Element.GetCurrentPattern(TransformPattern.Pattern);
            // Coded to expectations for an explorer window
            Assert.That(transformPattern.Current.CanMove);
            Assert.That(transformPattern.Current.CanResize);
            Assert.That(transformPattern.Current.CanRotate == false);
            
            // Little move
            transformPattern.Move(10, 10);

            // Little resize
            transformPattern.Resize(200, 200);
        }
    }

    [Test]
    public void TransformPatternCachedTest()
    {
        using (AppHost host = new AppHost("notepad.exe", ""))
        {
            CacheRequest req = new CacheRequest();
            req.Add(TransformPattern.Pattern);
            req.Add(TransformPattern.CanMoveProperty);
            req.Add(TransformPattern.CanResizeProperty);
            req.Add(TransformPattern.CanRotateProperty);
            using (req.Activate())
            {
                AutomationElement cachedEl = host.Element.GetUpdatedCache(req);

                TransformPattern transformPattern = (TransformPattern)cachedEl.GetCachedPattern(TransformPattern.Pattern);
                // Coded to expectations for an explorer window
                Assert.That(transformPattern.Cached.CanMove);
                Assert.That(transformPattern.Cached.CanResize);
                Assert.That(transformPattern.Cached.CanRotate == false);

                // Little move
                transformPattern.Move(10, 10);

                // Little resize
                transformPattern.Resize(200, 200);
            }
        }
    }

    [Test]
    public void ValuePatternTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            // Find a well-known combo box
            AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
            Assert.That(combo is not null);

            ValuePattern value = (ValuePattern)combo.GetCurrentPattern(ValuePattern.Pattern);
            Assert.That(value.Current.IsReadOnly == false);
            Assert.That(value.Current.Value.Length > 0);
        }
    }

    [Test]
    public void ValuePatternCachedTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            CacheRequest req = new CacheRequest();
            req.Add(WindowPattern.Pattern);
            req.Add(WindowPattern.CanMaximizeProperty);
            req.Add(WindowPattern.CanMinimizeProperty);
            req.Add(WindowPattern.IsTopmostProperty);
            req.Add(WindowPattern.WindowInteractionStateProperty);
            req.Add(WindowPattern.WindowVisualStateProperty);
            using (req.Activate())
            {
                // Find a well-known combo box
                AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
                Assert.That(combo is not null);

                ValuePattern value = (ValuePattern)combo.GetCurrentPattern(ValuePattern.Pattern);
                Assert.That(value.Current.IsReadOnly == false);
                Assert.That(value.Current.Value.Length > 0);
            }
        }
    }

    [Test]
    public void WindowPatternTest()
    {
        using (AppHost host = new AppHost("notepad.exe", ""))
        {
            // Window Pattern
            WindowPattern windowPattern = (WindowPattern)host.Element.GetCurrentPattern(WindowPattern.Pattern);
            Assert.That(windowPattern.Current.CanMaximize);
            Assert.That(windowPattern.Current.CanMinimize);
            Assert.That(windowPattern.Current.IsTopmost == false);
            Assert.That(windowPattern.Current.WindowVisualState, Is.Not.EqualTo(WindowVisualState.Minimized));
            Assert.That(windowPattern.Current.WindowInteractionState, Is.Not.EqualTo(WindowInteractionState.Closing));
        }
    }

    [Test]
    public void WindowPatternCachedTest()
    {
        using (AppHost host = new AppHost("notepad.exe", ""))
        {
            CacheRequest req = new CacheRequest();
            req.Add(WindowPattern.Pattern);
            req.Add(WindowPattern.CanMaximizeProperty);
            req.Add(WindowPattern.CanMinimizeProperty);
            req.Add(WindowPattern.IsTopmostProperty);
            req.Add(WindowPattern.WindowInteractionStateProperty);
            req.Add(WindowPattern.WindowVisualStateProperty);
            using (req.Activate())
            {
                AutomationElement cachedEl = host.Element.GetUpdatedCache(req);

                // Window Pattern
                WindowPattern windowPattern = (WindowPattern)cachedEl.GetCachedPattern(WindowPattern.Pattern);
                Assert.That(windowPattern.Cached.CanMaximize);
                Assert.That(windowPattern.Cached.CanMinimize);
                Assert.That(windowPattern.Cached.IsTopmost == false);
                Assert.That(windowPattern.Cached.WindowVisualState, Is.Not.EqualTo(WindowVisualState.Minimized));
                Assert.That(windowPattern.Cached.WindowInteractionState, Is.Not.EqualTo(WindowInteractionState.Closing));
            }
        }
    }

    [Test]
    public void LegacyIAccessiblePatternTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            // Find a well-known combo box
            AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
            Assert.That(combo is not null);

            // BLOCK
            {
                LegacyIAccessiblePattern acc = (LegacyIAccessiblePattern)combo.GetCurrentPattern(LegacyIAccessiblePattern.Pattern);
                Assert.That(acc.Current.ChildId, Is.EqualTo(0));
                Assert.That(acc.Current.Name.Length > 0);
                Assert.That(acc.Current.Value.Length > 0);
                Assert.That(acc.Current.KeyboardShortcut, Is.EqualTo("Alt+f"));
                Assert.That(acc.Current.Role, Is.EqualTo((uint)0x2E));
                Assert.That(acc.Current.State & 0x100400, Is.EqualTo((uint)0x100400));
            }

            // Get the tab controls
            AutomationElement tabCtrl = host.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "12320"));
            Assert.That(tabCtrl is not null);

            // Try out the selection
            // BLOCK
            {
                LegacyIAccessiblePattern acc = (LegacyIAccessiblePattern)tabCtrl.GetCurrentPattern(LegacyIAccessiblePattern.Pattern);
                AutomationElement[] selection = acc.Current.GetSelection();
                Assert.That(selection.Length > 0);
                Assert.That(selection[0] is AutomationElement);
                Assert.That(ControlType.TabItem, Is.EqualTo(selection[0].Current.ControlType));
            }

        }
    }

    [Test]
    public void LegacyIAccessiblePatternCachedTest()
    {
        using (AppHost host = new AppHost("rundll32.exe", "shell32.dll,Control_RunDLL intl.cpl"))
        {
            CacheRequest req = new CacheRequest();
            req.Add(LegacyIAccessiblePattern.Pattern);
            req.Add(LegacyIAccessiblePattern.NameProperty);
            req.Add(LegacyIAccessiblePattern.ChildIdProperty);
            req.Add(LegacyIAccessiblePattern.KeyboardShortcutProperty);
            req.Add(LegacyIAccessiblePattern.RoleProperty);
            req.Add(LegacyIAccessiblePattern.ValueProperty);
            req.Add(LegacyIAccessiblePattern.StateProperty);
            req.Add(LegacyIAccessiblePattern.SelectionProperty);
            using (req.Activate())
            {
                // Find a well-known combo box
                AutomationElement combo = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "1021"));
                Assert.That(combo is not null);

                // BLOCK
                {
                    LegacyIAccessiblePattern acc = (LegacyIAccessiblePattern)combo.GetCachedPattern(LegacyIAccessiblePattern.Pattern);
                    Assert.That(acc.Cached.ChildId, Is.EqualTo(0));
                    Assert.That(acc.Cached.Name.Length > 0);
                    Assert.That(acc.Cached.Value.Length > 0);
                    Assert.That(acc.Cached.KeyboardShortcut, Is.EqualTo("Alt+f"));
                    Assert.That(acc.Cached.Role, Is.EqualTo((uint)0x2E));
                    Assert.That(acc.Cached.State & 0x100400 , Is.EqualTo((uint)0x100400));
                }

                // Get the tab controls
                AutomationElement tabCtrl = host.Element.FindFirst(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "12320"));
                Assert.That(tabCtrl is not null);

                // BLOCK
                {
                    LegacyIAccessiblePattern acc = (LegacyIAccessiblePattern)tabCtrl.GetCachedPattern(LegacyIAccessiblePattern.Pattern);
                    AutomationElement[] selection = acc.Cached.GetSelection();
                    Assert.That(selection.Length > 0);
                    Assert.That(selection[0] is AutomationElement);
                    Assert.That(ControlType.TabItem, Is.EqualTo(selection[0].Current.ControlType));
                }

            }
        }
    }

    [DllImport("oleacc.dll")]
    internal static extern int AccessibleObjectFromWindow(
         IntPtr hwnd,
         uint id,
         ref Guid iid,
         [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

    [Test]
    public void IAccessibleInterop()
    {
        // Get the clock
        AutomationElement taskbar = AutomationElement.RootElement.FindFirst(TreeScope.Children,
            new PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd"));
        Assert.That(taskbar is not null);

        AutomationElement clock = taskbar.FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "TrayClockWClass"));
        Assert.That(clock is not null);

        // Get the IAccessible for the clock
        IntPtr clockHwnd = (IntPtr)clock.Current.NativeWindowHandle;
        Guid iidAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
        object obj = null;
        int retVal = AccessibleObjectFromWindow(clockHwnd, (uint)0xFFFFFFFC, ref iidAccessible, ref obj);
        Assert.That(obj is not null);
        Accessibility.IAccessible accessible = (Accessibility.IAccessible)obj;
        Assert.That(accessible is not null);
        Assert.That(accessible.get_accRole(0), Is.EqualTo(0x2B));

        // Convert to an element
        AutomationElement element = AutomationElement.FromIAccessible(accessible, 0);
        Assert.That(element is not null);
        Assert.That(ControlType.Button, Is.EqualTo(element.Current.ControlType));

        // Round-trip: let's get the IAccessible back out
        LegacyIAccessiblePattern legacy = (LegacyIAccessiblePattern)element.GetCurrentPattern(LegacyIAccessiblePattern.Pattern);
        Accessibility.IAccessible legacyIAcc = legacy.GetIAccessible();
        Assert.That(legacyIAcc is not null);
        Assert.That(legacyIAcc.get_accRole(0), Is.EqualTo(0x2B));
    }

}
