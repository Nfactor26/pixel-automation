using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    public class TestExplorerViewModelFixture
    {
        [Test]
        public void ValidateThatTestExplorerViewModelCanBeInitialized()
        {
            var testExplorerViewModel = new TestExplorerViewModel();
            Assert.IsTrue(testExplorerViewModel.DisplayName.Equals("Test Explorer"));
            Assert.IsNull(testExplorerViewModel.ActiveInstance);          
        }      
    }
}
