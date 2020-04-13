using Dawn;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Interfaces.Scripting;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    public abstract class ProjectManager : IProjectManager
    {
        protected EntityManager entityManager;
        protected ISerializer serializer;
        protected IFileSystem fileSystem;
        protected ITypeProvider typeProvider;
        protected IScriptEngineFactory scriptEngineFactory;
        protected ICodeEditorFactory codeEditorFactory;
        protected IScriptEditorFactory scriptEditorFactory;
        protected readonly ICodeGenerator codeGenerator;

   
        public ProjectManager(ISerializer serializer, IFileSystem fileSystem, ITypeProvider typeProvider, IScriptEditorFactory scriptEditorFactory, IScriptEngineFactory scriptEngineFactory, ICodeEditorFactory codeEditorFactory,ICodeGenerator codeGenerator)
        {
            this.serializer = serializer;
            this.fileSystem = fileSystem;
            this.typeProvider = typeProvider;
            this.scriptEngineFactory = scriptEngineFactory;
            this.codeEditorFactory = codeEditorFactory;
            this.scriptEditorFactory = scriptEditorFactory;
            this.codeGenerator = codeGenerator;
        }

        public IProjectManager WithEntityManager(EntityManager entityManager)
        {
            this.entityManager = entityManager;
            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            return this;
        }

        public IFileSystem GetProjectFileSystem()
        {
            return this.fileSystem;
        }

        public abstract void CreateSnapShot();        

        public abstract void Save();

        public abstract void  SaveAs();
      
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

        protected abstract string GetNewDataModelAssemblyName();

        protected object CompileAndCreateDataModel()
        {
            ICodeWorkspaceManager workspaceManager = this.codeEditorFactory.GetWorkspaceManager();

            string dataModelDirectory = this.fileSystem.DataModelDirectory;
            string[] existingDataModelFiles = Directory.GetFiles(dataModelDirectory, "*.cs");
            if(existingDataModelFiles.Any())
            {
                //This will add all the documents to workspace so that they are available during compilation
                foreach(var dataModelFile in existingDataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    if (!workspaceManager.HasDocument(documentName))
                    {
                        workspaceManager.AddDocument(documentName, File.ReadAllText(dataModelFile));
                    }                
                }
            }
            else
            {
                string dataModelInitialContent = GetDataModelFileContent();
                File.WriteAllText(Path.Combine(this.fileSystem.DataModelDirectory, "DataModel.cs"), dataModelInitialContent);
                workspaceManager.AddDocument("DataModel.cs", dataModelInitialContent);
            }


            using (var compilationResult = workspaceManager.CompileProject(GetNewDataModelAssemblyName()))
            {
                compilationResult.SaveAssemblyToDisk(fileSystem.TempDirectory);
                Assembly assembly = Assembly.LoadFrom(Path.Combine(fileSystem.TempDirectory, compilationResult.OutputAssemblyName));

                Type typeofDataModel = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals("DataModel"));
                if (typeofDataModel != null)
                {
                    return Activator.CreateInstance(typeofDataModel);
                }
            }

            throw new Exception($"Failed to create data model");

            //TODO : DefaultDataModelFileContent -- Move to common place
            string GetDataModelFileContent()
            {
                var classGenerator = this.codeGenerator.CreateClassGenerator("DataModel", "Pixel.Automation.Project.DataModels",new [] { typeof(object).Namespace });
                return classGenerator.GetGeneratedCode();
            }
        }


        #region Service Configuration

        protected virtual void ConfigureCodeEditor()
        {
            this.codeEditorFactory.Initialize(this.fileSystem.DataModelDirectory, this.fileSystem.GetAssemblyReferences());
            //ICodeEditorScreen codeEditorScreen = codeEditorFactory.CreateCodeEditor();
            //this.entityManager.RegisterDefault<ICodeEditorScreen>(codeEditorScreen);           
        }

        #endregion Service Configuration
             
        public object GetDataModel()
        {
            return this.entityManager.Arguments;
        }

        #region helper methods

        public void RestoreParentChildRelation(Entity entity, bool resetId = false)
        {
            Guard.Argument(entity).NotNull();
            foreach (var component in entity.Components)
            {
                component.Parent = entity;
                (component as Component).EntityManager = entity.EntityManager;              
                if (component is Entity)
                {
                    RestoreParentChildRelation(component as Entity, resetId);
                }

                Debug.Assert(component.Parent != null);            
            }
        }

        #endregion helper methods

    }
}
