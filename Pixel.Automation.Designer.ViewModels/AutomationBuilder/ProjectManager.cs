using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public abstract class ProjectManager : IProjectManager
    {
        private int compilationIteration = 0;

        protected EntityManager entityManager;
        protected ISerializer serializer;
        protected IFileSystem fileSystem;
        protected ITypeProvider typeProvider;
        protected IScriptEngineFactory scriptEngineFactory;
        protected ICodeEditorFactory codeEditorFactory;   
        protected readonly ICodeGenerator codeGenerator;

        protected Entity RootEntity
        {
            get => this.entityManager.RootEntity;
            set => this.entityManager.RootEntity = value;
        }


        public ProjectManager(ISerializer serializer, IFileSystem fileSystem, ITypeProvider typeProvider, IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory,ICodeGenerator codeGenerator)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider, nameof(typeProvider)).NotNull().Value;
            this.scriptEngineFactory = Guard.Argument(scriptEngineFactory, nameof(scriptEngineFactory)).NotNull().Value;
            this.codeEditorFactory = Guard.Argument(codeEditorFactory, nameof(codeEditorFactory)).NotNull().Value;     
            this.codeGenerator = Guard.Argument(codeGenerator, nameof(codeGenerator)).NotNull().Value;
        }

        ///<inheritdoc/>
        public abstract Task Save();

        ///<inheritdoc/>
        public IProjectManager WithEntityManager(EntityManager entityManager)
        {
            this.entityManager = entityManager;
            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            return this;
        }

        ///<inheritdoc/>
        public IFileSystem GetProjectFileSystem()
        {
            return this.fileSystem;
        }
               
        ///<inheritdoc/>
        public T Load<T>(string fileName) where T : new()
        {         
            string fileContents = File.ReadAllText(fileName);

            string dataModelAssemblyName = this.entityManager.Arguments.GetType().Assembly.GetName().Name;
            string assemblyNameWithoutIteration = dataModelAssemblyName.Substring(0, dataModelAssemblyName.LastIndexOf('_'));
          
            Regex regex = new Regex($"({assemblyNameWithoutIteration})(_\\d+)");
            fileContents = regex.Replace(fileContents, (m) =>
            {
                return m.Value.Replace($"{m.Groups[0].Value}", $"{dataModelAssemblyName}");
            });         

            File.WriteAllText(fileName, fileContents);        
            T entity = serializer.Deserialize<T>(fileName, typeProvider.GetAllTypes());
            return entity;
        }

        #region helper methods

        protected virtual void ConfigureCodeEditor()
        {
            this.codeEditorFactory.Initialize(this.fileSystem.DataModelDirectory, this.fileSystem.GetAssemblyReferences());               
        }

        /// <summary>
        /// Create a workspace manager. Add all the documents from data model directory to this workspace , compile the workspace and
        /// create an instnace of data model from generated assembly.
        /// </summary>
        /// <param name="dataModelName"></param>
        /// <returns>Instance of dataModel</returns>
        protected object CompileAndCreateDataModel(string dataModelName)
        {
            ICodeWorkspaceManager workspaceManager = this.codeEditorFactory.GetWorkspaceManager();

            string dataModelDirectory = this.fileSystem.DataModelDirectory;
            string[] existingDataModelFiles = Directory.GetFiles(dataModelDirectory, "*.cs");
            if (existingDataModelFiles.Any())
            {
                //This will add all the documents to workspace so that they are available during compilation
                foreach (var dataModelFile in existingDataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    if (!workspaceManager.HasDocument(documentName))
                    {
                        workspaceManager.AddDocument(documentName, File.ReadAllText(dataModelFile));
                    }
                }

                using (var compilationResult = workspaceManager.CompileProject(GetNewDataModelAssemblyName()))
                {
                    compilationResult.SaveAssemblyToDisk(fileSystem.TempDirectory);
                    Assembly assembly = Assembly.LoadFrom(Path.Combine(fileSystem.TempDirectory, compilationResult.OutputAssemblyName));

                    Type typeofDataModel = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(dataModelName));
                    if (typeofDataModel != null)
                    {
                        return Activator.CreateInstance(typeofDataModel);
                    }
                }

                throw new Exception($"Failed to create data model");
            }

            throw new Exception($"DataModel file : {dataModelName}.cs could not be located in {this.fileSystem.DataModelDirectory}");

        }
        
        /// <summary>
        /// Get the name of the project
        /// </summary>
        /// <returns></returns>
        protected abstract string GetProjectName();

        /// <summary>
        /// Check project temp folder for any existing assemblies and extract the iteration number that was used for naming that assembly.
        /// Generate the next assembly name by incrementing this iteration number e.g.  ProjectName_n+1 if n was the iteration used for last assembly.
        /// </summary>
        /// <returns></returns>
        protected string GetNewDataModelAssemblyName()
        {
            var assemblyFiles = Directory.GetFiles(this.fileSystem.TempDirectory, "*.dll").Select(f => new FileInfo(Path.Combine(this.fileSystem.TempDirectory, f)));
            if (assemblyFiles.Any())
            {
                var mostRecentAssembly = assemblyFiles.OrderBy(a => a.CreationTime).Last();
                string assemblyName = Path.GetFileNameWithoutExtension(mostRecentAssembly.Name);
                if (int.TryParse(assemblyName.Split('_', StringSplitOptions.RemoveEmptyEntries).Last(), out int lastIteration))
                {
                    compilationIteration = lastIteration;
                }
            }

            compilationIteration++;
            string dataModelAssemblyName = $"{GetProjectName().Trim().Replace(' ', '_')}_{compilationIteration}";
            return dataModelAssemblyName;
        }

        protected void RestoreParentChildRelation(Entity entity, bool resetId = false)
        {
            this.entityManager.RestoreParentChildRelation(entity, resetId);
        }
             
        #endregion helper methods

    }
}
