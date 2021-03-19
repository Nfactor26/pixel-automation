using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class PredicateArgumentFixture
    {
        [Test]
        public void ValidateThatPredicateArugmentCanBeInitialized()
        {
            var predicateArgument = new PredicateArgument<int>();
            Assert.AreEqual(ArgumentMode.Scripted, predicateArgument.Mode);
            Assert.IsTrue(predicateArgument.CanChangeType);
            Assert.IsFalse(predicateArgument.CanChangeMode);
            Assert.AreEqual(typeof(int), predicateArgument.GetArgumentType());

            var clone = predicateArgument.Clone() as PredicateArgument<int>;
            Assert.AreNotEqual(clone, predicateArgument);
        }
    }
}
