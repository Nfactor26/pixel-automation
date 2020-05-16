using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Threading;
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
            Assert.AreEqual(3, sequenceEntity.AcquireFocusTimeout);
            Assert.AreEqual(false, sequenceEntity.RequiresFocus);
            Assert.AreEqual(string.Empty, sequenceEntity.TargetAppId);
        }


        /// <summary>
        /// Given a standard test process setup and a sequence entity is configured to acquire focus , validate that
        /// sequence entity sets owner application window as the foreground window during call to BeforeProcess()
        /// </summary>
        /// <returns></returns>
        [Test]
        public void GivenSequenceEntityIsConfiguredToRequreFocusValidateThatApplicationWindowIsSetToForegroundWindow()
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
            using(sequenceEntity)
            {
                sequenceEntity.BeforeProcess();
                sequenceEntity.OnCompletion();
            }
        
            //Assert
            applicationWindowManager.Received(1).SetForeGroundWindow(Arg.Any<ApplicationWindow>());
        }


        /// <summary>
        /// Given a standard test process setup and a sequence entity is configured to acquire focus , validate that
        /// sequence entity sets owner application window as the foreground window during call to BeforeProcess()
        /// </summary>
        /// <returns></returns>
        [Test]
        public void GivenSequenceEntityIsConfiguredToRequreFocusValidateThatExceptionIsThrownIfOWnerApplicationWindowHandleIsZero()
        {

            var entityManager = Substitute.For<IEntityManager>();

            var applicationDetails = Substitute.For<IApplication, IComponent>();
            applicationDetails.Hwnd.Returns(new IntPtr(0));
            entityManager.GetOwnerApplication(Arg.Any<IComponent>()).Returns(applicationDetails);   
      

            var sequenceEntity = new SequenceEntity() { EntityManager = entityManager,  TargetAppId = "MockId", RequiresFocus = true };

            //Act
            using (sequenceEntity)
            {
                Assert.Throws<InvalidOperationException>(() => { sequenceEntity.BeforeProcess(); });
                sequenceEntity.OnFault(sequenceEntity);
            }         
        }


        /// <summary>
        /// Validate that exception is thrown if there is a timeout while waiting to acquire lock on mutex
        /// </summary>
        /// <returns></returns>
        [Test]
        public void ValidateThatExceptionIsThrownIfMutexLockCanNotBeAcquiredWithinConfiguredTimeout()
        {
            var sequenceEntity = new SequenceEntity() { RequiresFocus = true, TargetAppId = "MockId", AcquireFocusTimeout = 1 };
            using(var mutex = new Mutex(false, "Local\\Pixel.AppFocus"))
            {
                mutex.WaitOne(TimeSpan.FromSeconds(2));
                //Start on a new task since Mutex can be acquired multiple time by same thread 
                var task = new Task(() =>
                {
                    Assert.Throws<TimeoutException>(() =>
                    {
                        sequenceEntity.BeforeProcess();
                    });
                });
                task.Start();
                task.Wait();             
                sequenceEntity.Dispose();
                mutex.ReleaseMutex();
            }
        }


        /// <summary>
        /// Given a standard test process setup and a sequence entity is not configured to acquire focus , validate that
        /// sequence entity doesn't make any attempt to set owner application window as the foreground window
        /// </summary>
        /// <returns></returns>
        [Test]
        public void GivenSequenceEntityIsConfiguredNotToRequreFocusValidateThatNoAttemptIsMadeToSetOwnerApplicationAsForegroundWindow()
        {
            var entityManager = Substitute.For<IEntityManager>();       

            var applicationWindowManager = Substitute.For<IApplicationWindowManager>();
            applicationWindowManager.When(x => x.SetForeGroundWindow(Arg.Any<ApplicationWindow>())).Do(x => { });
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(applicationWindowManager);
      
            var sequenceEntity = new SequenceEntity() { RequiresFocus = false };         
            using (sequenceEntity)
            {
                sequenceEntity.BeforeProcess();
                sequenceEntity.OnCompletion();
            }
        
            applicationWindowManager.Received(0).SetForeGroundWindow(Arg.Any<ApplicationWindow>());
        }
    }
}
