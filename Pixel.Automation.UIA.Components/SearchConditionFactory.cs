using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public static class ConditionExtensions
    {
        public static OrCondition OrPropertyCondition(this Condition condition, PropertyCondition orCondition)
        {
            return new OrCondition(condition, orCondition);
        }

        public static AndCondition AndPropertyCondition(this Condition condition, PropertyCondition andCondition)
        {
            return new AndCondition(condition, andCondition);
        }

        public static AndCondition AndName(this Condition condition, string name)
        {
            return new AndCondition(condition, ConditionFactory.FromName(name));
        }

        public static AndCondition AndAutomationId(this Condition condition, string automationId)
        {
            return new AndCondition(condition, ConditionFactory.FromAutomationId(automationId));
        }

        public static AndCondition AndProcessId(this Condition condition, int processId)
        {
            return new AndCondition(condition, ConditionFactory.FromProcessId(processId));
        }

        public static AndCondition AndControlType(this Condition condition, ControlType controlType)
        {
            return new AndCondition(condition, ConditionFactory.FromControlType(controlType));
        }

        public static OrCondition OrControlType(this Condition condition, ControlType controlType)
        {
            return new OrCondition(condition, ConditionFactory.FromControlType(controlType));
        }

        public static AndCondition AndClassName(this Condition condition, string className)
        {
            return new AndCondition(condition, ConditionFactory.FromClassName(className));
        }

        public static AndCondition AndAccessKey(this Condition condition, string acessKey)
        {
            return new AndCondition(condition, ConditionFactory.FromAccessKey(acessKey));
        }

        public static AndCondition AndHelpText(this Condition condition, string helpText)
        {
            return new AndCondition(condition, ConditionFactory.FromHelpText(helpText));
        }

        public static AndCondition AndAcceleratorKey(this Condition condition, string acceleratorKey)
        {
            return new AndCondition(condition, ConditionFactory.FromHelpText(acceleratorKey));
        }
    }

    public class ConditionFactory
    {

        public static PropertyCondition FromName(string name)
        {
            return new PropertyCondition(AutomationElement.NameProperty, name);
        }

        public static PropertyCondition FromAutomationId(string automationID)
        {
            return new PropertyCondition(AutomationElement.AutomationIdProperty, automationID);
        }

        public static PropertyCondition FromProcessId(int processId)
        {
            return new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
        }

        public static PropertyCondition FromControlType(ControlType controlType)
        {
            return new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
        }

        public static PropertyCondition FromClassName(string className)
        {
            return new PropertyCondition(AutomationElement.ClassNameProperty, className);
        }

        public static PropertyCondition FromAccessKey(string accessKey)
        {
            return new PropertyCondition(AutomationElement.AccessKeyProperty, accessKey);
        }

        public static PropertyCondition FromHelpText(string helpText)
        {
            return new PropertyCondition(AutomationElement.HelpTextProperty, helpText);
        }

        //public static PropertyCondition FromLabeledBy(string labledBy)
        //{
        //    return new PropertyCondition(AutomationElement.LabeledByProperty, labledBy);
        //}

        public static PropertyCondition FromAcceleratorKey(string acceleratorKey)
        {
            return new PropertyCondition(AutomationElement.AcceleratorKeyProperty, acceleratorKey);
        }

    }
}
