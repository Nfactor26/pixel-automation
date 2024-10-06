using Pixel.Windows.Automation;
using NUnit.Framework;

namespace UIAComWrapperTests;

/// <summary>
///This is a test class for Conditions and is intended
///to contain all Conditions Unit Tests
///</summary>
[TestFixture]
public class ConditionTests
{
    /// <summary>
    ///A test for AndCondition
    ///</summary>
    [Test]
    public void AndConditionTest()
    {
        // Positive test
        Condition condition = Condition.TrueCondition;
        Condition condition2 = Condition.FalseCondition;
        AndCondition target = new AndCondition(condition, condition2);
        Condition[] actual;
        actual = target.GetConditions();
        Assert.That(actual is not null);
        Assert.That(actual.Length, Is.EqualTo(2));

        // Negative test - include a null
        try
        {
            target = new AndCondition(condition, null);

            Assert.Fail("expected exception");
        }
        catch (System.ArgumentException)
        {
        }
    }

    /// <summary>
    ///A test for GetConditions
    ///</summary>
    [Test]
    public void OrConditionTest()
    {
        Condition condition = Condition.TrueCondition;
        Condition condition2 = OrCondition.FalseCondition;
        OrCondition target = new OrCondition(condition, condition2);
        Condition[] actual;
        actual = target.GetConditions();
        Assert.That(actual is not null);
        Assert.That(actual.Length, Is.EqualTo(2));

        // Negative test - include a null
        try
        {
            target = new OrCondition(condition, null);

            Assert.Fail("expected exception");
        }
        catch (System.ArgumentException)
        {
        }
    }

    /// <summary>
    ///A test for NotCondition
    ///</summary>
    [Test]
    public void NotConditionTest()
    {
        Condition condition = Condition.TrueCondition;
        NotCondition target = new NotCondition(condition);
        Assert.That(target is not null);
        Condition child = target.Condition;
        Assert.That(child is not null);
    }

    /// <summary>
    ///A test for PropertyCondition
    ///</summary>
    [Test]
    public void PropertyConditionTest()
    {
        PropertyCondition cond1 = new PropertyCondition(
            AutomationElement.NameProperty, 
            "foo");
        Assert.That(cond1 is not null);
        Assert.That(cond1.Value, Is.EqualTo("foo"));
        Assert.That(cond1.Property.ProgrammaticName, Is.EqualTo("AutomationElementIdentifiers.NameProperty"));

        System.Windows.Rect rect = new System.Windows.Rect(0, 0, 20, 20);
        PropertyCondition cond2 = new PropertyCondition(
            AutomationElement.BoundingRectangleProperty,
            rect);
        Assert.That(cond2 is not null);
        object value = cond2.Value;
        Assert.That(value is double[]);
        Assert.That(((double[])value).Length, Is.EqualTo(4));
        Assert.That(cond2.Property.ProgrammaticName, Is.EqualTo("AutomationElementIdentifiers.BoundingRectangleProperty"));

        PropertyCondition cond3 = new PropertyCondition(
            AutomationElement.ClickablePointProperty,
            new System.Windows.Point(0, 0));
        Assert.That(cond3 is not null);
        value = cond3.Value;
        Assert.That(value is double[]);
        Assert.That(((double[])value).Length, Is.EqualTo(2));
        Assert.That(cond3.Property.ProgrammaticName, Is.EqualTo("AutomationElementIdentifiers.ClickablePointProperty"));

        // Negative case
        try
        {
            PropertyCondition cond4 = new PropertyCondition(
                null, null);

            Assert.Fail("expected exception");
        }
        catch (System.ArgumentException)
        {
        }



    }
}
