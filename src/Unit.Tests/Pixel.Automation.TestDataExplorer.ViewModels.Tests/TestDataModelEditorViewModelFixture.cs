﻿using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.TestDataExplorer.ViewModels.Tests
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
            scriptEditorFactory.CreateInlineScriptEditor(Arg.Any<EditorOptions>()).Returns(scriptEditor);
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
        public async Task ValidateThatCanGenerateCodeVariousDataSources(DataSource dataSource, bool wasCancelled)
        {         
            var typeDefinition = new TypeDefinition(typeof(EmptyModel));         
            var testDataSourceViewModel = Substitute.For<IStagedScreen>();
            testDataSourceViewModel.GetProcessedResult().Returns(new TestDataSourceResult(new TestDataSource() { DataSourceId = Guid.NewGuid().ToString() }, typeDefinition.ActualType));

            var testDataModelEditorViewModel = new TestDataModelEditorViewModel(scriptEditorFactory)
            {
                PreviousScreen = testDataSourceViewModel
            };


            await testDataModelEditorViewModel.ActivateAsync();
            scriptEditorFactory.Received(1).CreateInlineScriptEditor(Arg.Any<EditorOptions>());
            scriptEditorFactory.Received(1).AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Is<Type>(typeof(EmptyModel)));
            scriptEditor.Received(1).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            scriptEditor.Received(1).Activate();           

            if(!wasCancelled)
            {
                var couldProcessStage  = await testDataModelEditorViewModel.TryProcessStage();
                Assert.That(couldProcessStage);          
                scriptEditor.Received(1).CloseDocument(Arg.Is(true));

                var result = testDataModelEditorViewModel.GetProcessedResult();
                Assert.That((bool)result);

                await testDataModelEditorViewModel.OnFinished();
                Assert.That(testDataModelEditorViewModel.ScriptEditor is null);              
                scriptEditor.Received(1).Dispose();
            }
            else
            {
                await testDataModelEditorViewModel.OnCancelled();
                Assert.That(testDataModelEditorViewModel.ScriptEditor is null);               
                scriptEditor.Received(1).CloseDocument(Arg.Is(false));
                scriptEditor.Received(1).Dispose();
            }
        }
    }
}
