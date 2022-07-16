using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{

    [TestFixture]
    public class ApplicationExplorerViewModelFixture
    {
        private IEventAggregator eventAggregator;
        private ITypeProvider typeProvider;
        private IApplicationDataManager applicationDataManager;
        private IApplicationAware childScreen;
    
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            eventAggregator = Substitute.For<IEventAggregator>();
            typeProvider = Substitute.For<ITypeProvider>();
            applicationDataManager = Substitute.For<IApplicationDataManager>();
            childScreen = Substitute.For<IApplicationAware>();

            var application = CreateApplicationDescription();
            applicationDataManager.GetAllApplications().Returns(new[] { application });
            typeProvider.GetKnownTypes().Returns(new List<Type>() { typeof(MockApplication) });

            eventAggregator.HandlerExistsFor(Arg.Is<Type>(typeof(RepositoryApplicationOpenedEventArgs))).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            eventAggregator.ClearReceivedCalls();
            applicationDataManager.ClearReceivedCalls();
            typeProvider.ClearReceivedCalls();
            childScreen.ClearReceivedCalls();
        }

        [Test]
        public void ValidateThatApplicationExplorerViewModelCanBeCorrectlyInitialized()
        {
            var applicationExplorerViewModel = new ApplicationExplorerViewModel(eventAggregator, applicationDataManager, typeProvider, new[] { childScreen });

            Assert.AreEqual(1, applicationExplorerViewModel.Applications.Count);
            Assert.AreEqual(1, applicationExplorerViewModel.KnownApplications.Count);
            Assert.AreEqual(1, applicationExplorerViewModel.ChildViews.Count);
            Assert.AreEqual(PaneLocation.Bottom, applicationExplorerViewModel.PreferredLocation);
            Assert.AreEqual(280, applicationExplorerViewModel.PreferredHeight);

            applicationDataManager.Received(1).GetAllApplications();
        }

        /// <summary>
        /// Double clicking an application on the application explorer view should show the child view e.g. control explorer
        /// </summary>
        [Test]
        public void ValidateThatCanOpenChildView()
        {
            var applicationExplorerViewModel = new ApplicationExplorerViewModel(eventAggregator, applicationDataManager, typeProvider, new[] { childScreen });
            var applicationDescription = applicationExplorerViewModel.Applications.First();
            applicationExplorerViewModel.OpenApplication(applicationDescription);

            Assert.IsTrue(applicationExplorerViewModel.IsApplicationOpen);
            childScreen.Received(1).SetActiveApplication(Arg.Any<ApplicationDescriptionViewModel>());
            eventAggregator.Received(1).HandlerExistsFor(Arg.Is<Type>(typeof(RepositoryApplicationOpenedEventArgs)));
            eventAggregator.Received(1).PublishOnUIThreadAsync(Arg.Any<object>());
        }

        /// <summary>
        /// Clicking on back button on screen should activate the application explorer view up from child view
        /// </summary>
        [Test]
        public void ValidateThatCanGoBackToParentView()
        {
            var applicationExplorerViewModel = new ApplicationExplorerViewModel(eventAggregator, applicationDataManager, typeProvider, new[] { childScreen });
            var applicationDescription = applicationExplorerViewModel.Applications.First();
            
            applicationExplorerViewModel.OpenApplication(applicationDescription);
            Assert.IsTrue(applicationExplorerViewModel.IsApplicationOpen);
          
            applicationExplorerViewModel.GoBack();
            Assert.IsFalse(applicationExplorerViewModel.IsApplicationOpen);
            childScreen.Received(1).SetActiveApplication(Arg.Is(default(ApplicationDescriptionViewModel)));
            eventAggregator.Received(2).HandlerExistsFor(Arg.Is<Type>(typeof(RepositoryApplicationOpenedEventArgs)));
            eventAggregator.Received(2).PublishOnUIThreadAsync(Arg.Any<object>());
        }

        [Test]
        public async Task ValidateThatNewApplicationCanBeAdded()
        {
            var applicationExplorerViewModel = new ApplicationExplorerViewModel(eventAggregator, applicationDataManager, typeProvider, new[] { childScreen });
            var knownApplication = applicationExplorerViewModel.KnownApplications.First();

            await applicationExplorerViewModel.AddApplication(knownApplication);
            Assert.AreEqual(2, applicationExplorerViewModel.Applications.Count);
            await applicationDataManager.Received(1).AddOrUpdateApplicationAsync(Arg.Any<ApplicationDescription>());
            await eventAggregator.Received(3).PublishOnUIThreadAsync(Arg.Any<object>());
        }

        /// <summary>
        /// Double clicking the name of an application on view toggles the CanEdit state so that there is an editable text box visible to rename the application
        /// </summary>
        [Test]
        public void ValidateThatCanToggleRename()
        {
            var applicationExplorerViewModel = new ApplicationExplorerViewModel(eventAggregator, applicationDataManager, typeProvider, new[] { childScreen });
            var applicationDescription = applicationExplorerViewModel.Applications.First();

            applicationExplorerViewModel.SelectedApplication = applicationDescription;
            applicationExplorerViewModel.ToggleRename(applicationDescription);

            Assert.IsTrue(applicationExplorerViewModel.CanEdit);
        }
       

        ApplicationDescription CreateApplicationDescription()
        {
            IApplication applicationDetails = Substitute.For<IApplication>();
            applicationDetails.ApplicationName.Returns("NotePad");
            applicationDetails.ApplicationId.Returns("application-id");
            return new ApplicationDescription(applicationDetails)
            {
                ApplicationName = "NotePad",
                ApplicationDetails = applicationDetails
            };
        }
    }
}
