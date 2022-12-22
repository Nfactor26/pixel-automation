using Caliburn.Micro;
using Notifications.Wpf.Core;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="PrefabExplorerViewModel"/>
    /// </summary>
    [TestFixture]
    public class PrefabExplorerViewModelFixture
    {
        private IWindowManager windowManager;
        private INotificationManager notificationManager;
        private IEventAggregator eventAggregator;
        private IVersionManagerFactory versionManagerFactory;
        private IApplicationDataManager applicationDataManager;
        private IPrefabDataManager prefabDataManager;
        private IPrefabBuilderFactory prefabBuilderFactory;

        [OneTimeSetUp]
        public void SetUp()
        {
            windowManager = Substitute.For<IWindowManager>();
            notificationManager = Substitute.For<INotificationManager>();
            eventAggregator = Substitute.For<IEventAggregator>();
            versionManagerFactory = Substitute.For<IVersionManagerFactory>();
            prefabBuilderFactory = Substitute.For<IPrefabBuilderFactory>();          
            applicationDataManager = Substitute.For<IApplicationDataManager>();
            prefabDataManager = Substitute.For<IPrefabDataManager>();

            var prefab = CreatePrefabProject("prefab-one");
            prefabDataManager.GetPrefabsForScreen(Arg.Any<ApplicationDescription>(), Arg.Any<string>()).Returns(new[] { prefab });
        }


        /// <summary>
        /// Validate that the prefab explorer has correct initial state when initialized
        /// </summary>
        [Test]
        public void ValidateThatPrefabExplorerViewModelCanBeCorrectlyInitialized()
        {
            var prefabExplorer = new PrefabExplorerViewModel(eventAggregator, windowManager, notificationManager,
                versionManagerFactory, applicationDataManager, prefabDataManager, prefabBuilderFactory);

            Assert.AreEqual("Prefab Explorer", prefabExplorer.DisplayName);
            Assert.AreEqual(0, prefabExplorer.Prefabs.Count);
            Assert.IsNotNull(prefabExplorer.PrefabDragHandler);
            Assert.IsNull(prefabExplorer.SelectedPrefab);
        }

        /// <summary>
        /// Validate that when active application is changed, control explorer loads the details of available controls
        /// for this application from local store
        /// </summary>
        [Test]
        public void ValidateThatPrefabsAreLoadedWhenApplicationIsActivated()
        {
            var prefabExplorer = new PrefabExplorerViewModel(eventAggregator, windowManager, notificationManager,
                versionManagerFactory, applicationDataManager, prefabDataManager, prefabBuilderFactory);
            var applicationDescription = CreateApplicationDescription();
            prefabExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));

            Assert.AreEqual(1, prefabExplorer.Prefabs.Count);
            Assert.AreEqual(1, applicationDescription.AvailablePrefabs.Count);
            Assert.AreEqual(1, applicationDescription.AvailablePrefabs["Home"].Count);

            prefabExplorer.SetActiveApplication(null);
            Assert.AreEqual(0, prefabExplorer.Prefabs.Count);
            Assert.AreEqual(1, applicationDescription.AvailablePrefabs.Count);
            Assert.AreEqual(1, applicationDescription.AvailablePrefabs["Home"].Count);

        }


        /// <summary>
        /// Validate that Prefab version manager screen can be opened for a Prefab project to manage
        /// prefab versions.
        /// </summary>
        [Test]
        public async Task ValidateThaCanManagePrefabVersions()
        {
            var versionManager = Substitute.For<IVersionManager>();
            versionManagerFactory.CreatePrefabVersionManager(Arg.Any<PrefabProject>()).Returns(versionManager);
            
            var prefabExplorer = new PrefabExplorerViewModel(eventAggregator, windowManager, notificationManager,
                versionManagerFactory, applicationDataManager, prefabDataManager, prefabBuilderFactory);
            var applicationDescription = CreateApplicationDescription();
            prefabExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var prefabToManage = prefabExplorer.Prefabs.First();
            
            await prefabExplorer.ManagePrefab(prefabToManage);

            versionManagerFactory.Received(1).CreatePrefabVersionManager(Arg.Any<PrefabProject>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IVersionManager>());
        }


        [TearDown]
        public void TearDown()
        {
            windowManager.ClearReceivedCalls();
            eventAggregator.ClearReceivedCalls();
            versionManagerFactory.ClearReceivedCalls();
            prefabBuilderFactory.ClearReceivedCalls();
            applicationDataManager.ClearReceivedCalls();
        }

        PrefabProject CreatePrefabProject(string prefabName)
        {
            return new PrefabProject()
            {
                ApplicationId = "application-id",
                PrefabId = "prefab-id",            
                PrefabName = prefabName
            };
        }

        ApplicationDescriptionViewModel CreateApplicationDescriptionViewModel(ApplicationDescription applicationDescription)
        {
            var viewModel = new ApplicationDescriptionViewModel(applicationDescription);
            viewModel.AddScreen("Home");
            viewModel.ScreenCollection.SetActiveScreen("Home");
            return viewModel;
        }

        ApplicationDescription CreateApplicationDescription()
        {
            IApplication applicationDetails = Substitute.For<IApplication>();
            applicationDetails.ApplicationName.Returns("NotePad");
            applicationDetails.ApplicationId.Returns("application-id");
            var applicationDescription = new ApplicationDescription(applicationDetails)
            {
                ApplicationName = "NotePad",
                ApplicationDetails = applicationDetails
            };           
            return applicationDescription;
        }
    }
}
