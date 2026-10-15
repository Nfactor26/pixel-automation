using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Linq;

namespace Pixel.Automation.AppExplorer.ViewModels.Tests;

/// <summary>
/// Test Fixture for <see cref="ApplicationDescriptionViewModel"/>
/// </summary>
[TestFixture]
public class ApplicationDescriptionViewModelFixture
{      
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

    [Test]
    public void ValidateThatApplicationDescriptionViewModelCanBeCorrectlyInitialized()
    {
        var applicationDescription = CreateApplicationDescription();
        var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
        {
            ApplicationType = "WinApplication"
        };

        Assert.That(applicationDescriptionViewModel.Model is not null);
        Assert.That(applicationDescriptionViewModel.ApplicationDetails is not null);
        Assert.That(applicationDescriptionViewModel.ApplicationId, Is.EqualTo(applicationDescription.ApplicationId));
        Assert.That(applicationDescriptionViewModel.ApplicationName, Is.EqualTo(applicationDescription.ApplicationName));
        Assert.That(applicationDescriptionViewModel.ApplicationType, Is.EqualTo("WinApplication"));
        Assert.That(applicationDescriptionViewModel.Screens.Count, Is.EqualTo(0));          
    }

    [Test]
    public void ValidateThatCanAddAndRemoveControl()
    {
        var applicationDescription = CreateApplicationDescription();
        var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(applicationDescription)
        {
            ApplicationType = "WinApplication"
        };
        applicationDescriptionViewModel.AddScreen(new ApplicationScreen("Home"));

        var control = new Control.ControlDescriptionViewModel(new ControlDescription());
        applicationDescriptionViewModel.AddControl(control, "Home");
                
        Assert.That(applicationDescriptionViewModel["Home"].AvailableControls.Count, Is.EqualTo(1));     
        Assert.That(applicationDescriptionViewModel.ScreenCollection.Screens.Count(), Is.EqualTo(1));

        applicationDescriptionViewModel.DeleteControl(control, "Home");

        Assert.That(applicationDescriptionViewModel["Home"].AvailableControls.Count, Is.EqualTo(0));       

        applicationDescriptionViewModel.DeleteScreen("Home");
        Assert.That(applicationDescriptionViewModel.ScreenCollection.Screens.Count(), Is.EqualTo(0));

    }

}