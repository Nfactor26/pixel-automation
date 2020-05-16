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
        
            var applicationWindowManager = Substitute.For<IApplicationWindowManager>();
            applicationWindowManager.When(x => x.SetForeGroundWindow(Arg.Any<ApplicationWindow>())).Do(x => { });
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(applicationWindowManager);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.Exists(Arg.Any<string>()).Returns(true);
            entityManager.GetCurrentFileSystem().Returns(fileSystem);

            var serializer = Substitute.For<ISerializer>();
            var applicationDetails = Substitute.For<IApplication, IComponent>();
            applicationDetails.Hwnd.Returns(new IntPtr(1));
            var applicationDescription = new ApplicationDescription()
            {
                ApplicationName = "MockApplication",
                ApplicationType = "Windows",
                ApplicationDetails = applicationDetails
            };
            serializer.Deserialize<ApplicationDescription>(Arg.Any<string>(), null).Returns(applicationDescription);
            entityManager.GetServiceOfType<ISerializer>().Returns(serializer);

            //set up a simple process containing sequence entity
            var applicationEntity = new ApplicationEntity() { ApplicationId = "MockId", EntityManager = entityManager };
            var sequenceEntity = new SequenceEntity() { TargetAppId = "MockId", RequiresFocus = true };

            var rootEntity = new Entity() { EntityManager = entityManager };
            entityManager.RootEntity = rootEntity;
            var appPoolEntity = new ApplicationPoolEntity();
            rootEntity.AddComponent(appPoolEntity);
            appPoolEntity.AddComponent(applicationEntity);
            rootEntity.AddComponent(sequenceEntity);


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
           //TODO : Move GetApplicationDetails to EntityManager instead of having it as extension
           //TODO : Can we find somehow if hWnd is not valid and throw in that case as well instead of just throwing for 0 as hWnd
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
