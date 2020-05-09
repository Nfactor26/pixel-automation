using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Scripting.Components.Tests.ActorComponents
{
    public class ScriptedAssignActorComponentTests
    {
        //[Test]
        //public void CanAct()
        //{          
        //    IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
        //    scriptEngine.CreateDelegateAsync<Action<IApplication, IComponent>>(Arg.Any<string>()).Returns((a,b) =>
        //    {
        //       //Do nothing
        //    });

        //    IServiceResolver serviceResolver = Substitute.For<IServiceResolver>();
        //    serviceResolver.Get<IScriptEngine>(Arg.Any<string>()).Returns(scriptEngine);

        //    EntityManager entityManager = new EntityManager(serviceResolver);
        //    var scriptedAssignActor = new ScriptedAssignActorComponent()
        //    {
        //        EntityManager = entityManager,
        //        ScriptFile = "script.csx"
        //    };

        //    scriptedAssignActor.Act();

        //    serviceResolver.Received(1).Get<IScriptEngine>(null);
        //    scriptEngine.Received(1).CreateDelegateAsync<Action<IApplication, IComponent>>("script.csx");


        //}
    }
}
