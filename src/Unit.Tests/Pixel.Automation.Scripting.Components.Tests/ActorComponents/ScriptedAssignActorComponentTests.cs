using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components.Tests.ActorComponents
{
    public class ScriptedAssignActorComponentTests
    {
        [Test]
        public async Task CanAct()
        {
            var entityManager = Substitute.For<IEntityManager>();

            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult());
            entityManager.GetScriptEngine().Returns(scriptEngine);
            var scriptedAssignActor = new ScriptedAssignActorComponent()
            {
                EntityManager = entityManager,
                ScriptFile = "script.csx"
            };

            await scriptedAssignActor.ActAsync();

            await scriptEngine.Received(1).ExecuteFileAsync("script.csx");
        }
    }
}
