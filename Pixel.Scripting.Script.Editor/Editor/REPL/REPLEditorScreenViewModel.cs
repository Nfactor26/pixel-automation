using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.REPL
{
    public class REPLEditorScreenViewModel : Screen, IREPLScriptEditor, IDisposable
    {

        private readonly IEditorService editorService;
        private readonly IScriptEngine scriptEngine;
        private readonly string documentName;
        object previousState = default;
        object globals = default;

        private string result;
        public string Result
        {
            get => result;
            set
            {
                result = value;
                NotifyOfPropertyChange(() => Result);
            }
        }

        public CodeTextEditor Editor { get; }

        public REPLEditorScreenViewModel(IEditorService editorService, IScriptEngine scriptEngine)
        {
            this.editorService = editorService;
            this.scriptEngine = scriptEngine;

            this.DisplayName = "Script Editor";
            this.Editor = new CodeTextEditor(this.editorService)
            {
                ShowLineNumbers = true,
                Margin = new Thickness(5),
                FontSize = 23,
                FontFamily = new FontFamily("Consolas")
            };

            documentName = Path.GetRandomFileName();
            this.editorService.CreateFileIfNotExists(documentName, string.Empty);
            this.Editor.Text = this.editorService.GetFileContentFromDisk(documentName);

            if (!this.editorService.HasDocument(documentName))
                this.editorService.AddDocument(documentName, string.Empty);
            this.editorService.TryOpenDocument(documentName);
            this.Editor.OpenDocument(documentName);
        }      

        public void SetGlobals(object globals)
        {
            this.globals = globals;
            this.scriptEngine.SetGlobals(globals);
        }

        public async void ExecuteScript()
        {
            string scriptText = this.Editor.Text;
            //TextWriter standardOutput = Console.Out;
            try
            {
                //StringBuilder sb = new StringBuilder();
                //using (StringWriter stringWriter = new StringWriter(sb))
                //{
                    //Console.SetOut(stringWriter);

                    ScriptResult scriptResult = await this.scriptEngine.ExecuteScriptAsync(scriptText);
                    previousState = scriptResult.CurrentState;

                    //this.Editor.Text = string.Empty;

                    Result = scriptResult.ReturnValue?.ToString() ?? string.Empty;
                //}

            }
            catch (Exception ex)
            {
                Result = ex.Message;
            }
            finally
            {
                //Console.SetOut(standardOutput);
            }
        }

        public void Reset()
        {
            this.Editor.Text = string.Empty;
            previousState = null;
            Result = string.Empty;
        }
        
        public void Dispose()
        {
            (this.editorService as IDisposable)?.Dispose();
            this.editorService.TryCloseDocument(this.documentName);
            (this.Editor as IDisposable)?.Dispose();
        }       
    }
}
