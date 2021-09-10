using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Get the required assembly references for code editor , script editor and script runtime.  
    /// </summary>
    public class AssemblyReferenceManager
    {
        private readonly string dataModelDirectory;
        private readonly string scriptsDirectory;

        List<string> defaultEditorReferences = new List<string>();
        List<string> codeEditorReferences = new List<string>();
        List<string> scriptCommonReferences = new List<string>();

        string CodeReferencesFile => Path.Combine(this.dataModelDirectory, "Refs.Code.Include");
        string ScriptReferencesFile => Path.Combine(this.scriptsDirectory, "Refs.Script.Include");

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dataModelDirectory"></param>
        /// <param name="scriptsDirectory"></param>
        public AssemblyReferenceManager(ApplicationSettings applicationSettings, string dataModelDirectory, string scriptsDirectory)
        {
            this.dataModelDirectory = dataModelDirectory;
            this.scriptsDirectory = scriptsDirectory;

            this.defaultEditorReferences.AddRange(applicationSettings.DefaultEditorReferences ?? Enumerable.Empty<string>());
            this.codeEditorReferences.AddRange(applicationSettings.DefaultCodeReferences ?? Enumerable.Empty<string>());
            this.scriptCommonReferences.AddRange(applicationSettings.DefaultScriptReferences ?? Enumerable.Empty<string>());

            AddCustomReferences(this.codeEditorReferences, this.CodeReferencesFile);
            AddCustomReferences(this.scriptCommonReferences, this.ScriptReferencesFile);
        }

        /// <summary>
        /// Get the references for the code editor
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCodeEditorReferences()
        {
            foreach (var item in this.defaultEditorReferences)
            {
                yield return item;
            }
            foreach (var item in this.codeEditorReferences)
            {
                yield return item;
            }
            yield break;
        }

        /// <summary>
        /// Get the references  for the script editor
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetScriptEditorReferences()
        {
            foreach (var item in this.defaultEditorReferences)
            {
                yield return item;
            }
            foreach (var item in this.scriptCommonReferences)
            {
                yield return item;
            }
            yield break;
        }


        /// <summary>
        /// Get the references for script runtime
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetScriptRunTimeReferences()
        {
            return this.scriptCommonReferences;
        }


        /// <summary>
        /// If a custom file exists, load references from custom file and add to defaults.       
        /// </summary>
        /// <param name="defaultRefFile"></param>
        /// <param name="customRefFile"></param>
        /// <returns></returns>
        void AddCustomReferences(List<string> references, string customRefFile)
        {
            if (File.Exists(customRefFile))
            {
                var customReferences = File.ReadAllLines(customRefFile);
                foreach (var reference in customReferences)
                {
                    if (!references.Contains(reference))
                    {
                        references.Add(reference);
                    }
                }
            }
        }
    }
}
