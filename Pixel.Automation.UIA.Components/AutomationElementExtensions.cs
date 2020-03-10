extern alias uiaComWrapper;
using System.Collections.Generic;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public static class AutomationElementExtensions
    {       
        public static void Invoke(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                InvokePattern invokePattern = automationElement.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();
            }
        }

        public static void Toggle(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                TogglePattern togglePattern = automationElement.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
                togglePattern.Toggle();
            }
        }

        public static void Expand(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                ExpandCollapsePattern expandPattern = automationElement.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                expandPattern.Expand();
            }
        }


        public static void Collapse(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                ExpandCollapsePattern collapsePattern = automationElement.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                collapsePattern.Collapse();
            }
        }

       
        public static void SetValue(this AutomationElement automationElement, string value)
        {
            if (automationElement != null)
            {
                ValuePattern valuePattern = automationElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                valuePattern.SetValue(value);
            }
        }


        public static string GetValue(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                ValuePattern valuePattern = automationElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                return valuePattern.Current.Value;
            }
            return string.Empty;
        }


        public static void Select(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                SelectionItemPattern selectionItemPattern = automationElement.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                selectionItemPattern.Select();
            }
        }

        public static void AddToSelection(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                SelectionItemPattern selectionItemPattern = automationElement.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                selectionItemPattern.AddToSelection();
            }
        }

        public static void RemoveFromSelection(this AutomationElement automationElement)
        {
            if (automationElement != null)
            {
                SelectionItemPattern selectionItemPattern = automationElement.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                selectionItemPattern.RemoveFromSelection();
            }
        }

        public static void Scroll(this AutomationElement automationElement, ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            if (automationElement != null)
            {
                ScrollPattern scrollPattern = automationElement.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
                scrollPattern.Scroll(horizontalAmount, verticalAmount);
            }
        }

        public static void ResizeTo(this AutomationElement automationElement, double sizeX, double sizeY)
        {
            if (automationElement != null)
            {
                TransformPattern transformPattern = automationElement.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                transformPattern.Resize(sizeX, sizeY);
            }
        }

        public static void MoveTo(this AutomationElement automationElement, double posX, double posY)
        {
            if (automationElement != null)
            {
                TransformPattern transformPattern = automationElement.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                transformPattern.Move(posX, posY);
            }
        }

        public static void RotateBy(this AutomationElement automationElement, double degrees)
        {
            if (automationElement != null)
            {
                TransformPattern transformPattern = automationElement.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                transformPattern.Rotate(degrees);
            }
        }

        public static List<AutomationElement> ToList(this AutomationElementCollection automationElementCollection)
        {
            List<AutomationElement> automationElements = new List<AutomationElement>();
            foreach(AutomationElement automationElement in automationElementCollection)
            {
                automationElements.Add(automationElement);
            }
            return automationElements;
        }

    }
}
