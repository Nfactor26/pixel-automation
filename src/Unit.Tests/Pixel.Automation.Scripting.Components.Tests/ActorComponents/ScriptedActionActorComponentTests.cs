using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components.Tests.ActorComponents
{
    public class ScriptedActionActorComponentTests
    {
        [Test]
        public async Task CanAct()
        {
            var entityManager = Substitute.For<IEntityManager>();

            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Action<IApplication, IComponent>>(Arg.Any<string>()).Returns((a, b) =>
            {
               
            });
            entityManager.GetScriptEngine().Returns(scriptEngine);
            var scriptedActionActor = new ScriptedActionActorComponent()
            {
                EntityManager = entityManager,
                ScriptFile = "script.csx"
            };

            await scriptedActionActor.ActAsync();
        
            await scriptEngine.Received(1).CreateDelegateAsync<Action<IApplication, IComponent>>("script.csx");
        }
    }
}
