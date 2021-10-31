using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Helpers
{
    public static class ScriptEditorExtensions
    {
        /// <summary>
        /// Create and show a script editor screen. Project lifecyle is manged internally.
        /// </summary>
        /// <param name="editorFactory"></param>
        /// <param name="windowManager"></param>
        /// <param name="forComponent"></param>
        /// <param name="scriptFile"></param>
        /// <param name="initialScriptContentGetter"></param>
        /// <returns></returns>
        public static async Task CreateAndShowScriptEditorScreenAsync(this IScriptEditorFactory editorFactory, IWindowManager windowManager,
            IComponent forComponent, string scriptFile, Func<IComponent, string> initialScriptContentGetter)
        {
            Guard.Argument(windowManager).NotNull();
            Guard.Argument(forComponent).NotNull();
            Guard.Argument(scriptFile).NotNull().NotEmpty();
            Guard.Argument(initialScriptContentGetter).NotNull();

            using (IScriptEditorScreen scriptEditor = editorFactory.CreateScriptEditorScreen())
            {
                AddProject(editorFactory, forComponent);
                string initialContent = initialScriptContentGetter(forComponent);
                editorFactory.AddDocument(scriptFile, forComponent.Id, initialContent);
                scriptEditor.OpenDocument(scriptFile, forComponent.Id, initialContent);

                await windowManager.ShowDialogAsync(scriptEditor);

                //Removing project is important. Script editor projects can have only one document.
                //If we keep this project for reuse later and component has multiple scriptable fields, there will be issues.
                //we don't want to just remove document as it will leave any dependencies added to project which we don't want for other script.
                //TODO : Would it be better to generate a random identifier and combine it with component id to use as a project id ?
                editorFactory.RemoveProject(forComponent.Id);
            }            
        }

        /// <summary>
        /// Create a new cached inline script editor. Project lifecycle is internally maintained.
        /// </summary>
        /// <param name="editorFactory"></param>
        /// <param name="forComponent"></param>
        /// <param name="scriptFile"></param>
        /// <param name="initialScriptContentGetter"></param>
        /// <returns></returns>
        public static IInlineScriptEditor CreateAndInitializeInilineScriptEditor(this IScriptEditorFactory editorFactory,
            IComponent forComponent, string scriptFile, Func<IComponent, string> initialScriptContentGetter)
        {          
            Guard.Argument(forComponent).NotNull();
            Guard.Argument(scriptFile).NotNull().NotEmpty();
            Guard.Argument(initialScriptContentGetter).NotNull();
            var cacheKey = $"{forComponent.Id}-{Path.GetFileNameWithoutExtension(scriptFile)}";
            var scriptEditor = editorFactory.CreateInlineScriptEditor(cacheKey);
            AddProject(editorFactory, forComponent);
            string initialContent = initialScriptContentGetter(forComponent);
            editorFactory.AddDocument(scriptFile, forComponent.Id, initialContent);
            scriptEditor.OpenDocument(scriptFile, forComponent.Id, initialContent);
            return scriptEditor;
        }

        private static void AddProject(IScriptEditorFactory editorFactory, IComponent forComponent)
        {
            if (forComponent.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
            {
                //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                editorFactory.AddProject(forComponent.Id, new string[] { testCaseEntity.Tag }, forComponent.EntityManager.Arguments.GetType());
            }
            else if (forComponent.TryGetAnsecstorOfType<TestFixtureEntity>(out TestFixtureEntity testFixtureEntity))
            {
                //Test fixture have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                editorFactory.AddProject(forComponent.Id, new string[] { testFixtureEntity.Tag }, forComponent.EntityManager.Arguments.GetType());
            }
            else
            {
                editorFactory.AddProject(forComponent.Id, Array.Empty<string>(), forComponent.EntityManager.Arguments.GetType());
            }
        }
    }
}
