using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Decisions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Test.Helpers;
using System.Linq;

namespace Pixel.Automation.Core.Components.Tests
{   
    public class IfEntityTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void AssertThatIfEntityReturnsCorrectComponents(bool evaluatesTo)
        {
          
            var entityManager = Substitute.For<IEntityManager>();
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult(evaluatesTo));
            entityManager.GetScriptEngine().Returns(scriptEngine);

            var ifEntity = new IfEntity() { EntityManager = entityManager };
            ifEntity.ResolveDependencies();

            Assert.AreEqual(2, ifEntity.Components.Count());

            var ifComponent = Substitute.For<ActorComponent>();
            var elseComponent = Substitute.For<ActorComponent>();

            (ifEntity.GetComponentsByName("Then").Single() as Entity).AddComponent(ifComponent);
            (ifEntity.GetComponentsByName("Else").Single() as Entity).AddComponent(elseComponent);


            var components = ifEntity.GetNextComponentToProcess().ToList();
         
            if (evaluatesTo)
            {
                Assert.IsTrue(components.Contains(ifComponent));
            }
            else
            {
                Assert.IsTrue(components.Contains(elseComponent));
            }       

        }
    }
}
