﻿using Caliburn.Micro;
using Notifications.Wpf.Core;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="ControlExplorerViewModel"/>
    /// </summary>
    [TestFixture]
    public class ControlExplorerViewModelFixture
    {
        private IControlEditorFactory controlEditorFactory;
        private IControlEditor controlEditor;
        private IEventAggregator eventAggregator;
        private IWindowManager windowManager;
        private INotificationManager notificationManager;
        private IApplicationDataManager applicationDataManager;
        
        [OneTimeSetUp]
        public void SetUp()
        {
            controlEditorFactory = Substitute.For<IControlEditorFactory>();
            controlEditor = Substitute.For<IControlEditor>();
            eventAggregator = Substitute.For<IEventAggregator>();
            windowManager = Substitute.For<IWindowManager>();
            notificationManager = Substitute.For<INotificationManager>();
            applicationDataManager = Substitute.For<IApplicationDataManager>();
            controlEditorFactory.CreateControlEditor(Arg.Any<IControlIdentity>()).Returns(controlEditor);
                      
            var controlDescription = CreateControl("SaveButton");
            applicationDataManager.GetControlsForScreen(Arg.Any<ApplicationDescription>(), Arg.Any<string>()).Returns(new [] { controlDescription });
            this.applicationDataManager.AddOrUpdateControlImageAsync(Arg.Any<ControlDescription>(), Arg.Any<Stream>()).Returns(Path.GetRandomFileName());
        }

        [TearDown]
        public void TearDown()
        {
            controlEditor.ClearReceivedCalls();
            eventAggregator.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
            applicationDataManager.ClearReceivedCalls();           
        }

        /// <summary>
        /// Validate that ControlExplorerViewModel has correct initial state when intialized
        /// </summary>
        [Test]
        public void ValidateThatControlExplorerViewModelCanBeInitializedCorrectly()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator,
                controlEditorFactory, applicationDataManager);

            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(0));
            Assert.That(controlExplorer.SelectedControl is null);
        }

        /// <summary>
        /// Double clicking the name of an control on view should toggle the CanEdit state so that there is an editable text box visible to rename the control
        /// </summary>
        [Test]
        public void ValidateThatCanToggleRename()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator, 
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var controlToRename = controlExplorer.Controls.First();

            controlExplorer.SelectedControl = controlToRename;
            controlExplorer.ToggleRename(controlToRename);

            Assert.That(controlExplorer.CanEdit);
        }


        /// <summary>
        /// Validate that when active application is changed, control explorer loads the details of available controls
        /// for this application from local store
        /// </summary>
        [Test]
        public void ValidateThatControlsAreLoadedWhenApplicationIsActivated()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator, 
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();           
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));

            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(1));
            Assert.That(applicationDescription.Screens.Count, Is.EqualTo(1));
            Assert.That(applicationDescription.Screens[0].AvailableControls.Count, Is.EqualTo(1));

            controlExplorer.SetActiveApplication(null);
            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(0));
            Assert.That(applicationDescription.Screens.Count, Is.EqualTo(1));
            Assert.That(applicationDescription.Screens[0].AvailableControls.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Validate that when the user selects to configure a control , Control Editor screen should open.
        /// Clicking save on screen should save any edits while clicking cancel should discard changes.
        /// </summary>
        /// <param name="wasSaved"></param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatControlEditorCanBeOpenedToEditControlConfigurationWhenConfigureOptionIsSelected(bool wasSaved)
        {         
            windowManager.ShowDialogAsync(Arg.Any<IControlEditor>()).Returns(wasSaved);

            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator, 
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var controlToConfigure = controlExplorer.Controls.First();

            await controlExplorer.ConfigureControlAsync(controlToConfigure);           

            int expected = wasSaved ? 1 : 0;
            controlEditor.Received(1).Initialize(Arg.Any<ControlDescription>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IControlEditor>());
            await applicationDataManager.Received(expected).UpdateControlAsync(Arg.Any<ControlDescription>());
            applicationDataManager.Received(0).SaveApplicationToDisk(Arg.Any<ApplicationDescription>());

        }

        /// <summary>
        /// Validate that when the user selects to edit control the details of control are shown in property grid for editing.
        /// The EditControlAsync raises a notification that is handled by PropertyGrid.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ValidateThatPropertyGridDisplayControlDetailsToBeEditedWhenEditOptionIsSelected()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator,
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();           
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var controlToEdit = controlExplorer.Controls.First();
      
            await controlExplorer.EditControlAsync(controlToEdit);

            await eventAggregator.Received(1).PublishOnUIThreadAsync(Arg.Any<object>());
        }

        /// <summary>
        /// Validate that it is possible to clone an existing control 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ValidateThatExistingControlCanBeCloned()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator, 
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var controlToEdit = controlExplorer.Controls.First();

            await controlExplorer.CloneControl(controlToEdit);
            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(2));

            await applicationDataManager.Received(1).AddControlToScreenAsync(Arg.Any<ControlDescription>(), Arg.Any<string>());         
            await applicationDataManager.Received(1).AddOrUpdateControlImageAsync(Arg.Any<ControlDescription>(), Arg.Any<Stream>());
            applicationDataManager.Received(1).SaveApplicationToDisk(Arg.Any<ApplicationDescription>());
        }

        /// <summary>
        /// Validate that control explorer can receive the ScrapedControl collection and process them in to ControlDescription 
        /// and save them
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ValidateThatControlExplorerCanProcessScrapedControls()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator,
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));

            using (var bitMap = Bitmap.FromFile("Resources\\Image.Png"))
            {
                using(var ms = new MemoryStream())
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    ScrapedControl scrapedControl = new ScrapedControl()
                    {
                        ControlImage = ms.ToArray(),
                        ControlData = new MockControlIdentity()
                        {
                            ApplicationId = "application-id",
                            ControlImage = "image.Png",
                        }
                    };
                    await controlExplorer.HandleAsync(new[] { scrapedControl }, CancellationToken.None);
                }               
            }

            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(2));
            await applicationDataManager.Received(1).AddControlToScreenAsync(Arg.Any<ControlDescription>(), Arg.Any<string>());           
            await applicationDataManager.Received(1).AddOrUpdateControlImageAsync(Arg.Any<ControlDescription>(), Arg.Any<Stream>());
            applicationDataManager.Received(1).SaveApplicationToDisk(Arg.Any<ApplicationDescription>());
        }

        /// <summary>
        /// Validate that control details can be saved
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ValidateThatCanSaveControlDetails()
        {
            var controlExplorer = new ControlExplorerViewModel(windowManager, notificationManager, eventAggregator, 
                controlEditorFactory, applicationDataManager);
            var applicationDescription = CreateApplicationDescription();
            controlExplorer.SetActiveApplication(CreateApplicationDescriptionViewModel(applicationDescription));
            var controlToEdit = controlExplorer.Controls.First();

            await controlExplorer.UpdateControlDetails(controlToEdit);
            Assert.That(controlExplorer.Controls.Count, Is.EqualTo(1));

            await applicationDataManager.Received(1).UpdateControlAsync(Arg.Any<ControlDescription>());
            await applicationDataManager.Received(0).UpdateApplicationAsync(Arg.Any<ApplicationDescription>());
            await applicationDataManager.Received(0).AddOrUpdateControlImageAsync(Arg.Any<ControlDescription>(), Arg.Any<Stream>());
        }

        ControlDescription CreateControl(string name)
        {
            var controlIdentity = new MockControlIdentity()
            {
                ApplicationId = "application-id",
                ControlImage = "image.Png",
            };
            var controlDescription = new ControlDescription(controlIdentity)
            {
                ControlName = name,
                GroupName = "Default",
                ControlImage = "Resources\\Image.Png"
            };
            return controlDescription;
        }

        ApplicationDescriptionViewModel CreateApplicationDescriptionViewModel(ApplicationDescription applicationDescription)
        {
            var viewModel = new ApplicationDescriptionViewModel(applicationDescription);
            viewModel.AddScreen(new ApplicationScreen("Home"));
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
