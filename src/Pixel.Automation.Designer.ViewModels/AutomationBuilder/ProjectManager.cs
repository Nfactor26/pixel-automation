using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public abstract class ProjectManager : IProjectManager
    {
        protected readonly ILogger logger = Log.ForContext<ProjectManager>();

        private int compilationIteration = 0;

        protected IEntityManager entityManager;
        protected ISerializer serializer;
        protected IFileSystem fileSystem;
        protected ITypeProvider typeProvider;    
        protected ICodeEditorFactory codeEditorFactory;
        protected IScriptEditorFactory scriptEditorFactory;
        protected readonly ICodeGenerator codeGenerator;
        protected IArgumentTypeProvider argumentTypeProvider;
        protected IApplicationDataManager applicationDataManager;

        protected Entity RootEntity
        {
            get => this.entityManager.RootEntity;
            set => this.entityManager.RootEntity = value;
        }


        public ProjectManager(ISerializer serializer, IEntityManager entityManager, IFileSystem fileSystem, ITypeProvider typeProvider, IArgumentTypeProvider argumentTypeProvider,
            ICodeEditorFactory codeEditorFactory, IScriptEditorFactory scriptEditorFactory, ICodeGenerator codeGenerator, IApplicationDataManager applicationDataManager)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.entityManager = Guard.Argument(entityManager, nameof(entityManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider, nameof(typeProvider)).NotNull().Value; 
            this.argumentTypeProvider = Guard.Argument(argumentTypeProvider, nameof(argumentTypeProvider)).NotNull().Value;
            this.codeEditorFactory = Guard.Argument(codeEditorFactory, nameof(codeEditorFactory)).NotNull().Value;
            this.scriptEditorFactory = Guard.Argument(scriptEditorFactory, nameof(scriptEditorFactory)).NotNull().Value;
            this.codeGenerator = Guard.Argument(codeGenerator, nameof(codeGenerator)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
        }

        ///<inheritdoc/>
        public abstract Task Save();

        ///<inheritdoc/>
        public abstract Task Refresh();

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
            logger.Information($"Trying to configure code editor for project  : {this.GetProjectName()}.");
            this.codeEditorFactory.Initialize(this.fileSystem.DataModelDirectory, this.fileSystem.ReferenceManager.GetCodeEditorReferences());       
            logger.Information($"Configure code editor for project  : {this.GetProjectName()} completed.");

        }


        /// <summary>
        /// Configure ScriptEditor with EntityManager->Arguments as globalsType. For subsequent calls, if Arguments change,
        /// underlying workspace will be disposed and new workspace will be created to reflect change in globalsType.
        /// If there are any inline script editor controls, they are not impacted. They will pick up this change in globalsType.
        /// ScriptEditor should be configured for primary as well as secondary entity manager since they have different globalsType.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="globalsType"></param>
        protected virtual void ConfigureScriptEditor()
        {      
            logger.Information($"Trying to configure script editor for project  : {this.GetProjectName()}.");
            var assemblyReferences = new List<string>(this.fileSystem.ReferenceManager.GetScriptEditorReferences());
            assemblyReferences.Add(this.entityManager.Arguments.GetType().Assembly.Location);
            this.scriptEditorFactory.Initialize(this.fileSystem.WorkingDirectory, assemblyReferences.ToArray());      
            logger.Information($"Configure script editor for project  : {this.GetProjectName()} completed.");
        }

        /// <summary>
        /// Add data model assembly to arguments type provider so that it can show types defined in data model assembly 
        /// </summary>
        /// <param name="scriptArguments"></param>
        protected virtual void ConfigureArgumentTypeProvider(Assembly assembly)
        {          
            this.argumentTypeProvider.WithDataModelAssembly(assembly);
        }


        /// <summary>
        /// Create a workspace manager. Add all the documents from data model directory to this workspace , compile the workspace and
        /// create an instnace of data model from generated assembly.
        /// </summary>
        /// <param name="dataModelName"></param>
        /// <returns>Instance of dataModel</returns>
        protected object CompileAndCreateDataModel(string dataModelName)
        {
            logger.Information($"Trying to compile data model assembly for project : {this.GetProjectName()}");
            this.codeEditorFactory.AddProject(this.GetProjectName(), $"Pixel.Automation.{this.GetProjectName()}", Array.Empty<string>());

            string dataModelDirectory = this.fileSystem.DataModelDirectory;
            string[] existingDataModelFiles = Directory.GetFiles(dataModelDirectory, "*.cs");
            if (existingDataModelFiles.Any())
            {
                //This will add all the documents to workspace so that they are available during compilation
                foreach (var dataModelFile in existingDataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    this.codeEditorFactory.AddDocument(documentName, this.GetProjectName(), File.ReadAllText(dataModelFile));
                }
                try
                {
                    //Delete assemblies accumulated from old sessions
                    //This can throw exception if any assembly is already loaded.
                    Directory.Delete(fileSystem.TempDirectory, true);
                }
                catch
                {

                }
                finally
                {
                    Directory.CreateDirectory(fileSystem.TempDirectory);
                }

                using (var compilationResult = this.codeEditorFactory.CompileProject(this.GetProjectName(), GetNewDataModelAssemblyName()))
                {
                    compilationResult.SaveAssemblyToDisk(fileSystem.TempDirectory);
                    string assemblyLocation = Path.Combine(fileSystem.TempDirectory, compilationResult.OutputAssemblyName);
                    Assembly assembly = Assembly.LoadFrom(assemblyLocation);
                    logger.Information($"Data model assembly compiled and assembly loaded from {assemblyLocation}");
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
            
            //There are some scenarios where the data model assembly with same name is already loaded in domain e.g. after deploying a version
            //and trying to open new version for edit.
            compilationIteration++;
            string dataModelAssemblyName = $"{GetProjectName().Trim().Replace(' ', '_')}";
            string dataModelAssemblyNameWithIteration = $"{dataModelAssemblyName}_{compilationIteration}";
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            while(loadedAssemblies.Any(a => a.GetName().Name.Equals(dataModelAssemblyNameWithIteration)))
            {
                compilationIteration++;
                dataModelAssemblyNameWithIteration = $"{GetProjectName().Trim().Replace(' ', '_')}_{compilationIteration}";
            }
           
            return dataModelAssemblyNameWithIteration;
        }

        protected void RestoreParentChildRelation(Entity entity)
        {
            this.entityManager.RestoreParentChildRelation(entity);
        }
             
        #endregion helper methods

    }
}
