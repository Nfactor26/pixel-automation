using Dawn;
using Pixel.Scripting.Reference.Manager.Contracts;
using Pixel.Scripting.Reference.Manager.Models;

namespace Pixel.Scripting.Reference.Manager;

/// <summary>
/// ReferenceManager is used to retrieve required assembly references and imports for code editor , script editor and script runtime for a automation project 
/// or prefab project
/// </summary>
public class ReferenceManager : IReferenceManager
{
    private ReferenceCollection referenceCollection;

    /// <summary>
    /// constructor
    /// </summary>   
    /// <param name="referenceCollection"></param>
    public ReferenceManager(ReferenceCollection referenceCollection)
    {       
        this.referenceCollection = Guard.Argument(referenceCollection).NotNull();
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetCodeEditorReferences()
    {
        return this.referenceCollection.CommonEditorReferences.Union(this.referenceCollection.CodeEditorReferences);
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetScriptEditorReferences()
    {
        return this.referenceCollection.CommonEditorReferences.Union(this.referenceCollection.ScriptEditorReferences);
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetScriptEngineReferences()
    {
        return this.referenceCollection.ScriptEngineReferences;       
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetImportsForScripts()
    {
        return this.referenceCollection.ScriptImports;       
    }

    ///<inheritdoc/>  
    public IEnumerable<string> GetWhiteListedReferences()
    {
        return this.referenceCollection.WhiteListedReferences;
    }

    ///<inheritdoc/>  
    public void SetReferenceCollection(ReferenceCollection referenceCollection)
    {
        this.referenceCollection = Guard.Argument(referenceCollection).NotNull();
    }
}
