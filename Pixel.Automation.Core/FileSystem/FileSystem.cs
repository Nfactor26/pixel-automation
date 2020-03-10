using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.Core
{
    public abstract class FileSystem : IFileSystem
    {

        protected readonly ISerializer serializer;

        protected string ReferencesFile => Path.Combine(this.DataModelDirectory, "AssemblyReferences.dat");

        public string WorkingDirectory { get; protected set; }

        public string ScriptsDirectory { get; protected set; }

        public string TempDirectory { get; protected set; }

        public string DataModelDirectory { get; protected set; }

        public Version ActiveVersion { get; protected set; }

        public FileSystem(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        protected void Initialize()
        {
            if(String.IsNullOrEmpty(this.WorkingDirectory))
            {
                throw new InvalidOperationException("WorkingDirectory is not initialized");
            }

            this.ScriptsDirectory = Path.Combine(this.WorkingDirectory, "Scripts");
            this.TempDirectory = Path.Combine(this.WorkingDirectory, "Temp");
            this.DataModelDirectory = Path.Combine(this.WorkingDirectory, "DataModel");           

            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }

            if (!Directory.Exists(ScriptsDirectory))
            {
                Directory.CreateDirectory(ScriptsDirectory);
            }

            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }

            if (!Directory.Exists(DataModelDirectory))
            {
                Directory.CreateDirectory(DataModelDirectory);
            }

        }

        public abstract void SwitchToVersion(Version version);
      

        private AssemblyReferences editorReferences;

        public string[] GetAssemblyReferences()
        {
            if (File.Exists(ReferencesFile))
            {
                this.editorReferences = serializer.Deserialize<AssemblyReferences>(ReferencesFile, null);
            }
            else
            {
                this.editorReferences = new AssemblyReferences();
                serializer.Serialize<AssemblyReferences>(ReferencesFile, this.editorReferences);

            }
            return this.editorReferences.GetReferencesOrDefault();
        }

        public void UpdateAssemblyReferences(IEnumerable<string> references)
        {
            if(this.editorReferences == null)
            {
                GetAssemblyReferences();
            }
            this.editorReferences.AddReferences(references);
            serializer.Serialize<AssemblyReferences>(ReferencesFile, this.editorReferences);
        }

        public string[] GetKnownDirectories()
        {
            return new string[]
            {
                Environment.CurrentDirectory,
                Path.Combine(Environment.CurrentDirectory, string.Empty),
                Path.Combine(Environment.CurrentDirectory, "Core"),
                Path.Combine(Environment.CurrentDirectory, "Components"),               
                Path.Combine(Environment.CurrentDirectory, "Roslyn")
            };
        }

    }
}
