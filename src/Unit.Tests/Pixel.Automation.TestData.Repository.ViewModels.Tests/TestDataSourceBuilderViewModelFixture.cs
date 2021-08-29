using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Editor.Core;
using System;

namespace Pixel.Automation.TestData.Repository.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataSourceBuilderViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataSourceBuilderViewModelFixture
    {
        [TestCase]
        public void ValidateDataTestDataSourceBuilderCanBeCorrectlyInitialized()
        {
            var screen = Substitute.For<IStagedScreen>();
            var testDataSourceBuilderViewModel = new TestDataSourceBuilderViewModel(new[] { screen });

            Assert.AreEqual("Data Source Editor", testDataSourceBuilderViewModel.DisplayName);         
            
        }

        [TestCase]
        public void ValidateDataTestDataSourceBuilderShouldThrowExceptionWhenNoScreens()
        {           
            Assert.Throws<ArgumentException>(() => { new TestDataSourceBuilderViewModel(new IStagedScreen[] { }); });        
        }
    }
}
