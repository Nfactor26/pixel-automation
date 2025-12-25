#r "Pixel.Automation.Core.dll"
#r "Pixel.Automation.UIAComWrapper.dll"
#r "Pixel.Automation.UIA.Components.dll"

using Pixel.Windows.Automation;
using Pixel.Automation.UIA.Components;
using Pixel.Automation.Core.Controls;

/// <summary>
/// Get the AutomationElement wrapped by the given UIControl.   
/// </summary>
AutomationElement GetAutomationElement(UIControl control)
{
    return control.GetApiControl<AutomationElement>();
}

bool HasDescendantWithName(UIControl control, string name, TreeScope treeScope = TreeScope.Children)
{  
    var condition = ConditionFactory.FromName(name);
    return HasDescendantMatchingCondition(control, condition, treeScope);
}

bool HasDescendantWithAutomationId(UIControl control, string automationId, TreeScope treeScope = TreeScope.Children)
{ 
    var condition = ConditionFactory.FromAutomationId(automationId);
    return HasDescendantMatchingCondition(control, condition, treeScope);
}

bool HasDescendantWithClassName(UIControl control, string className, TreeScope treeScope = TreeScope.Children)
{ 
    var condition = ConditionFactory.FromClassName(className);
    return HasDescendantMatchingCondition(control, condition, treeScope);
}

bool HasDescendantWithControlType(UIControl control, ControlType controlType, TreeScope treeScope = TreeScope.Children)
{ 
    var condition = ConditionFactory.FromControlType(controlType);
    return HasDescendantMatchingCondition(control, condition, treeScope);
}

bool HasDescendantWithHelpText(UIControl control, string helpText, TreeScope treeScope = TreeScope.Children)
{ 
    var condition = ConditionFactory.FromHelpText(helpText);
    return HasDescendantMatchingCondition(control, condition, treeScope);
}

bool HasDescendantMatchingCondition(UIControl control, Condition condition, TreeScope treeScope)
{
    var automationElement = GetAutomationElement(control);    
    var childElement = automationElement.FindFirst(treeScope, condition);
    return childElement != null;
}