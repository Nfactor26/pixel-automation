using Dawn;
using Pixel.Automation.Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public static class AutomationElementExtensions
    {          
        static object GetAutomationPattern(AutomationElement automationElement, AutomationPattern automationPattern)
        {
            if(!automationElement.TryGetCurrentPattern(automationPattern, out object pattern))
            {
                var supportedPatterns = automationElement.GetSupportedPatterns();
                var supportedPatternsString = string.Join(", ", supportedPatterns.Select(p => p.ProgrammaticName));
                throw new InvalidOperationException($"{automationPattern.ProgrammaticName} is not supported by control. Supported patterns are [ {supportedPatternsString} ]");
            }
            return pattern;
        }

        public static void Invoke(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            InvokePattern invokePattern = GetAutomationPattern(automationElement, InvokePattern.Pattern) as InvokePattern;
            invokePattern.Invoke();
        }

        public static void Toggle(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            TogglePattern togglePattern = GetAutomationPattern(automationElement, TogglePattern.Pattern) as TogglePattern;
            togglePattern.Toggle();
        }

        public static bool IsControlExpanded(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ExpandCollapsePattern collapsePattern = GetAutomationPattern(automationElement, ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            return collapsePattern.Current.ExpandCollapseState == ExpandCollapseState.Expanded;
        }

        public static void Expand(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ExpandCollapsePattern expandPattern = GetAutomationPattern(automationElement, ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            expandPattern.Expand();
        }

        public static bool IsControlCollapsed(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ExpandCollapsePattern collapsePattern = GetAutomationPattern(automationElement, ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            return collapsePattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed;
        }

        public static void Collapse(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ExpandCollapsePattern collapsePattern = GetAutomationPattern(automationElement, ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            collapsePattern.Collapse();
        }

       
       
        public static void SetValue(this AutomationElement automationElement, string value)
        {
            Guard.Argument(automationElement).NotNull();
            ValuePattern valuePattern = GetAutomationPattern(automationElement, ValuePattern.Pattern) as ValuePattern;
            valuePattern.SetValue(value);           
        }


        public static string GetValue(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ValuePattern valuePattern = GetAutomationPattern(automationElement, ValuePattern.Pattern) as ValuePattern;
            return valuePattern.Current.Value ?? string.Empty;           
        }


        public static void Select(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            SelectionItemPattern selectionItemPattern = GetAutomationPattern(automationElement, SelectionItemPattern.Pattern) as SelectionItemPattern;
            selectionItemPattern.Select();        
        }

        public static void AddToSelection(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            SelectionItemPattern selectionItemPattern = GetAutomationPattern(automationElement, SelectionItemPattern.Pattern) as SelectionItemPattern;
            selectionItemPattern.AddToSelection();
        }

        public static void RemoveFromSelection(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            SelectionItemPattern selectionItemPattern = GetAutomationPattern(automationElement, SelectionItemPattern.Pattern) as SelectionItemPattern;
            selectionItemPattern.RemoveFromSelection();           
        }

        public static bool CanScrollInDirection(this AutomationElement automationElement, ScrollDirection scrollDirection)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollPattern scrollPattern = GetAutomationPattern(automationElement, ScrollPattern.Pattern) as ScrollPattern;
            switch(scrollDirection)
            {
                case ScrollDirection.Down:
                    return scrollPattern.Current.VerticallyScrollable && scrollPattern.Current.VerticalScrollPercent < 100;                
                case ScrollDirection.Up:
                    return scrollPattern.Current.VerticallyScrollable && scrollPattern.Current.VerticalScrollPercent > 0;
                case ScrollDirection.Right:
                    return scrollPattern.Current.HorizontallyScrollable && scrollPattern.Current.HorizontalScrollPercent < 100;
                case ScrollDirection.Left:
                    return scrollPattern.Current.HorizontallyScrollable && scrollPattern.Current.HorizontalScrollPercent > 0;
                default:
                    throw new ArgumentException($"{scrollDirection} is not supported");
            }           
        }

        public static bool CanScrollHorizontally(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollPattern scrollPattern = GetAutomationPattern(automationElement, ScrollPattern.Pattern) as ScrollPattern;
            return scrollPattern.Current.HorizontallyScrollable;
        }

        public static bool CanScrollVertically(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollPattern scrollPattern = GetAutomationPattern(automationElement, ScrollPattern.Pattern) as ScrollPattern;
            return scrollPattern.Current.VerticallyScrollable;
        }

        public static void Scroll(this AutomationElement automationElement, ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollPattern scrollPattern = GetAutomationPattern(automationElement, ScrollPattern.Pattern) as ScrollPattern;
            scrollPattern.Scroll(horizontalAmount, verticalAmount);           
        }

        public static void ScrollToPercent(this AutomationElement automationElement, double horizontalAmount, double verticalAmount)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollPattern scrollPattern = GetAutomationPattern(automationElement, ScrollPattern.Pattern) as ScrollPattern;
            scrollPattern.SetScrollPercent(horizontalAmount, verticalAmount);
        }

        public static void ScrollIntoView(this AutomationElement automationElement)
        {
            Guard.Argument(automationElement).NotNull();
            ScrollItemPattern scrollItemPattern = GetAutomationPattern(automationElement, ScrollItemPattern.Pattern) as ScrollItemPattern;
            scrollItemPattern.ScrollIntoView();
        }

        public static void ResizeTo(this AutomationElement automationElement, double width, double height)
        {
            Guard.Argument(automationElement).NotNull();
            TransformPattern transformPattern = GetAutomationPattern(automationElement, TransformPattern.Pattern) as TransformPattern;
            transformPattern.Resize(width, height);       
        }

        public static void MoveTo(this AutomationElement automationElement, double posX, double posY)
        {
            Guard.Argument(automationElement).NotNull();
            TransformPattern transformPattern = GetAutomationPattern(automationElement, TransformPattern.Pattern) as TransformPattern;
            transformPattern.Move(posX, posY);          
        }

        public static void RotateBy(this AutomationElement automationElement, double degrees)
        {
            Guard.Argument(automationElement).NotNull();
            TransformPattern transformPattern = GetAutomationPattern(automationElement, TransformPattern.Pattern) as TransformPattern;
            transformPattern.Rotate(degrees);           
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


        public static int GetDescendantsCount(this AutomationElement automationElement)
        {
            var allElements = automationElement.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            return allElements.Count;
        }

        /// <summary>
        /// Find all descendants control of a root node matching specified search criteria. This is a BFS search and looks one element at a time unlike FindAll()
        /// This is used during scraping to find the index of target node without having to find all descendants control of root node.
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="matchCondition"></param>
        /// <returns></returns>
        public static IEnumerable<AutomationElement> FindAllDescendants(this AutomationElement rootNode, Condition matchCondition)
        {
            Queue<AutomationElement> queue = new Queue<AutomationElement>();
            queue.Enqueue(rootNode);
            while (queue.Count > 0)
            {
                AutomationElement node = queue.Dequeue();
                foreach (AutomationElement childNode in node.FindAll(TreeScope.Children, Condition.TrueCondition))
                {
                    //if the element matches search condition, return this match
                    if(childNode.FindFirst(TreeScope.Element, matchCondition) != null)
                    {
                        yield return childNode;
                    }
                    queue.Enqueue(childNode);
                }
            }
            yield break;
        }
    }
}
