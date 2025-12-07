using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    public class EditorReferences : ICloneable
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


        public IEnumerable<string> GetCodeEditorReferences()
        {
            return this.CommonEditorReferences.Union(this.CodeEditorReferences);
        }

        public IEnumerable<string> GetScriptEditorReferences()
        {
            return this.CommonEditorReferences.Union(this.ScriptEditorReferences);
        }

        public object Clone()
        {
            return new EditorReferences
            {
                CommonEditorReferences = [.. this.CommonEditorReferences],
                CodeEditorReferences = [.. this.CodeEditorReferences],
                ScriptEditorReferences = [.. this.ScriptEditorReferences],
                ScriptEngineReferences = [.. this.ScriptEngineReferences],
                ScriptImports = [.. this.ScriptImports],
                WhiteListedReferences = [.. this.WhiteListedReferences]
            };
        }

        public override bool Equals(object obj)
        {
            if(obj is EditorReferences other)
            {
               if(other.CommonEditorReferences.SequenceEqual(this.CommonEditorReferences) &&
                  other.CodeEditorReferences.SequenceEqual(this.CodeEditorReferences) &&
                  other.ScriptEditorReferences.SequenceEqual(this.ScriptEditorReferences) &&
                  other.ScriptEngineReferences.SequenceEqual(this.ScriptEngineReferences) &&
                  other.ScriptImports.SequenceEqual(this.ScriptImports) &&
                  other.WhiteListedReferences.SequenceEqual(this.WhiteListedReferences))
               {
                   return true;
               }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.CommonEditorReferences,
                this.CodeEditorReferences,
                this.ScriptEditorReferences,
                this.ScriptEngineReferences,
                this.ScriptImports,
                this.WhiteListedReferences
            );
        }
    }
}
