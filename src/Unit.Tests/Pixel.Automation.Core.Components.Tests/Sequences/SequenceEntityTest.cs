using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Tests
{
    public class SequenceEntityTest
    {
        /// <summary>
        /// Validate that Sequence entity can be initialized
        /// </summary>
        [Test]
        public void ValidateThatSequenceEntityCanBeInitializer()
        {
            var sequenceEntity = new SequenceEntity();           
            Assert.That(sequenceEntity.RequiresFocus, Is.EqualTo(false));
            Assert.That(sequenceEntity.TargetAppId, Is.EqualTo(string.Empty));
        }


        /// <summary>
        /// Given a standard test process setup and a sequence entity is configured to acquire focus , validate that
        /// sequence entity sets owner application window as the foreground window during call to BeforeProcess()
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GivenSequenceEntityIsConfiguredToRequreFocusValidateThatApplicationWindowIsSetToForegroundWindow()
        {
            var entityManager = Substitute.For<IEntityManager>();
            
            var applicationDetails = Substitute.For<IApplication, IComponent>();
            applicationDetails.Hwnd.Returns(new IntPtr(1));
            entityManager.GetOwnerApplication(Arg.Any<IComponent>()).Returns(applicationDetails);

            var applicationWindowManager = Substitute.For<IApplicationWindowManager>();
            applicationWindowManager.When(x => x.SetForeGroundWindow(Arg.Any<ApplicationWindow>())).Do(x => { });
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(applicationWindowManager);
        
            var sequenceEntity = new SequenceEntity() { EntityManager = entityManager, TargetAppId = "MockId", RequiresFocus = true };

            //Act
            await sequenceEntity.BeforeProcessAsync();
            await sequenceEntity.OnCompletionAsync();

            //Assert
            applicationWindowManager.Received(1).SetForeGroundWindow(Arg.Any<ApplicationWindow>());
        }


        /// <summary>
        /// Given a standard test process setup and a sequence entity is configured to acquire focus , validate that
        /// sequence entity sets owner application window as the foreground window during call to BeforeProcess()
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GivenSequenceEntityIsConfiguredToRequreFocusValidateThatExceptionIsThrownIfOWnerApplicationWindowHandleIsZero()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var applicationDetails = Substitute.For<IApplication, IComponent>();
            applicationDetails.Hwnd.Returns(new IntPtr(0));
            entityManager.GetOwnerApplication(Arg.Any<IComponent>()).Returns(applicationDetails);   
      

            var sequenceEntity = new SequenceEntity() { EntityManager = entityManager,  TargetAppId = "MockId", RequiresFocus = true };

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await sequenceEntity.BeforeProcessAsync(); });
            await sequenceEntity.OnFaultAsync(sequenceEntity);
        }

        /// <summary>
        /// Given a standard test process setup and a sequence entity is not configured to acquire focus , validate that
        /// sequence entity doesn't make any attempt to set owner application window as the foreground window
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GivenSequenceEntityIsConfiguredNotToRequreFocusValidateThatNoAttemptIsMadeToSetOwnerApplicationAsForegroundWindow()
        {
            var entityManager = Substitute.For<IEntityManager>();       

            var applicationWindowManager = Substitute.For<IApplicationWindowManager>();
            applicationWindowManager.When(x => x.SetForeGroundWindow(Arg.Any<ApplicationWindow>())).Do(x => { });
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(applicationWindowManager);
      
            var sequenceEntity = new SequenceEntity() { RequiresFocus = false };
            await sequenceEntity.BeforeProcessAsync();
            await sequenceEntity.OnCompletionAsync();

            applicationWindowManager.Received(0).SetForeGroundWindow(Arg.Any<ApplicationWindow>());
        }
    }
}
