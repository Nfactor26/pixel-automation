using System;
using Pixel.Windows.Automation;
using Pixel.Windows.Automation.Providers;
using NUnit.Framework;
using Interop.UIAutomationClient;
using UIAutomationClient = Interop.UIAutomationClient;
using SupportedTextSelection = Pixel.Windows.Automation.SupportedTextSelection;
using ZoomUnit = Pixel.Windows.Automation.ZoomUnit;

namespace UIAComWrapperTests
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class MockPatternProvider : IRawElementProviderSimple, 
        IAnnotationProvider,
        IStylesProvider,
        ISpreadsheetProvider,
        ISpreadsheetItemProvider,
        ITransformProvider2,
        IDragProvider,
        IDropTargetProvider,
        ITextProvider,
        ITextProvider2,
        ITextChildProvider
    {
        private IntPtr _hwnd;

        public MockPatternProvider(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }

        #region IRawElementProviderSimple Members

        public UIAutomationClient.ProviderOptions ProviderOptions
        {
            get
            {
                return UIAutomationClient.ProviderOptions.ProviderOptions_ClientSideProvider;
            }
        }

        public object GetPatternProvider(int patternId)
        {
            if (patternId == AnnotationPattern.Pattern.Id ||
                patternId == StylesPattern.Pattern.Id ||
                patternId == SpreadsheetPattern.Pattern.Id ||
                patternId == SpreadsheetItemPattern.Pattern.Id ||
                patternId == TransformPattern.Pattern.Id ||
                patternId == TransformPattern2.Pattern.Id ||
                patternId == DragPattern.Pattern.Id ||
                patternId == DropTargetPattern.Pattern.Id ||
                patternId == TextPattern.Pattern.Id ||
                patternId == TextPattern2.Pattern.Id ||
                patternId == TextChildPattern.Pattern.Id)
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public object GetPropertyValue(int propertyId)
        {
            if (propertyId == AutomationElement.ProviderDescriptionProperty.Id)
            {
                return "Mock Pattern Provider";
            }
            return null;
        }

        public IRawElementProviderSimple HostRawElementProvider
        {
            get
            {
                return AutomationInteropProvider.HostProviderFromHandle(this._hwnd);
            }
        }

        #endregion

        public static IRawElementProviderSimple MockPatternFactory(IntPtr hwnd, int idChild, int idObject)
        {
            return new MockPatternProvider(hwnd);
        }

        #region IAnnotationProvider members

        public AnnotationType AnnotationTypeId
        {
            get { return AnnotationType.Comment; }
        }

        public string AnnotationTypeName
        {
            get { return "Comment"; }
        }

        public string Author
        {
            get { return "John Doe"; }
        }

        public string DateTime
        {
            get { return "July 4, 1776"; }
        }

        public IRawElementProviderSimple Target
        {
            get { return this; }
        }

        #endregion

        #region IStylesProvider methods

        public StyleId StyleId
        {
            get { return StyleId.Heading5; }
        }

        public string StyleName
        {
            get { return "Heading #5"; }
        }

        public int FillColor
        {
            get { return 0x00FF00; }
        }

        public string FillPatternStyle
        {
            get { return "pinstriped"; }
        }

        public string Shape
        {
            get { return "dodecahedron"; }
        }

        public int FillPatternColor
        {
            get { return 0x0000FF; }
        }

        public string ExtendedProperties
        {
            get { return "prop1=a;prop2=b"; }
        }

        #endregion

        #region ISpreadsheetProvider methods

        public IRawElementProviderSimple GetItemByName(string name)
        {
            if (name == "primary")
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region ISpreadsheetItemProvider methods

        public string Formula
        {
            get { return "E=mc^2"; }
        }

        public IRawElementProviderSimple[] GetAnnotationObjects()
        {
            return new IRawElementProviderSimple[2] { this, this };
        }

        public AnnotationType[] GetAnnotationTypes()
        {
            return new AnnotationType[2] { AnnotationType.SpellingError, AnnotationType.GrammarError };
        }

        #endregion

        #region ITransformProvider2 methods

        double zoomLevel = 1.0;

        public void Zoom(double zoom)
        {
            if (zoom < 0.25 || zoom > 4.0)
            {
                throw new InvalidOperationException();
            }

            this.zoomLevel = zoom;
        }

        public bool CanZoom
        {
            get { return true; }
        }

        public double ZoomLevel
        {
            get { return this.zoomLevel; }
        }

        public double ZoomMinimum
        {
            get { return 0.25; }
        }

        public double ZoomMaximum
        {
            get { return 4.0; }
        }

        public void ZoomByUnit(ZoomUnit zoomUnit)
        {
            double delta = 0;
            switch (zoomUnit)
            {
                case ZoomUnit.LargeDecrement: delta = -0.25; break;
                case ZoomUnit.SmallDecrement: delta = -0.1; break;
                case ZoomUnit.LargeIncrement: delta = 0.25; break;
                case ZoomUnit.SmallIncrement: delta = 0.1; break;
                case ZoomUnit.NoAmount: delta = 0; break;
            }
            double proposed = this.zoomLevel + delta;
            if (proposed < 0.25 || proposed > 4.0)
            {
                throw new InvalidOperationException();
            }
            this.zoomLevel = proposed;
        }

        public void Move(double x, double y)
        {
            throw new NotImplementedException();
        }

        public void Resize(double width, double height)
        {
            throw new NotImplementedException();
        }

        public void Rotate(double degrees)
        {
            throw new NotImplementedException();
        }

        public bool CanMove
        {
            get { throw new NotImplementedException(); }
        }

        public bool CanResize
        {
            get { throw new NotImplementedException(); }
        }

        public bool CanRotate
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDragProvider methods

        public bool IsGrabbed
        {
            get { return true; }
        }

        public string DropEffect
        {
            get { return "Copy"; }
        }

        public string[] DropEffects
        {
            get { return new string[2] { "Copy", "Move" };  }
        }

        public IRawElementProviderSimple[] GetGrabbedItems()
        {
            return new IRawElementProviderSimple[2] { this, this };
        }

        #endregion

        #region IDropTargetProvider methods

        public string DropTargetEffect
        {
            get { return "Copy"; }
        }

        public string[] DropTargetEffects
        {
            get { return new string[2] { "Copy", "Move" }; }
        }

        #endregion

        #region ITextProvider2 

        public ITextRangeProvider[] GetSelection()
        {
            return new ITextRangeProvider[] {
                new MockTextRangeProvider(this, "selection")
            };
        }

        public ITextRangeProvider[] GetVisibleRanges()
        {
            return new ITextRangeProvider[] {
                new MockTextRangeProvider(this, "visible")
            };
        }

        public ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement)
        {
            if (childElement == null)
            {
                throw new ArgumentNullException();
            }

            return new MockTextRangeProvider(this, "fromChild");
        }

        public ITextRangeProvider RangeFromPoint(System.Windows.Point screenLocation)
        {
            return new MockTextRangeProvider(this, "fromPoint");
        }

        public ITextRangeProvider DocumentRange
        {
            get
            {
                return new MockTextRangeProvider(this, "document");
            }
        }

        public SupportedTextSelection SupportedTextSelection
        {
            get { return SupportedTextSelection.None; }
        }

        public ITextRangeProvider RangeFromAnnotation(IRawElementProviderSimple annotation)
        {
            if (annotation == null)
            {
                throw new ArgumentNullException();
            }

            return new MockTextRangeProvider(this, "fromAnnotation");
        }

        public ITextRangeProvider GetCaretRange(out bool isActive)
        {
            isActive = true;
            return new MockTextRangeProvider(this, "caret");
        }

        #endregion

        #region ITextChildProvider

        public IRawElementProviderSimple TextContainer
        {
            get { return this; }
        }

        public ITextRangeProvider TextRange
        {
            get { return RangeFromChild(this); }
        }

        #endregion
    }

    /// <summary>
    /// A very simple mock object for a text range
    /// </summary>
    public class MockTextRangeProvider : 
        ITextRangeProvider
    {
        public MockTextRangeProvider(MockPatternProvider mockObject, string text)
        {
            this.mockObject = mockObject;
            this.text = text;
        }

        private MockPatternProvider mockObject;
        private string text;

        #region ITextRangeProvider

        public ITextRangeProvider Clone()
        {
            throw new NotImplementedException();
        }

        public bool Compare(ITextRangeProvider range)
        {
            throw new NotImplementedException();
        }

        public int CompareEndpoints(Pixel.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, Pixel.Windows.Automation.Text.TextPatternRangeEndpoint targetEndpoint)
        {
            throw new NotImplementedException();
        }

        public void ExpandToEnclosingUnit(Pixel.Windows.Automation.Text.TextUnit unit)
        {
            throw new NotImplementedException();
        }

        public ITextRangeProvider FindAttribute(int attribute, object value, bool backward)
        {
            throw new NotImplementedException();
        }

        public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public object GetAttributeValue(int attribute)
        {
            throw new NotImplementedException();
        }

        public double[] GetBoundingRectangles()
        {
            throw new NotImplementedException();
        }

        public IRawElementProviderSimple GetEnclosingElement()
        {
            throw new NotImplementedException();
        }

        public string GetText(int maxLength)
        {
            if (maxLength < 0)
            {
                return this.text;
            }
            else
            {
                return this.text.Substring(0, maxLength);
            }
        }

        public int Move(Pixel.Windows.Automation.Text.TextUnit unit, int count)
        {
            throw new NotImplementedException();
        }

        public int MoveEndpointByUnit(Pixel.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, Pixel.Windows.Automation.Text.TextUnit unit, int count)
        {
            throw new NotImplementedException();
        }

        public void MoveEndpointByRange(Pixel.Windows.Automation.Text.TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, Pixel.Windows.Automation.Text.TextPatternRangeEndpoint targetEndpoint)
        {
            throw new NotImplementedException();
        }

        public void Select()
        {
            throw new NotImplementedException();
        }

        public void AddToSelection()
        {
            throw new NotImplementedException();
        }

        public void RemoveFromSelection()
        {
            throw new NotImplementedException();
        }

        public void ScrollIntoView(bool alignToTop)
        {
            throw new NotImplementedException();
        }

        public IRawElementProviderSimple[] GetChildren()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Summary description for ClientSideProvidersTest
    /// </summary>
    [TestFixture]
    public class MockPatternTests
    {
        private IntPtr targetHwnd;
        private AutomationElement mockObject;

        [SetUp]
        public void MyTestInitialize()
        {
            // Find the taskbar, which will be our target
            AutomationElement taskBar = AutomationElementTest.GetTaskbar();
            this.targetHwnd = (IntPtr)taskBar.Current.NativeWindowHandle;

            // Register a client side provider
            ClientSideProviderDescription provider = new ClientSideProviderDescription(
                new ClientSideProviderFactoryCallback(MockPatternProvider.MockPatternFactory), "Shell_TrayWnd");
            ClientSideProviderDescription[] providers = new ClientSideProviderDescription[1] { provider };
            ClientSettings.RegisterClientSideProviders(providers);

            // Get the overridden element
            this.mockObject = AutomationElement.FromHandle(this.targetHwnd);
            Assert.That(this.mockObject is not null);
        }

        [TearDown]
        public void MyTestCleanup() 
        {
            // Restore the status quo ante
            ClientSettings.RegisterClientSideProviders(new ClientSideProviderDescription[0]);
        }

        [Test]
        public void AnnotationPatternTest()
        {
            CheckWin8();
            // Get the annotation pattern
            object patternAsObj;
            AnnotationPattern pattern;
            mockObject.TryGetCurrentPattern(AnnotationPattern.Pattern, out patternAsObj);
            Assert.That(patternAsObj is not null);
            pattern = (AnnotationPattern)patternAsObj;

            // Test it
            Assert.That(pattern.Current.AnnotationTypeId, Is.EqualTo(AnnotationType.Comment));
            Assert.That(pattern.Current.AnnotationTypeName, Is.EqualTo("Comment"));
            Assert.That(pattern.Current.Author, Is.EqualTo("John Doe"));
            Assert.That(pattern.Current.DateTime, Is.EqualTo("July 4, 1776"));
            Assert.That(Automation.Compare(this.mockObject, pattern.Current.Target));
        }

        [Test]
        public void StylesPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            StylesPattern pattern = (StylesPattern)mockObject.GetCurrentPattern(StylesPattern.Pattern);

            // Test it
            Assert.That(pattern.Current.StyleId, Is.EqualTo(StyleId.Heading5));
            Assert.That(pattern.Current.StyleName, Is.EqualTo("Heading #5"));
            Assert.That(pattern.Current.FillPatternStyle, Is.EqualTo("pinstriped"));
            Assert.That(pattern.Current.FillColor, Is.EqualTo(0x00FF00));
            Assert.That(pattern.Current.FillPatternColor, Is.EqualTo(0x0000FF));
            Assert.That(pattern.Current.Shape, Is.EqualTo("dodecahedron"));
            Assert.That(pattern.Current.ExtendedProperties, Is.EqualTo("prop1=a;prop2=b"));
        }

        [Test]
        public void SpreadsheetPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            SpreadsheetPattern pattern = (SpreadsheetPattern)mockObject.GetCurrentPattern(SpreadsheetPattern.Pattern);

            // Test it
            AutomationElement result = pattern.GetItemByName("primary");
            Assert.That(result is not null);
            Assert.That(Automation.Compare(this.mockObject, result));

            Assert.That(pattern.GetItemByName("secondary") is null);
        }

        [Test]
        public void SpreadsheetItemPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            SpreadsheetItemPattern pattern = (SpreadsheetItemPattern)mockObject.GetCurrentPattern(SpreadsheetItemPattern.Pattern);

            // Test it
            Assert.That(pattern.Current.Formula, Is.EqualTo("E=mc^2"));
            AutomationElement [] annotationObjects = pattern.Current.GetAnnotationObjects();
            Assert.That(annotationObjects.Length, Is.EqualTo(2));
            Assert.That(Automation.Compare(this.mockObject, annotationObjects[0]));
            Assert.That(Automation.Compare(this.mockObject, annotationObjects[1]));
            AnnotationType[] annotationTypes = pattern.Current.GetAnnotationTypes();
            Assert.That(annotationTypes.Length, Is.EqualTo(2));
            Assert.That(annotationTypes[0], Is.EqualTo(AnnotationType.SpellingError));
            Assert.That(annotationTypes[1], Is.EqualTo(AnnotationType.GrammarError));

        }

        [Test]
        // Not sure why this one is not working -- it crashes on call into the mock Transform2
        public void TransformPattern2Test()
        {
            CheckWin8();
            // Get the  pattern
            System.Diagnostics.Debug.WriteLine(String.Format("Pattern ID is {0}", TransformPattern2.Pattern.Id));
            TransformPattern2 pattern = (TransformPattern2)mockObject.GetCurrentPattern(TransformPattern2.Pattern);

            // Test it
            Assert.That(pattern.Current.ZoomMinimum, Is.EqualTo(0.25));
            Assert.That(pattern.Current.ZoomMaximum, Is.EqualTo(4.0));
            Assert.That(pattern.Current.ZoomLevel, Is.EqualTo(1.0));
            pattern.Zoom(2.0);
            Assert.That(pattern.Current.ZoomLevel, Is.EqualTo(2.0));
            try
            {
                pattern.Zoom(10.0);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception)
            {
                Assert.Fail("Expected InvalidOperationException");
            }

            pattern.ZoomByUnit(ZoomUnit.LargeDecrement);
            Assert.That(pattern.Current.ZoomLevel, Is.EqualTo(1.75));
            pattern.ZoomByUnit(ZoomUnit.NoAmount);
            Assert.That(pattern.Current.ZoomLevel, Is.EqualTo(1.75));
        }

        [Test]
        public void DragPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            DragPattern pattern = (DragPattern)mockObject.GetCurrentPattern(DragPattern.Pattern);

            // Test it
            Assert.That(pattern.Current.IsGrabbed, Is.EqualTo(true));
            Assert.That(pattern.Current.DropEffect, Is.EqualTo("Copy"));
            Assert.That(pattern.Current.DropEffects.Length, Is.EqualTo(2));
            Assert.That(pattern.Current.DropEffects[0], Is.EqualTo("Copy"));
            Assert.That(pattern.Current.DropEffects[1], Is.EqualTo("Move"));
            AutomationElement[] grabbedItems = pattern.Current.GrabbedItems;
            Assert.That(grabbedItems.Length, Is.EqualTo(2));
            Assert.That(Automation.Compare(this.mockObject, grabbedItems[0]));
            Assert.That(Automation.Compare(this.mockObject, grabbedItems[1]));
        }

        [Test]
        public void DropTargetPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            DropTargetPattern pattern = (DropTargetPattern)mockObject.GetCurrentPattern(DropTargetPattern.Pattern);

            // Test it
            Assert.That(pattern.Current.DropTargetEffect, Is.EqualTo("Copy"));
            Assert.That(pattern.Current.DropTargetEffects.Length, Is.EqualTo(2));
            Assert.That(pattern.Current.DropTargetEffects[0], Is.EqualTo("Copy"));
            Assert.That(pattern.Current.DropTargetEffects[1], Is.EqualTo("Move"));
        }

        [Test]
        public void TextPattern2Test()
        {
            CheckWin8();
            // Get the  pattern
            TextPattern2 pattern = (TextPattern2)mockObject.GetCurrentPattern(TextPattern2.Pattern);

            // Test it
            Pixel.Windows.Automation.Text.TextPatternRange annotationRange;
            try
            {
                annotationRange = pattern.RangeFromAnnotation(null);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
            }
            catch (Exception)
            {
                Assert.Fail("Expected ArgumentNullException");
            }
            annotationRange = pattern.RangeFromAnnotation(this.mockObject);
            Assert.That(annotationRange is not null);
            Assert.That(annotationRange.GetText(-1), Is.EqualTo("fromAnnotation"));
            bool isActive;
            Pixel.Windows.Automation.Text.TextPatternRange caretRange;
            caretRange = pattern.GetCaretRange(out isActive);
            Assert.That(isActive, Is.EqualTo(true));
            Assert.That(caretRange is not null);
            Assert.That(caretRange.GetText(-1), Is.EqualTo("caret"));
        }

        [Test]
        public void TextChildPatternTest()
        {
            CheckWin8();
            // Get the  pattern
            TextChildPattern pattern = (TextChildPattern)mockObject.GetCurrentPattern(TextChildPattern.Pattern);

            // Test it
            AutomationElement container = pattern.TextContainer;
            Assert.That(Automation.Compare(this.mockObject, container));
            Pixel.Windows.Automation.Text.TextPatternRange childRange;
            childRange = pattern.TextRange;
            Assert.That(childRange is not null);
            Assert.That(childRange.GetText(-1), Is.EqualTo("fromChild"));
        }

        private void CheckWin8()
        {
            var win8version = new Version(6, 2, 9200, 0);
            if (Environment.OSVersion.Version < win8version) 
                Assert.Inconclusive("These features only available on Windows 8, can't test them here");
        }
    }
}
