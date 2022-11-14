using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class EditorReferences
    {
        /// <summary>
        /// Assembly references that should be added to both code editor and script editor projects
        /// </summary>
        [DataMember]
        public ICollection<string> CommonEditorReferences { get; set; } = new List<string>();

        /// <summary>
        /// Assembly references that should be added to the data model project
        /// </summary>
        [DataMember]
        public ICollection<string> CodeEditorReferences { get; set; } = new List<string>();

        /// <summary>
        /// Assembly references that should be added to the script editor.
        /// Script editor references is different from script engine references as some of the script editor references should come from
        /// 'ref' assemblies at design time. These references should be automatically resolved by the script engine at runtime.
        /// </summary>
        [DataMember]
        public ICollection<string> ScriptEditorReferences { get; set; } = new List<string>();

        /// <summary>
        /// Assembly references that should be added to the script engine.       
        /// </summary>
        [DataMember]
        public ICollection<string> ScriptEngineReferences { get; set; } = new List<string>();

        /// <summary>
        /// Imports that should be added to the script editor and script engine.
        /// </summary>
        [DataMember]
        public ICollection<string> ScriptImports { get; set; } = new List<string>();

        /// <summary>
        /// CachedScriptMetadataResolver->ResolveMissingAssembly tries to resolve dependencies for primary references in a script.
        /// If we allow resolving all secondary references by default, the memory foot print increases a lot. White listed references  
        /// are allowed to be resolved by CacheScriptMetadataResolver. Primary references are not restricted.
        /// </summary>
        [DataMember]
        public ICollection<string> WhiteListedReferences { get; set; } = new List<string>();

    }
}
