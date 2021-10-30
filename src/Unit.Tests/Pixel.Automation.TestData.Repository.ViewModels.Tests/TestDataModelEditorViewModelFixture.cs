﻿using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;

namespace Pixel.Automation.TestData.Repository.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataModelEditorViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataModelEditorViewModelFixture
    {
        private IScriptEditorFactory scriptEditorFactory;
        private IInlineScriptEditor scriptEditor;     
        private IProjectFileSystem fileSystem;
      
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            scriptEditor = Substitute.For<IInlineScriptEditor>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            scriptEditorFactory.CreateInlineScriptEditor().Returns(scriptEditor);
            //scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Is<Type>(typeof(Empty)))).Do(x => { });

           
            fileSystem = Substitute.For<IProjectFileSystem>();
          
            fileSystem.WorkingDirectory.Returns(Environment.CurrentDirectory);
            fileSystem.TestDataRepository.Returns(Path.Combine(Environment.CurrentDirectory, "TestDataRepository"));
        }

        [TearDown]
        public void TearDown()
        {
            scriptEditorFactory.ClearReceivedCalls();
            scriptEditor.ClearReceivedCalls();         
            fileSystem.ClearReceivedCalls();          
        }

        [TestCase(DataSource.Code, true)]
        [TestCase(DataSource.Code, false)]
        [TestCase(DataSource.CsvFile, true)]
        [TestCase(DataSource.CsvFile, false)]
        public void ValidateThatCanGenerateCodeVariousDataSources(DataSource dataSource, bool wasCancelled)
        {         
            var typeDefinition = new Editor.Core.TypeDefinition(typeof(EmptyModel));         
            var testDataSourceViewModel = Substitute.For<IStagedScreen>();
            testDataSourceViewModel.GetProcessedResult().Returns(new TestDataSourceResult(new TestDataSource() { Id = Guid.NewGuid().ToString() }, typeDefinition.ActualType));

            var testDataModelEditorViewModel = new TestDataModelEditorViewModel(scriptEditorFactory)
            {
                PreviousScreen = testDataSourceViewModel
            };


            testDataModelEditorViewModel.ActivateAsync();
            scriptEditorFactory.Received(1).CreateInlineScriptEditor();
            scriptEditorFactory.Received(1).AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Is<Type>(typeof(EmptyModel)));
            scriptEditor.Received(1).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            scriptEditor.Received(1).Activate();           

            if(!wasCancelled)
            {
                bool couldProcessStage = testDataModelEditorViewModel.TryProcessStage(out string errorDescription);
                Assert.IsTrue(couldProcessStage);
                Assert.IsEmpty(errorDescription);
                scriptEditor.Received(1).CloseDocument(Arg.Is(true));

                var result = testDataModelEditorViewModel.GetProcessedResult();
                Assert.IsTrue((bool)result);

                testDataModelEditorViewModel.OnFinished();
                Assert.IsNull(testDataModelEditorViewModel.ScriptEditor);
                scriptEditorFactory.Received(1).RemoveProject(Arg.Any<string>());
                scriptEditor.Received(1).Dispose();
            }
            else
            {
                testDataModelEditorViewModel.OnCancelled();
                Assert.IsNull(testDataModelEditorViewModel.ScriptEditor);
                scriptEditorFactory.Received(1).RemoveProject(Arg.Any<string>());
                scriptEditor.Received(1).CloseDocument(Arg.Is(false));
                scriptEditor.Received(1).Dispose();
            }
        }
    }
}
